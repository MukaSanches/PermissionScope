using PermissionScope.Core;

namespace PermissionScope.Windows;

/// <summary>Fictional descriptors evaluated by Windows Authz. Never creates accounts, files or ACLs.</summary>
public static class DemoFixture
{
    public const string Version = "permissionscope-demo-v1";
    public const string Root = @"C:\PermissionScope-Demo";
    public const string UserSid = "S-1-5-21-111111111-222222222-333333333-1001";
    public const string FinanceSid = "S-1-5-21-111111111-222222222-333333333-2001";
    public const string EmployeesSid = "S-1-5-21-111111111-222222222-333333333-2002";
    public static string Name(string sid) => sid switch
    {
        UserSid => @"LAB\Alex", FinanceSid => @"LAB\Finance", EmployeesSid => @"LAB\Employees",
        "S-1-5-18" => "SYSTEM", "S-1-1-0" => "Everyone", _ => sid
    };
    private static GroupGraph Graph()
    {
        var graph = new GroupGraph();
        graph.Add(new(UserSid, FinanceSid, "Synthetic fixture membership; supplied to Authz"));
        graph.Add(new(UserSid, EmployeesSid, "Synthetic fixture membership; supplied to Authz"));
        return graph;
    }
    public static AccessDecision Evaluate(string sddl, string path, bool remote = false)
    {
        using var access = new EffectiveAccessResolver(new IdentityResolver(), UserSid, true, [FinanceSid, EmployeesSid]);
        var descriptor = DescriptorParser.Parse(sddl, Name);
        var mask = access.CheckDiscretionary(sddl);
        var identity = access.Identity with { Identity = new(UserSid, Name(UserSid), "Synthetic fixture", true) };
        var state = remote ? AccessState.Unknown : mask == Rights.FullControl ? AccessState.Granted : mask == 0 ? AccessState.Denied : AccessState.Partial;
        return new(UserSid, Name(UserSid), state, mask, remote ? Rights.Read : null,
            "Synthetic demonstration. Windows Authz evaluates supplied fixture SIDs and descriptors; this is not an actual user logon or file-open test.",
            remote ? "Synthetic remote example: the server logon token is not established." : null,
            AccessPathBuilder.Explain(descriptor, identity, mask, path, Graph()));
    }
    public static PermissionSnapshot Create(bool after = false)
    {
        var observed = new DateTimeOffset(2026, 1, after ? 2 : 1, 10, 0, 0, TimeSpan.Zero);
        var resources = new List<ResourceAccess>();
        void Add(string leaf, string aces, bool remote = false)
        {
            var path = remote ? @"\\LAB-FILESERVER\Shared" : Root + "\\" + leaf;
            var descriptor = DescriptorParser.Parse("O:SYG:SYD:P" + aces, Name);
            var share = remote ? new ShareInfo("LAB-FILESERVER", "Shared", null, descriptor, "Synthetic server; no connection is made") : null;
            resources.Add(new(path, true, false, observed, descriptor, share, Evaluate(descriptor.Sddl, path, remote), FindingRules.Evaluate(path, descriptor), null));
        }
        Add("Finance", $"(A;;{(after ? "FR" : "0x1301BF")};;;{FinanceSid})");
        Add("Projects", $"(A;;FA;;;{EmployeesSid})");
        Add("Shared", $"(A;;FR;;;{EmployeesSid})");
        Add("Private", $"(D;;FA;;;{UserSid})(A;;FR;;;{EmployeesSid})");
        Add("Remote", $"(A;;FR;;;{EmployeesSid})", true);
        return new(PermissionSnapshot.CurrentSchema, "1.0.0", after ? "11111111111111111111111111111112" : "11111111111111111111111111111111",
            Root, observed, observed.AddSeconds(1), false, UserSid, resources, Graph().Edges)
        { FixtureId = Version, IdentityWarnings = ["Synthetic demonstration: LAB identities, resource names, descriptors and timestamps are fictional. No machine inventory was collected."] };
    }
}
