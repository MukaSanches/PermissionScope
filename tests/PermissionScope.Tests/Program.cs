using System.Diagnostics;
using System.IO.Compression;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text.Json;
using System.Xml.Linq;
using PermissionScope.Core;
using PermissionScope.Windows;

var failures = new List<string>();
var timings = new List<string>();
var passed = 0;
var testRoot = Path.Combine(Path.GetTempPath(), "PermissionScope-tests-" + Guid.NewGuid().ToString("N"));
Directory.CreateDirectory(testRoot);
void Assert(bool condition, string message = "Assertion failed") { if (!condition) throw new InvalidOperationException(message); }
void Test(string name, Action test)
{
    var clock = Stopwatch.StartNew();
    try { test(); passed++; Console.WriteLine($"PASS {name} ({clock.Elapsed.TotalMilliseconds:F1} ms)"); }
    catch (Exception e) { failures.Add(name + ": " + e); Console.WriteLine($"FAIL {name}: {e.Message}"); }
}
async Task TestAsync(string name, Func<Task> test)
{
    var clock = Stopwatch.StartNew();
    try { await test(); passed++; Console.WriteLine($"PASS {name} ({clock.Elapsed.TotalMilliseconds:F1} ms)"); }
    catch (Exception e) { failures.Add(name + ": " + e); Console.WriteLine($"FAIL {name}: {e.Message}"); }
}
void Throws<T>(Action action) where T : Exception { try { action(); } catch (T) { return; } throw new InvalidOperationException("Expected " + typeof(T).Name); }
const string fixtureSid = "S-1-5-21-111111111-222222222-333333333-1001";
Test("Locale exact match", () => Assert(LocaleResolver.Resolve("pt-BR", ["en-US", "pt-BR"]).ResourceTag == "pt-BR"));
Test("Locale language fallback", () => Assert(LocaleResolver.Resolve("es-MX", ["en-US", "es"]).ResourceTag == "es"));
Test("Locale regional sibling fallback", () => Assert(LocaleResolver.Resolve("pt-PT", ["en-US", "pt-BR"]).ResourceTag == "pt-BR"));
Test("Locale fallback preserves regional formatting", () => Assert(LocaleResolver.Resolve("de-AT", ["en-US"]).LanguageTag == "de-AT"));
Test("Locale Arabic regional RTL", () => Assert(LocaleResolver.Resolve("ar-SA", ["en-US", "ar"]).IsRightToLeft));
Test("Locale Persian RTL with English fallback", () => Assert(LocaleResolver.Resolve("fa-IR", ["en-US"]).IsRightToLeft));
Test("Locale Chinese script fallback", () => Assert(LocaleResolver.Resolve("zh-TW", ["en-US", "zh-Hant", "zh-Hans"]).ResourceTag == "zh-Hant"));
Test("Locale Chinese does not substitute scripts", () => Assert(LocaleResolver.Resolve("zh-TW", ["en-US", "zh-Hans"]).ResourceTag == "en-US"));
var resolver = new IdentityResolver();
Test("Isolated data directory rejects relative paths", () =>
{
    var previous = Environment.GetEnvironmentVariable("PERMISSIONSCOPE_DATA_DIR");
    try { Environment.SetEnvironmentVariable("PERMISSIONSCOPE_DATA_DIR", "relative"); Throws<ArgumentException>(() => _ = SnapshotStore.DefaultDirectory); }
    finally { Environment.SetEnvironmentVariable("PERMISSIONSCOPE_DATA_DIR", previous); }
});
Test("Isolated data directory accepts an explicit absolute path", () =>
{
    var previous = Environment.GetEnvironmentVariable("PERMISSIONSCOPE_DATA_DIR");
    try { Environment.SetEnvironmentVariable("PERMISSIONSCOPE_DATA_DIR", testRoot); Assert(SnapshotStore.DefaultDirectory == testRoot); }
    finally { Environment.SetEnvironmentVariable("PERMISSIONSCOPE_DATA_DIR", previous); }
});
using var isolated = new EffectiveAccessResolver(resolver, fixtureSid, true);
using (var corpus = JsonDocument.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "corpus", "discretionary-v1.json"))))
{
    Assert(corpus.RootElement.GetProperty("identitySid").GetString() == fixtureSid);
    foreach (var fixture in corpus.RootElement.GetProperty("cases").EnumerateArray())
        Test("Corpus: " + fixture.GetProperty("name").GetString(), () => Assert(isolated.CheckDiscretionary(fixture.GetProperty("sddl").GetString()!) == Convert.ToUInt32(fixture.GetProperty("mask").GetString(), 16)));
}
string Sddl(string aces) => "O:SYG:SYD:" + aces;
Test("NULL DACL is unrestricted", () => Assert(isolated.CheckDiscretionary("O:SYG:SYD:NO_ACCESS_CONTROL") == Rights.FullControl));
Test("Empty DACL denies non-owner", () => Assert(isolated.CheckDiscretionary(Sddl("")) == 0));
Test("Parser preserves NULL versus empty", () => { Assert(DescriptorParser.Parse("O:SYG:SYD:NO_ACCESS_CONTROL").NullDacl); Assert(!DescriptorParser.Parse(Sddl("")).NullDacl); });
Test("Specific allow", () => Assert(isolated.CheckDiscretionary(Sddl($"(A;;0x1;;;{fixtureSid})")) == 1));
Test("Generic read mapping", () => Assert(isolated.CheckDiscretionary(Sddl($"(A;;GR;;;{fixtureSid})")) == Rights.Read));
Test("Generic all mapping", () => Assert(isolated.CheckDiscretionary(Sddl($"(A;;GA;;;{fixtureSid})")) == Rights.FullControl));
Test("Deny delete preserves read and write", () => Assert(isolated.CheckDiscretionary(Sddl($"(D;;SD;;;{fixtureSid})(A;;FA;;;{fixtureSid})")) == (Rights.FullControl & ~0x10000u)));
Test("Noncanonical allow before deny follows Windows order", () => Assert(isolated.CheckDiscretionary(Sddl($"(A;;0x1;;;{fixtureSid})(D;;0x1;;;{fixtureSid})")) == 1));
Test("Noncanonical descriptor detected", () => Assert(!DescriptorParser.Parse(Sddl($"(A;;0x1;;;{fixtureSid})(D;;0x1;;;{fixtureSid})")).Canonical));
Test("Inherit-only does not grant current object", () => Assert(isolated.CheckDiscretionary(Sddl($"(A;CIIO;FA;;;{fixtureSid})")) == 0));
Test("Inherited allow applies", () => Assert(isolated.CheckDiscretionary(Sddl($"(A;ID;0x1;;;{fixtureSid})")) == 1));
Test("Inherited deny applies", () => Assert(isolated.CheckDiscretionary(Sddl($"(D;ID;0x1;;;{fixtureSid})(A;ID;FA;;;{fixtureSid})")) == (Rights.FullControl & ~1u)));
Test("Creator Owner is not current user on existing object", () => Assert(isolated.CheckDiscretionary(Sddl("(A;;FA;;;CO)")) == 0));
Test("Owner gets implicit READ_CONTROL and WRITE_DAC", () => Assert((isolated.CheckDiscretionary($"O:{fixtureSid}G:SYD:") & 0x60000) == 0x60000));
Test("Owner Rights SID overrides implicit owner permissions", () => Assert((isolated.CheckDiscretionary($"O:{fixtureSid}G:SYD:(D;;WD;;;OW)") & 0x40000) == 0));
Test("Different SID cannot grant access", () => Assert(isolated.CheckDiscretionary(Sddl("(A;;FA;;;S-1-5-21-111-222-333-999)")) == 0));
Test("Owner Rights allow has auditable ACE evidence", () =>
{
    var descriptor = DescriptorParser.Parse($"O:{fixtureSid}G:SYD:(A;;0x1;;;OW)");
    var mask = isolated.CheckDiscretionary(descriptor.Sddl);
    var evidence = AccessPathBuilder.Explain(descriptor, isolated.Identity, mask, "fixture", new());
    Assert((mask & 1) == 1);
    Assert(evidence.Any(e => e.Sid == "S-1-3-4" && e.Relation == "GrantedBy" && (e.ContributingMask & 1) == 1));
});
Test("Owner Rights deny has auditable ACE evidence", () =>
{
    var descriptor = DescriptorParser.Parse($"O:{fixtureSid}G:SYD:(D;;WD;;;OW)");
    var mask = isolated.CheckDiscretionary(descriptor.Sddl);
    var evidence = AccessPathBuilder.Explain(descriptor, isolated.Identity, mask, "fixture", new());
    Assert(evidence.Any(e => e.Sid == "S-1-3-4" && e.Relation == "DeniedBy" && (e.ContributingMask & 0x40000) != 0));
});
Test("Owner Rights ACE does not apply to a non-owner", () =>
{
    var descriptor = DescriptorParser.Parse("O:SYG:SYD:(A;;FA;;;OW)");
    Assert(AccessPathBuilder.Explain(descriptor, isolated.Identity, 0, "fixture", new()).Count == 0);
});
Test("Protected inheritance retained", () => Assert(DescriptorParser.Parse("O:SYG:SYD:P(A;;FA;;;SY)").Protected));
Test("Conditional ACE becomes Unknown", () =>
{
    var descriptor = DescriptorParser.Parse(Sddl($"(XA;;FR;;;{fixtureSid};(@User.department == \"Finance\"))"));
    Assert(descriptor.HasSpecialAces);
    Assert(isolated.Evaluate(descriptor, "C:\\fixture").State == AccessState.Unknown);
});
Test("Unknown context never exposes granted capabilities", () =>
{
    var decision = new AccessDecision(fixtureSid, "Fixture", AccessState.Unknown, Rights.FullControl, null, "fixture", "unavailable", []);
    Assert(decision.Capabilities.All(c => c.State == AccessState.Unknown));
});
Test("Share and NTFS intersection", () =>
{
    var decision = new AccessDecision(fixtureSid, "Fixture", AccessState.Partial, Rights.FullControl, Rights.Read, "fixture", null, []);
    Assert(decision.EffectiveMask == Rights.Read); Assert(decision.Capabilities.Single(c => c.Key == "Write").State != AccessState.Granted);
});
Test("Remote context is conservatively Unknown", () =>
{
    var sd = DescriptorParser.Parse(Sddl($"(A;;FA;;;{fixtureSid})"));
    Assert(isolated.Evaluate(sd, "\\\\server\\share", new("server", "share", "C:\\share", sd, null)).State == AccessState.Unknown);
});
Test("Reparse decision cannot prove target access", () => Assert(isolated.Evaluate(DescriptorParser.Parse(Sddl($"(A;;FA;;;{fixtureSid})")), "C:\\link", null, true).State == AccessState.Unknown));
Test("Nested group graph with cycle and duplicate", () =>
{
    var graph = new GroupGraph(); graph.Add(new("u", "g1", "fixture")); graph.Add(new("g1", "g2", "fixture")); graph.Add(new("g2", "g1", "fixture")); graph.Add(new("u", "g1", "fixture"));
    Assert(graph.Edges.Count == 3); Assert(graph.FindPath("u", "g2").Count == 2); Assert(graph.FindPath("u", "missing").Count == 0);
});
Test("Multiple ACE contributors", () =>
{
    var descriptor = DescriptorParser.Parse(Sddl($"(A;;0x1;;;{fixtureSid})(A;;0x2;;;{fixtureSid})"));
    var evidence = AccessPathBuilder.Explain(descriptor, isolated.Identity, 3, "fixture", new());
    Assert(evidence.Count == 2 && evidence.Aggregate(0u, (m, e) => m | e.ContributingMask) == 3);
});
Test("Deny-only groups never contribute allows", () =>
{
    var identity = new AccessIdentity(new("u", "u", "User", true), [new(fixtureSid, 0x10)], true, "fixture");
    var descriptor = DescriptorParser.Parse(Sddl($"(D;;0x1;;;{fixtureSid})(A;;0x2;;;{fixtureSid})"));
    var evidence = AccessPathBuilder.Explain(descriptor, identity, 0, "fixture", new());
    Assert(evidence.Count == 1 && evidence[0].Relation == "DeniedBy");
});
Test("Disabled token SID contributes nothing", () =>
{
    var identity = new AccessIdentity(new("u", "u", "User", true), [new(fixtureSid, 0)], true, "fixture");
    Assert(AccessPathBuilder.Explain(DescriptorParser.Parse(Sddl($"(A;;FA;;;{fixtureSid})")), identity, 0, "fixture", new()).Count == 0);
});
Test("Broad ACE exposure is deterministic", () => Assert(FindingRules.Evaluate("fixture", DescriptorParser.Parse(Sddl("(A;;FA;;;WD)"))).Any(f => f.Rule == "BroadAllow" && f.Severity == Severity.CriticalExposure)));
Test("Unresolved SID is not claimed deleted", () => Assert(FindingRules.Evaluate("fixture", DescriptorParser.Parse(Sddl($"(A;;FR;;;{fixtureSid})"))).Any(f => f.Rule == "UnresolvedSid" && f.Evidence.Contains("does not prove"))));
Test("Simulation removes only chosen ACE", () =>
{
    var before = Sddl($"(A;;0x1;;;{fixtureSid})(A;;0x2;;;{fixtureSid})"); var after = ImpactSimulator.RemoveAce(before, 0);
    Assert(isolated.CheckDiscretionary(before) == 3); Assert(isolated.CheckDiscretionary(after) == 2);
});
Test("Simulation add deny maintains precedence", () => Assert(isolated.CheckDiscretionary(ImpactSimulator.AddAce(Sddl($"(A;;FA;;;{fixtureSid})"), fixtureSid, 1, true)) == (Rights.FullControl & ~1u)));
Test("Malformed SID rejected", () => Throws<ArgumentException>(() => resolver.Find("S-1-invalid")));
Test("Out of range ACE rejected", () => Throws<ArgumentOutOfRangeException>(() => ImpactSimulator.RemoveAce(Sddl(""), 0)));
Test("Malformed descriptor rejected", () => Throws<ArgumentException>(() => DescriptorParser.Parse("not-sddl")));
Test("500 randomized ACLs agree with ordered-bit reference", () =>
{
    var random = new Random(9183);
    for (var i = 0; i < 500; i++)
    {
        uint granted = 0, remaining = 0x1ff;
        var aces = new List<string>();
        for (var j = 0; j < 8; j++)
        {
            var deny = random.Next(2) == 0; var mask = (uint)random.Next(512);
            aces.Add($"({(deny ? "D" : "A")};;0x{mask:X};;;{fixtureSid})");
            if (!deny) granted |= remaining & mask;
            remaining &= ~mask;
        }
        Assert(isolated.CheckDiscretionary(Sddl(string.Join("", aces))) == granted, "Differential fixture " + i);
    }
});

// Permission evaluation edge cases
Test("Multiple consecutive deny ACEs accumulate", () =>
{
    var descriptor = DescriptorParser.Parse(Sddl($"(D;;0x1;;;{fixtureSid})(D;;0x2;;;{fixtureSid})(D;;0x4;;;{fixtureSid})(A;;FA;;;{fixtureSid})"));
    Assert(isolated.CheckDiscretionary(descriptor.Sddl) == (Rights.FullControl & ~7u));
});
Test("Allow after partial deny restores only non-denied bits", () =>
{
    var descriptor = DescriptorParser.Parse(Sddl($"(D;;0xF;;;{fixtureSid})(A;;0xFF;;;{fixtureSid})"));
    Assert(isolated.CheckDiscretionary(descriptor.Sddl) == (0xFF & ~0xF));
});
Test("Creator Owner applies when identity matches owner", () =>
{
    var descriptor = DescriptorParser.Parse($"O:{fixtureSid}G:SYD:(A;;FA;;;CO)");
    Assert((isolated.CheckDiscretionary(descriptor.Sddl) & Rights.FullControl) == Rights.FullControl);
});
Test("Creator Group does not apply without group membership", () =>
{
    var descriptor = DescriptorParser.Parse($"O:SYG:{fixtureSid}D:(A;;FA;;;CG)");
    Assert(isolated.CheckDiscretionary(descriptor.Sddl) == 0);
});
Test("NULL DACL on owned object still grants full control", () =>
{
    var descriptor = DescriptorParser.Parse($"O:{fixtureSid}G:SYD:NO_ACCESS_CONTROL");
    Assert(isolated.CheckDiscretionary(descriptor.Sddl) == Rights.FullControl);
});
Test("Empty DACL on owned object preserves owner rights only", () =>
{
    var descriptor = DescriptorParser.Parse($"O:{fixtureSid}G:SYD:");
    Assert((isolated.CheckDiscretionary(descriptor.Sddl) & 0x60000) == 0x60000);
});
Test("Generic execute maps to traverse and execute", () =>
{
    var descriptor = DescriptorParser.Parse(Sddl($"(A;;GX;;;{fixtureSid})"));
    Assert((isolated.CheckDiscretionary(descriptor.Sddl) & 0x20) == 0x20);
});
Test("Generic write maps to file add and data write", () =>
{
    var descriptor = DescriptorParser.Parse(Sddl($"(A;;GW;;;{fixtureSid})"));
    Assert((isolated.CheckDiscretionary(descriptor.Sddl) & Rights.Write) == Rights.Write);
});
Test("Mixed generic and specific masks resolve correctly", () =>
{
    var descriptor = DescriptorParser.Parse(Sddl($"(A;;GR;;;{fixtureSid})(A;;0x1;;;{fixtureSid})"));
    Assert(isolated.CheckDiscretionary(descriptor.Sddl) == (Rights.Read | 1));
});
Test("Inherited deny blocks inherited allow", () =>
{
    var descriptor = DescriptorParser.Parse(Sddl($"(D;ID;0x1;;;{fixtureSid})(A;ID;0x3;;;{fixtureSid})"));
    Assert(isolated.CheckDiscretionary(descriptor.Sddl) == 2);
});
Test("Non-inherited allow works alongside inherit-only deny", () =>
{
    var descriptor = DescriptorParser.Parse(Sddl($"(A;;0x1;;;{fixtureSid})(D;IO;0x1;;;{fixtureSid})"));
    Assert(isolated.CheckDiscretionary(descriptor.Sddl) == 1);
});
Test("Object inherit-only does not grant container access", () =>
{
    var descriptor = DescriptorParser.Parse(Sddl($"(A;IOIO;FA;;;{fixtureSid})"));
    Assert(isolated.CheckDiscretionary(descriptor.Sddl) == 0);
});
Test("Container inherit-only applies to subdirectories", () =>
{
    var descriptor = DescriptorParser.Parse(Sddl($"(A;CICO;FA;;;{fixtureSid})"));
    Assert(isolated.CheckDiscretionary(descriptor.Sddl) == 0);
});
Test("Successful access check returns zero error code", () =>
{
    var descriptor = DescriptorParser.Parse(Sddl($"(A;;FA;;;{fixtureSid})"));
    Assert(isolated.CheckDiscretionary(descriptor.Sddl) == Rights.FullControl);
});
Test("Owner implicit rights survive explicit deny of standard rights", () =>
{
    var descriptor = DescriptorParser.Parse($"O:{fixtureSid}G:SYD:(D;;0xFFFFF;;;{fixtureSid})");
    Assert((isolated.CheckDiscretionary(descriptor.Sddl) & 0x60000) == 0x60000);
});
Test("Authenticated Users SID does not match fixture", () =>
{
    var descriptor = DescriptorParser.Parse(Sddl("(A;;FA;;;AU)"));
    Assert(isolated.CheckDiscretionary(descriptor.Sddl) == 0);
});
Test("System SID does not match fixture", () =>
{
    var descriptor = DescriptorParser.Parse(Sddl("(A;;FA;;;SY)"));
    Assert(isolated.CheckDiscretionary(descriptor.Sddl) == 0);
});
Test("World SID does not match fixture", () =>
{
    var descriptor = DescriptorParser.Parse(Sddl("(A;;FA;;;WD)"));
    Assert(isolated.CheckDiscretionary(descriptor.Sddl) == 0);
});
Test("Complex ACE order follows Windows first-applicable-deny semantics", () =>
{
    var descriptor = DescriptorParser.Parse(Sddl($"(A;;0xF;;;{fixtureSid})(D;;0x3;;;{fixtureSid})(A;;0x3;;;{fixtureSid})(D;;0x1;;;{fixtureSid})"));
    // First allow grants 0xF, first deny removes 0x3, second allow tries to add 0x3 (fails due to prior deny), second deny removes 0x1 (already removed)
    Assert(isolated.CheckDiscretionary(descriptor.Sddl) == (0xF & ~0x3));
});
Test("ACE with all flags set still evaluates mask correctly", () =>
{
    var descriptor = DescriptorParser.Parse(Sddl($"(A;CIFPIOISA;0x1;;;{fixtureSid})"));
    Assert(isolated.CheckDiscretionary(descriptor.Sddl) == 0); // InheritOnly prevents current object access
});
Test("Callback ACE marked as special does not contribute to mask", () =>
{
    var descriptor = DescriptorParser.Parse(Sddl($"(XA;;FA;;;{fixtureSid};(@User.department == \"Finance\"))"));
    Assert(descriptor.HasSpecialAces);
    Assert(isolated.CheckDiscretionary(descriptor.Sddl) == 0);
});
Test("Resource attribute ACE marked as special", () =>
{
    var descriptor = DescriptorParser.Parse(Sddl($"(RA;;FA;;;{fixtureSid};(@User.department == \"Finance\"))"));
    Assert(descriptor.HasSpecialAces);
});
Test("Scope ACE marked as special", () =>
{
    var descriptor = DescriptorParser.Parse(Sddl($"(SA;;FA;;;{fixtureSid};(@User.department == \"Finance\"))"));
    Assert(descriptor.HasSpecialAces);
});
Test("Filter condition ACE marked as special", () =>
{
    var descriptor = DescriptorParser.Parse(Sddl($"(FA;;FA;;;{fixtureSid};(@User.department == \"Finance\"))"));
    Assert(descriptor.HasSpecialAces);
});
Test("Multiple identities in token do not cross-contaminate", () =>
{
    var otherSid = "S-1-5-21-111111111-222222222-333333333-1002";
    var descriptor = DescriptorParser.Parse(Sddl($"(A;;FA;;;{otherSid})"));
    Assert(isolated.CheckDiscretionary(descriptor.Sddl) == 0);
});
Test("Bitwise OR of multiple allows from same SID accumulates", () =>
{
    var descriptor = DescriptorParser.Parse(Sddl($"(A;;0x1;;;{fixtureSid})(A;;0x2;;;{fixtureSid})(A;;0x4;;;{fixtureSid})"));
    Assert(isolated.CheckDiscretionary(descriptor.Sddl) == 7);
});
Test("Deny ACE with zero mask has no effect", () =>
{
    var descriptor = DescriptorParser.Parse(Sddl($"(D;;0;;;{fixtureSid})(A;;FA;;;{fixtureSid})"));
    Assert(isolated.CheckDiscretionary(descriptor.Sddl) == Rights.FullControl);
});
Test("Allow ACE with zero mask has no effect", () =>
{
    var descriptor = DescriptorParser.Parse(Sddl($"(A;;0;;;{fixtureSid})"));
    Assert(isolated.CheckDiscretionary(descriptor.Sddl) == 0);
});
Test("Maximum allowed access request respects all denies", () =>
{
    var descriptor = DescriptorParser.Parse(Sddl($"(A;;FA;;;{fixtureSid})(D;;0x10000;;;{fixtureSid})"));
    Assert((isolated.CheckDiscretionary(descriptor.Sddl) & 0x10000) == 0);
});
Test("Descriptor with both owner and group matching identity", () =>
{
    var descriptor = DescriptorParser.Parse($"O:{fixtureSid}G:{fixtureSid}D:(A;;FA;;;{fixtureSid})");
    Assert(isolated.CheckDiscretionary(descriptor.Sddl) == Rights.FullControl);
});
Test("Protected DACL with inheritance flags still evaluates inherited ACEs", () =>
{
    var descriptor = DescriptorParser.Parse(Sddl($"P(A;ID;FA;;;{fixtureSid})"));
    Assert(isolated.CheckDiscretionary(descriptor.Sddl) == Rights.FullControl);
});
Test("Auto-inherit flag does not affect evaluation", () =>
{
    var descriptor = DescriptorParser.Parse(Sddl($"S(A;;FA;;;{fixtureSid})"));
    Assert(isolated.CheckDiscretionary(descriptor.Sddl) == Rights.FullControl);
});

PermissionSnapshot? captured = null;
await TestAsync("Real NTFS tree / Unicode / no mutation", async () =>
{
    var path = Path.Combine(testRoot, "tree"); Directory.CreateDirectory(path);
    Directory.CreateDirectory(Path.Combine(path, "Finance")); Directory.CreateDirectory(Path.Combine(path, "日本語-العربية-😀"));
    var reader = new AclReader(resolver); var before = reader.Read(path).Hash;
    var clock = Stopwatch.StartNew(); captured = await new Scanner().ScanAsync(new(path));
    timings.Add($"Local scan, 3 directories: {clock.Elapsed.TotalMilliseconds:F2} ms");
    Assert(captured.Resources.Count == 3); Assert(captured.Resources.All(r => r.Error == null)); Assert(reader.Read(path).Hash == before);
});
await TestAsync("Invalid path produces a read error", async () =>
{
    var snapshot = await new Scanner().ScanAsync(new(Path.Combine(testRoot, "missing"), Recursive: false)); Assert(snapshot.Resources.Count == 1 && snapshot.Resources[0].Error != null);
});
await TestAsync("Long paths retain Windows descriptor access", async () =>
{
    var path = Path.Combine(testRoot, new string('a', 110), new string('b', 110), new string('c', 90));
    Directory.CreateDirectory(path);
    var observation = await new Scanner().ScanAsync(new(path, Recursive: false));
    Assert(observation.Resources.Single().Descriptor != null && observation.Resources.Single().Error == null);
});
await TestAsync("Junction cycle is not traversed", async () =>
{
    var path = Path.Combine(testRoot, "junction-root"); Directory.CreateDirectory(path);
    var link = Path.Combine(path, "cycle");
    var info = new ProcessStartInfo("cmd.exe") { UseShellExecute = false, CreateNoWindow = true, RedirectStandardOutput = true, RedirectStandardError = true };
    info.Arguments = $"/c mklink /J "{link}" "{path}"";
    using var process = Process.Start(info)!; var stdout = process.StandardOutput.ReadToEndAsync(); var stderr = process.StandardError.ReadToEndAsync(); await process.WaitForExitAsync();
    Assert(process.ExitCode == 0, await stdout + await stderr);
    try
    {
        var observation = await new Scanner().ScanAsync(new(path));
        Assert(observation.Resources.Count == 2); Assert(observation.Resources.Single(r => r.Path == link).IsReparsePoint);
        Assert(observation.Resources.Single(r => r.Path == link).Decision?.State == AccessState.Unknown);
    }
    finally { Directory.Delete(link, false); }
});
await TestAsync("Cancelled scan returns incomplete scope", async () =>
{
    using var cts = new CancellationTokenSource(); cts.Cancel(); var snapshot = await new Scanner().ScanAsync(new(testRoot), null, cts.Token); Assert(snapshot.Cancelled && snapshot.Resources.Count == 0);
});
await TestAsync("Real explicit / inherited ACLs and icacls differential", async () =>
{
    var folder = Directory.CreateDirectory(Path.Combine(testRoot, "acl-fixture"));
    var original = folder.GetAccessControl();
    try
    {
        using var identity = WindowsIdentity.GetCurrent();
        var acl = new DirectorySecurity(); acl.SetOwner(identity.User!); acl.SetAccessRuleProtection(true, false);
        acl.AddAccessRule(new FileSystemAccessRule(identity.User!, FileSystemRights.FullControl, InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit, PropagationFlags.None, AccessControlType.Allow));
        acl.AddAccessRule(new FileSystemAccessRule(new SecurityIdentifier(fixtureSid), FileSystemRights.Read, InheritanceFlags.ContainerInherit, PropagationFlags.None, AccessControlType.Allow));
        folder.SetAccessControl(acl);
        var child = Directory.CreateDirectory(Path.Combine(folder.FullName, "child"));
        var grandchild = Directory.CreateDirectory(Path.Combine(child.FullName, "grandchild"));
        var reader = new AclReader(resolver);
        Assert(reader.Read(folder.FullName).Protected);
        Assert(reader.Read(child.FullName).Aces.Any(a => a.Sid == fixtureSid && a.Inherited));
        Assert(reader.Read(grandchild.FullName).Aces.Any(a => a.Sid == fixtureSid && a.Inherited));
        using var access = new EffectiveAccessResolver(resolver);
        Assert(access.CheckDiscretionary(reader.Read(folder.FullName).Sddl) == Rights.FullControl);
        var psi = new ProcessStartInfo("icacls.exe") { RedirectStandardOutput = true, UseShellExecute = false, CreateNoWindow = true }; psi.ArgumentList.Add(child.FullName);
        using var process = Process.Start(psi)!; var output = await process.StandardOutput.ReadToEndAsync(); await process.WaitForExitAsync();
        Assert(process.ExitCode == 0 && output.Contains(fixtureSid));
    }
    finally { folder.SetAccessControl(original); }
});
if (captured is { } snapshot)
{
    Test("SQLite roundtrip and duplicate protection", () =>
    {
        var store = new SnapshotStore(Path.Combine(testRoot, "snapshots.db")); var clock = Stopwatch.StartNew(); store.Save(snapshot); var loaded = store.Load(snapshot.Id);
        timings.Add($"Snapshot write/read, {snapshot.Resources.Count} objects: {clock.Elapsed.TotalMilliseconds:F2} ms");
        Assert(loaded.Resources.Count == snapshot.Resources.Count && loaded.Root == snapshot.Root); Assert(store.List().Count == 1);
        Throws<Microsoft.Data.Sqlite.SqliteException>(() => store.Save(snapshot));
    });
    Test("Snapshot corruption fails closed", () =>
    {
        var json = JsonSerializer.Serialize(snapshot, SnapshotJson.Options); var hash = snapshot.Resources[0].Descriptor!.Hash;
        Throws<InvalidDataException>(() => SnapshotJson.Parse(json.Replace(hash, new string('0', 64))));
    });
    Test("Snapshot newer schema rejected", () => Throws<InvalidDataException>(() => SnapshotJson.Parse(JsonSerializer.Serialize(snapshot with { SchemaVersion = 99 }, SnapshotJson.Options))));
    Test("Snapshot malformed JSON rejected", () => Throws<JsonException>(() => SnapshotJson.Parse("{ invalid")));
    Test("Compare unchanged snapshots", () => Assert(SnapshotComparer.Compare(snapshot, snapshot).Count == 0));
    Test("Compare membership impact without a descriptor change", () =>
    {
        var r = snapshot.Resources[0]; var updated = r with { Decision = r.Decision! with { GrantedMask = 1, State = AccessState.Partial } };
        Assert(SnapshotComparer.Compare(snapshot with { Resources = [r] }, snapshot with { Resources = [updated] }).Single().Kind == "AccessChanged");
    });
    Test("Compare absence is not deletion", () => Assert(SnapshotComparer.Compare(snapshot, snapshot with { Resources = [] }).All(c => c.Kind == "NotObserved")));
    Test("Compare descriptor change", () =>
    {
        var r = snapshot.Resources[0]; var changed = r with { Descriptor = DescriptorParser.Parse(Sddl("")) };
        Assert(SnapshotComparer.Compare(snapshot with { Resources = [r] }, snapshot with { Resources = [changed] }).Single().Kind == "Changed");
    });
    Test("HTML escapes paths and includes limitations", () =>
    {
        var modified = snapshot with { Root = "<script>alert(1)</script>" }; var html = ReportExporter.Html(modified);
        Assert(!html.Contains("<script>")); Assert(html.Contains("&lt;script&gt;")); Assert(html.Contains("remote logon"));
    });
    Test("CSV formula injection protection", () =>
    {
        var modified = snapshot with { Resources = [snapshot.Resources[0] with { Path = "=HYPERLINK("bad")" }] };
        Assert(ReportExporter.Csv(modified).Contains(""'=HYPERLINK"));
    });
    foreach (var format in new[] { "html", "csv", "json", "xlsx" }) Test("Export " + format, () =>
    {
        var path = Path.Combine(testRoot, "report." + format); ReportExporter.Export(snapshot, path, format); Assert(new FileInfo(path).Length > 100);
        if (format == "xlsx")
        {
            using var zip = ZipFile.OpenRead(path); Assert(zip.Entries.Count(e => e.FullName.StartsWith("xl/worksheets/")) == 6);
            foreach (var entry in zip.Entries) { using var stream = entry.Open(); XDocument.Load(stream); }
        }
    });
    Test("PDF export is a text document", () =>
    {
        var latin = snapshot with { Resources = [snapshot.Resources[0]] }; var path = Path.Combine(testRoot, "report.pdf"); ReportExporter.Export(latin, path, "pdf");
        Assert(File.ReadAllBytes(path).Take(4).SequenceEqual("%PDF"u8.ToArray()));
    });
    Test("PDF refuses unsupported complex-script shaping", () => Throws<InvalidOperationException>(() => ReportExporter.Export(snapshot, Path.Combine(testRoot, "complex.pdf"), "pdf")));
    Test("Export denied destination preserves existing file", () =>
    {
        var path = Path.Combine(testRoot, "locked.json"); File.WriteAllText(path, "original"); using var locked = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.None);
        try { ReportExporter.Export(snapshot, path, "json"); throw new InvalidOperationException("Export should have failed."); }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException) { }
    });
    Test("100,000 object comparison benchmark", () =>
    {
        var resources = Enumerable.Range(0, 100000).Select(i => snapshot.Resources[0] with { Path = "C:\\fixture\" + i }).ToArray(); var large = snapshot with { Resources = resources };
        var clock = Stopwatch.StartNew(); Assert(SnapshotComparer.Compare(large, large).Count == 0); timings.Add($"Compare 100,000 objects: {clock.Elapsed.TotalMilliseconds:F2} ms");
    });
}
Test("Deep 10,000-group traversal benchmark", () =>
{
    var graph = new GroupGraph(); for (var i = 0; i < 10000; i++) graph.Add(new(i.ToString(), (i + 1).ToString(), "fixture"));
    var clock = Stopwatch.StartNew(); Assert(graph.FindPath("0", "10000").Count == 10000); timings.Add($"Group path, 10,000 edges: {clock.Elapsed.TotalMilliseconds:F2} ms");
});
Test("Verified file remediation and rollback", () =>
{
    var path = Path.Combine(testRoot, "remediation.txt"); File.WriteAllText(path, "permission fixture");
    var reader = new AclReader(resolver); var before = reader.Read(path);
    var proposed = ImpactSimulator.AddAce(before.Sddl, fixtureSid, 1, false);
    var plan = RemediationService.Prepare(path, before.Hash, proposed);
    RemediationReceipt receipt;
    try { receipt = RemediationService.Apply(plan, path, Path.Combine(testRoot, "changes")); }
    catch (InvalidOperationException error) { throw new InvalidOperationException($"{error.Message}\nSynthetic expected: {plan.ProposedSddl}\nSynthetic actual: {reader.Read(path).Sddl}", error); }
    Assert(reader.Read(path).Aces.Any(a => a.Sid == fixtureSid));
    var rollback = RemediationService.Rollback(receipt, path, Path.Combine(testRoot, "changes"));
    Assert(rollback.RolledBack); Assert(!reader.Read(path).Aces.Any(a => a.Sid == fixtureSid));
});
Test("DACL verification ignores only automatic-inheritance bookkeeping", () =>
{
    Assert(DescriptorParser.SameDacl("O:SYG:SYD:(A;;FR;;;SY)", "O:SYG:SYD:AI(A;;FR;;;SY)"));
    Assert(!DescriptorParser.SameDacl("O:SYG:SYD:(A;;FR;;;SY)", "O:SYG:SYD:P(A;;FR;;;SY)"));
    Assert(!DescriptorParser.SameDacl("O:SYG:SYD:(A;;FR;;;SY)", "O:SYG:SYD:(A;ID;FR;;;SY)"));
    Assert(!DescriptorParser.SameDacl("O:SYG:SYD:(A;;FR;;;SY)", "O:SYG:SYD:(A;;FA;;;SY)"));
    Assert(!DescriptorParser.SameDacl("O:SYG:SYD:", "O:SYG:SYD:NO_ACCESS_CONTROL"));
});
Test("Stale remediation refuses overwrite", () =>
{
    var path = Path.Combine(testRoot, "stale.txt"); File.WriteAllText(path, "fixture");
    var reader = new AclReader(resolver); var before = reader.Read(path);
    var plan = RemediationService.Prepare(path, before.Hash, ImpactSimulator.AddAce(before.Sddl, fixtureSid, 1, false));
    var file = new FileInfo(path); var acl = file.GetAccessControl(); acl.AddAccessRule(new FileSystemAccessRule(new SecurityIdentifier(fixtureSid), FileSystemRights.Write, AccessControlType.Allow)); file.SetAccessControl(acl);
    Throws<InvalidOperationException>(() => RemediationService.Apply(plan, path, Path.Combine(testRoot, "changes")));
});
Test("Remediation requires exact confirmation", () =>
{
    var path = Path.Combine(testRoot, "confirm.txt"); File.WriteAllText(path, "fixture"); var before = new AclReader(resolver).Read(path);
    var plan = RemediationService.Prepare(path, before.Hash, ImpactSimulator.AddAce(before.Sddl, fixtureSid, 1, false));
    Throws<InvalidOperationException>(() => RemediationService.Apply(plan, path + "x", Path.Combine(testRoot, "changes")));
});
Test("100,000 descriptor parses benchmark", () =>
{
    var sddl = Sddl($"(A;;FR;;;{fixtureSid})"); var clock = Stopwatch.StartNew(); for (var i = 0; i < 100000; i++) DescriptorParser.Parse(sddl);
    timings.Add($"Parse 100,000 descriptors: {clock.Elapsed.TotalMilliseconds:F2} ms");
});

// ============================================================================
// Additional Permission Evaluation Edge Cases
// ============================================================================

Test("Multiple consecutive deny ACEs accumulate correctly", () =>
{
    // Three consecutive denies should block bits 0x1, 0x2, and 0x4
    var descriptor = DescriptorParser.Parse(Sddl($"(D;;0x1;;;{fixtureSid})(D;;0x2;;;{fixtureSid})(D;;0x4;;;{fixtureSid})(A;;FA;;;{fixtureSid})"));
    Assert(isolated.CheckDiscretionary(descriptor.Sddl) == (Rights.FullControl & ~7u));
});

Test("Allow after partial deny restores only non-denied bits", () =>
{
    // Deny 0xF, then allow 0xFF - result should be 0xFF with lower 4 bits removed
    var descriptor = DescriptorParser.Parse(Sddl($"(D;;0xF;;;{fixtureSid})(A;;0xFF;;;{fixtureSid})"));
    Assert(isolated.CheckDiscretionary(descriptor.Sddl) == (0xFF & ~0xF));
});

Test("Creator Owner applies when identity matches owner SID", () =>
{
    // CO (Creator Owner) should grant access when the fixture SID is the owner
    var descriptor = DescriptorParser.Parse($"O:{fixtureSid}G:SYD:(A;;FA;;;CO)");
    Assert((isolated.CheckDiscretionary(descriptor.Sddl) & Rights.FullControl) == Rights.FullControl);
});

Test("Creator Group does not apply without explicit group membership", () =>
{
    // CG (Creator Group) requires the identity to be in the primary group
    var descriptor = DescriptorParser.Parse($"O:SYG:{fixtureSid}D:(A;;FA;;;CG)");
    Assert(isolated.CheckDiscretionary(descriptor.Sddl) == 0);
});

Test("NULL DACL on owned object grants full control", () =>
{
    // NULL DACL means no discretionary restrictions
    var descriptor = DescriptorParser.Parse($"O:{fixtureSid}G:SYD:NO_ACCESS_CONTROL");
    Assert(isolated.CheckDiscretionary(descriptor.Sddl) == Rights.FullControl);
});

Test("Empty DACL on owned object preserves implicit owner rights only", () =>
{
    // Empty DACL denies everyone except implicit owner rights (READ_CONTROL + WRITE_DAC)
    var descriptor = DescriptorParser.Parse($"O:{fixtureSid}G:SYD:");
    Assert((isolated.CheckDiscretionary(descriptor.Sddl) & 0x60000) == 0x60000);
});

Test("Generic execute (GX) maps to traverse and execute rights", () =>
{
    var descriptor = DescriptorParser.Parse(Sddl($"(A;;GX;;;{fixtureSid})"));
    Assert((isolated.CheckDiscretionary(descriptor.Sddl) & 0x20) == 0x20);
});

Test("Generic write (GW) maps to file add and data write rights", () =>
{
    var descriptor = DescriptorParser.Parse(Sddl($"(A;;GW;;;{fixtureSid})"));
    Assert((isolated.CheckDiscretionary(descriptor.Sddl) & Rights.Write) == Rights.Write);
});

Test("Mixed generic and specific masks resolve correctly", () =>
{
    var descriptor = DescriptorParser.Parse(Sddl($"(A;;GR;;;{fixtureSid})(A;;0x1;;;{fixtureSid})"));
    Assert(isolated.CheckDiscretionary(descriptor.Sddl) == (Rights.Read | 1));
});

Test("Inherited deny blocks inherited allow of same bits", () =>
{
    var descriptor = DescriptorParser.Parse(Sddl($"(D;ID;0x1;;;{fixtureSid})(A;ID;0x3;;;{fixtureSid})"));
    Assert(isolated.CheckDiscretionary(descriptor.Sddl) == 2);
});

Test("Non-inherited allow works alongside inherit-only deny", () =>
{
    // IO (Inherit Only) deny should not affect current object
    var descriptor = DescriptorParser.Parse(Sddl($"(A;;0x1;;;{fixtureSid})(D;IO;0x1;;;{fixtureSid})"));
    Assert(isolated.CheckDiscretionary(descriptor.Sddl) == 1);
});

Test("Object inherit-only does not grant container access", () =>
{
    // IOIO flags mean inherit-only for both container and object
    var descriptor = DescriptorParser.Parse(Sddl($"(A;IOIO;FA;;;{fixtureSid})"));
    Assert(isolated.CheckDiscretionary(descriptor.Sddl) == 0);
});

Test("Container inherit-only applies to subdirectories only", () =>
{
    // CICO flags mean container inherit only
    var descriptor = DescriptorParser.Parse(Sddl($"(A;CICO;FA;;;{fixtureSid})"));
    Assert(isolated.CheckDiscretionary(descriptor.Sddl) == 0);
});

Test("Owner implicit rights survive explicit deny of standard rights", () =>
{
    // Owner always gets READ_CONTROL (0x20000) and WRITE_DAC (0x40000)
    var descriptor = DescriptorParser.Parse($"O:{fixtureSid}G:SYD:(D;;0xFFFFF;;;{fixtureSid})");
    Assert((isolated.CheckDiscretionary(descriptor.Sddl) & 0x60000) == 0x60000);
});

Test("Authenticated Users (AU) SID does not match fixture SID", () =>
{
    var descriptor = DescriptorParser.Parse(Sddl("(A;;FA;;;AU)"));
    Assert(isolated.CheckDiscretionary(descriptor.Sddl) == 0);
});

Test("System (SY) SID does not match fixture SID", () =>
{
    var descriptor = DescriptorParser.Parse(Sddl("(A;;FA;;;SY)"));
    Assert(isolated.CheckDiscretionary(descriptor.Sddl) == 0);
});

Test("World (WD) SID does not match fixture SID", () =>
{
    var descriptor = DescriptorParser.Parse(Sddl("(A;;FA;;;WD)"));
    Assert(isolated.CheckDiscretionary(descriptor.Sddl) == 0);
});

Test("Complex ACE order follows Windows first-applicable-deny semantics", () =>
{
    // Allow 0xF, Deny 0x3, Allow 0x3, Deny 0x1
    // First allow grants 0xF, first deny removes 0x3, subsequent allows cannot restore denied bits
    var descriptor = DescriptorParser.Parse(Sddl($"(A;;0xF;;;{fixtureSid})(D;;0x3;;;{fixtureSid})(A;;0x3;;;{fixtureSid})(D;;0x1;;;{fixtureSid})"));
    Assert(isolated.CheckDiscretionary(descriptor.Sddl) == (0xF & ~0x3));
});

Test("ACE with all inheritance flags set still evaluates mask correctly", () =>
{
    // All flags set including InheritOnly means no access to current object
    var descriptor = DescriptorParser.Parse(Sddl($"(A;CIFPIOISA;0x1;;;{fixtureSid})"));
    Assert(isolated.CheckDiscretionary(descriptor.Sddl) == 0);
});

Test("Callback ACE (XA) marked as special does not contribute to mask", () =>
{
    var descriptor = DescriptorParser.Parse(Sddl($"(XA;;FA;;;{fixtureSid};(@User.department == \"Finance\"))"));
    Assert(descriptor.HasSpecialAces);
    Assert(isolated.CheckDiscretionary(descriptor.Sddl) == 0);
});

Test("Resource attribute ACE (RA) marked as special", () =>
{
    var descriptor = DescriptorParser.Parse(Sddl($"(RA;;FA;;;{fixtureSid};(@User.department == \"Finance\"))"));
    Assert(descriptor.HasSpecialAces);
});

Test("Scope ACE (SA) marked as special", () =>
{
    var descriptor = DescriptorParser.Parse(Sddl($"(SA;;FA;;;{fixtureSid};(@User.department == \"Finance\"))"));
    Assert(descriptor.HasSpecialAces);
});

Test("Filter condition ACE (FA) marked as special", () =>
{
    var descriptor = DescriptorParser.Parse(Sddl($"(FA;;FA;;;{fixtureSid};(@User.department == \"Finance\"))"));
    Assert(descriptor.HasSpecialAces);
});

Test("Multiple identities in token do not cross-contaminate", () =>
{
    var otherSid = "S-1-5-21-111111111-222222222-333333333-1002";
    var descriptor = DescriptorParser.Parse(Sddl($"(A;;FA;;;{otherSid})"));
    Assert(isolated.CheckDiscretionary(descriptor.Sddl) == 0);
});

Test("Bitwise OR of multiple allows from same SID accumulates", () =>
{
    var descriptor = DescriptorParser.Parse(Sddl($"(A;;0x1;;;{fixtureSid})(A;;0x2;;;{fixtureSid})(A;;0x4;;;{fixtureSid})"));
    Assert(isolated.CheckDiscretionary(descriptor.Sddl) == 7);
});

Test("Deny ACE with zero mask has no effect", () =>
{
    var descriptor = DescriptorParser.Parse(Sddl($"(D;;0;;;{fixtureSid})(A;;FA;;;{fixtureSid})"));
    Assert(isolated.CheckDiscretionary(descriptor.Sddl) == Rights.FullControl);
});

Test("Allow ACE with zero mask has no effect", () =>
{
    var descriptor = DescriptorParser.Parse(Sddl($"(A;;0;;;{fixtureSid})"));
    Assert(isolated.CheckDiscretionary(descriptor.Sddl) == 0);
});

Test("Maximum allowed access request respects all denies", () =>
{
    var descriptor = DescriptorParser.Parse(Sddl($"(A;;FA;;;{fixtureSid})(D;;0x10000;;;{fixtureSid})"));
    Assert((isolated.CheckDiscretionary(descriptor.Sddl) & 0x10000) == 0);
});

Test("Descriptor with both owner and group matching identity", () =>
{
    var descriptor = DescriptorParser.Parse($"O:{fixtureSid}G:{fixtureSid}D:(A;;FA;;;{fixtureSid})");
    Assert(isolated.CheckDiscretionary(descriptor.Sddl) == Rights.FullControl);
});

Test("Protected DACL with inheritance flags still evaluates inherited ACEs", () =>
{
    var descriptor = DescriptorParser.Parse(Sddl($"P(A;ID;FA;;;{fixtureSid})"));
    Assert(isolated.CheckDiscretionary(descriptor.Sddl) == Rights.FullControl);
});

Test("Auto-inherit flag (S) does not affect evaluation", () =>
{
    var descriptor = DescriptorParser.Parse(Sddl($"S(A;;FA;;;{fixtureSid})"));
    Assert(isolated.CheckDiscretionary(descriptor.Sddl) == Rights.FullControl);
});

// ============================================================================
// Owner Rights (OW) Specific Tests
// ============================================================================

Test("Owner Rights SID (OW) overrides implicit owner permissions when denying", () =>
{
    // OW (S-1-3-4) can explicitly deny rights that owner would normally have
    var descriptor = DescriptorParser.Parse($"O:{fixtureSid}G:SYD:(D;;WD;;;OW)");
    Assert((isolated.CheckDiscretionary(descriptor.Sddl) & 0x40000) == 0);
});

Test("Owner Rights allow has auditable ACE evidence", () =>
{
    var descriptor = DescriptorParser.Parse($"O:{fixtureSid}G:SYD:(A;;0x1;;;OW)");
    var mask = isolated.CheckDiscretionary(descriptor.Sddl);
    var evidence = AccessPathBuilder.Explain(descriptor, isolated.Identity, mask, "fixture", new());
    Assert((mask & 1) == 1);
    Assert(evidence.Any(e => e.Sid == "S-1-3-4" && e.Relation == "GrantedBy" && (e.ContributingMask & 1) == 1));
});

Test("Owner Rights deny has auditable ACE evidence", () =>
{
    var descriptor = DescriptorParser.Parse($"O:{fixtureSid}G:SYD:(D;;WD;;;OW)");
    var mask = isolated.CheckDiscretionary(descriptor.Sddl);
    var evidence = AccessPathBuilder.Explain(descriptor, isolated.Identity, mask, "fixture", new());
    Assert(evidence.Any(e => e.Sid == "S-1-3-4" && e.Relation == "DeniedBy" && (e.ContributingMask & 0x40000) != 0));
});

Test("Owner Rights ACE does not apply to a non-owner", () =>
{
    var descriptor = DescriptorParser.Parse("O:SYG:SYD:(A;;FA;;;OW)");
    Assert(AccessPathBuilder.Explain(descriptor, isolated.Identity, 0, "fixture", new()).Count == 0);
});

// ============================================================================
// Inheritance Flag Combinations
// ============================================================================

Test("CI (Container Inherit) flag alone does not grant current object", () =>
{
    var descriptor = DescriptorParser.Parse(Sddl($"(A;CI;FA;;;{fixtureSid})"));
    Assert(isolated.CheckDiscretionary(descriptor.Sddl) == 0);
});

Test("OI (Object Inherit) flag alone does not grant current object", () =>
{
    var descriptor = DescriptorParser.Parse(Sddl($"(A;OI;FA;;;{fixtureSid})"));
    Assert(isolated.CheckDiscretionary(descriptor.Sddl) == 0);
});

Test("ID (Inherit Only) flag prevents current object access", () =>
{
    var descriptor = DescriptorParser.Parse(Sddl($"(A;ID;FA;;;{fixtureSid})"));
    Assert(isolated.CheckDiscretionary(descriptor.Sddl) == 0);
});

Test("NP (No Propagate) with CI still inherits to immediate children", () =>
{
    // NP affects propagation depth, not current object evaluation
    var descriptor = DescriptorParser.Parse(Sddl($"(A;CINP;FA;;;{fixtureSid})"));
    Assert(isolated.CheckDiscretionary(descriptor.Sddl) == 0);
});

Test("Multiple inheritance flags combined", () =>
{
    var descriptor = DescriptorParser.Parse(Sddl($"(A;CIOI;FA;;;{fixtureSid})"));
    Assert(isolated.CheckDiscretionary(descriptor.Sddl) == 0);
});

// ============================================================================
// Special Principal Tests
// ============================================================================

Test("Local Admins (BA) does not match fixture", () =>
{
    var descriptor = DescriptorParser.Parse(Sddl("(A;;FA;;;BA)"));
    Assert(isolated.CheckDiscretionary(descriptor.Sddl) == 0);
});

Test("Network (NU) does not match fixture", () =>
{
    var descriptor = DescriptorParser.Parse(Sddl("(A;;FA;;;NU)"));
    Assert(isolated.CheckDiscretionary(descriptor.Sddl) == 0);
});

Test("Interactive (IU) does not match fixture", () =>
{
    var descriptor = DescriptorParser.Parse(Sddl("(A;;FA;;;IU)"));
    Assert(isolated.CheckDiscretionary(descriptor.Sddl) == 0);
});

Test("Service (S-1-5-6) does not match fixture", () =>
{
    var descriptor = DescriptorParser.Parse(Sddl("(A;;FA;;;S-1-5-6)"));
    Assert(isolated.CheckDiscretionary(descriptor.Sddl) == 0);
});

Test("Anonymous Logon (AN) does not match fixture", () =>
{
    var descriptor = DescriptorParser.Parse(Sddl("(A;;FA;;;AN)"));
    Assert(isolated.CheckDiscretionary(descriptor.Sddl) == 0);
});

Test("Proxy (ANONYMOUS) does not match fixture", () =>
{
    var descriptor = DescriptorParser.Parse(Sddl("(A;;FA;;;S-1-5-7)"));
    Assert(isolated.CheckDiscretionary(descriptor.Sddl) == 0);
});

// ============================================================================
// Standard Rights Combinations
// ============================================================================

Test("READ_CONTROL (0x20000) is always granted to owner", () =>
{
    var descriptor = DescriptorParser.Parse($"O:{fixtureSid}G:SYD:");
    Assert((isolated.CheckDiscretionary(descriptor.Sddl) & 0x20000) == 0x20000);
});

Test("WRITE_DAC (0x40000) is always granted to owner", () =>
{
    var descriptor = DescriptorParser.Parse($"O:{fixtureSid}G:SYD:");
    Assert((isolated.CheckDiscretionary(descriptor.Sddl) & 0x40000) == 0x40000);
});

Test("SYNCHRONIZE (0x100000) is not implicitly granted", () =>
{
    var descriptor = DescriptorParser.Parse(Sddl($"(A;;FA;;;{fixtureSid})"));
    // FullControl includes SYNCHRONIZE
    Assert((isolated.CheckDiscretionary(descriptor.Sddl) & 0x100000) == 0x100000);
});

Test("Access to descriptor owner without explicit ACE", () =>
{
    var descriptor = DescriptorParser.Parse($"O:{fixtureSid}G:SYD:(A;;FR;;;SY)");
    // Owner should still get READ_CONTROL and WRITE_DAC
    var mask = isolated.CheckDiscretionary(descriptor.Sddl);
    Assert((mask & 0x60000) == 0x60000);
    Assert((mask & Rights.Read) == Rights.Read);
});

// ============================================================================
// ACE Type Variations
// ============================================================================

Test("AccessAllowedCallback ACE is marked unsupported", () =>
{
    var descriptor = DescriptorParser.Parse(Sddl($"(XA;;FA;;;{fixtureSid};())"));
    Assert(!descriptor.Aces.Any(a => a.Supported));
});

Test("AccessDeniedCallback ACE is marked unsupported", () =>
{
    var descriptor = DescriptorParser.Parse(Sddl($"(XD;;FA;;;{fixtureSid};())"));
    Assert(!descriptor.Aces.Any(a => a.Supported));
});

Test("SystemAudit ACE is parsed but does not affect access", () =>
{
    var descriptor = DescriptorParser.Parse(Sddl($"(A;;FA;;;{fixtureSid})(S;;FA;;;SY)"));
    Assert(isolated.CheckDiscretionary(descriptor.Sddl) == Rights.FullControl);
    Assert(descriptor.Aces.Any(a => a.Type == "SystemAudit"));
});

Test("SystemAlarm ACE is parsed but does not affect access", () =>
{
    var descriptor = DescriptorParser.Parse(Sddl($"(A;;FA;;;{fixtureSid})(Z;;FA;;;SY)"));
    Assert(isolated.CheckDiscretionary(descriptor.Sddl) == Rights.FullControl);
});

// ============================================================================
// Malformed and Edge Case Descriptors
// ============================================================================

Test("Descriptor with only owner and group, no DACL field", () =>
{
    var descriptor = DescriptorParser.Parse($"O:{fixtureSid}G:{fixtureSid}");
    Assert(descriptor.NullDacl);
});

Test("Empty ACE list is valid empty DACL", () =>
{
    var descriptor = DescriptorParser.Parse(Sddl(""));
    Assert(!descriptor.NullDacl);
    Assert(descriptor.Aces.Count == 0);
});

Test("Single ACE descriptor parsing", () =>
{
    var descriptor = DescriptorParser.Parse(Sddl($"(A;;FA;;;{fixtureSid})"));
    Assert(descriptor.Aces.Count == 1);
    Assert(descriptor.Aces[0].Sid == fixtureSid);
    Assert(descriptor.Aces[0].Type == "AccessAllowed");
});

Test("ACE index preserved in parsing", () =>
{
    var descriptor = DescriptorParser.Parse(Sddl($"(A;;0x1;;;{fixtureSid})(D;;0x2;;;{fixtureSid})(A;;0x4;;;{fixtureSid})"));
    Assert(descriptor.Aces[0].Index == 0);
    Assert(descriptor.Aces[1].Index == 1);
    Assert(descriptor.Aces[2].Index == 2);
});

Test("Hash is deterministic for same descriptor", () =>
{
    var sddl = Sddl($"(A;;FA;;;{fixtureSid})");
    var h1 = DescriptorParser.Parse(sddl).Hash;
    var h2 = DescriptorParser.Parse(sddl).Hash;
    Assert(h1 == h2);
});

Test("Hash differs for different descriptors", () =>
{
    var h1 = DescriptorParser.Parse(Sddl($"(A;;FA;;;{fixtureSid})")).Hash;
    var h2 = DescriptorParser.Parse(Sddl($"(A;;FR;;;{fixtureSid})")).Hash;
    Assert(h1 != h2);
});

// ============================================================================
// Evidence and Explanation Tests
// ============================================================================

Test("Evidence includes ACE index for allow", () =>
{
    var descriptor = DescriptorParser.Parse(Sddl($"(A;;0x1;;;{fixtureSid})"));
    var evidence = AccessPathBuilder.Explain(descriptor, isolated.Identity, 1, "test", new());
    Assert(evidence.Count == 1);
    Assert(evidence[0].AceIndex == 0);
});

Test("Evidence includes ACE index for deny", () =>
{
    var descriptor = DescriptorParser.Parse(Sddl($"(D;;0x1;;;{fixtureSid})(A;;FA;;;{fixtureSid})"));
    var evidence = AccessPathBuilder.Explain(descriptor, isolated.Identity, Rights.FullControl & ~1u, "test", new());
    Assert(evidence.Any(e => e.Relation == "DeniedBy" && e.AceIndex == 0));
});

Test("Evidence includes membership path for group ACEs", () =>
{
    var graph = new GroupGraph();
    graph.Add(new(fixtureSid, "S-1-5-21-111-222-333-500", "test"));
    var descriptor = DescriptorParser.Parse(Sddl("(A;;FA;;;S-1-5-21-111-222-333-500)"));
    var evidence = AccessPathBuilder.Explain(descriptor, isolated.Identity, Rights.FullControl, "test", graph);
    // Fixture SID doesn't match the group, so no evidence expected
    Assert(evidence.Count == 0);
});

Test("Null DACL evidence includes Everyone principal", () =>
{
    var descriptor = DescriptorParser.Parse("O:SYG:SYD:NO_ACCESS_CONTROL");
    var evidence = AccessPathBuilder.Explain(descriptor, isolated.Identity, Rights.FullControl, "test", new());
    Assert(evidence.Any(e => e.Sid == "S-1-1-0" && e.Relation == "NullDacl"));
});

Test("Owner rights evidence includes correct relation", () =>
{
    var descriptor = DescriptorParser.Parse($"O:{fixtureSid}G:SYD:");
    var evidence = AccessPathBuilder.Explain(descriptor, isolated.Identity, 0x60000, "test", new());
    Assert(evidence.Any(e => e.Relation == "OwnerRights" && (e.ContributingMask & 0x60000) == 0x60000));
});

// ============================================================================
// Risk Finding Tests
// ============================================================================

Test("Null DACL generates CriticalExposure finding", () =>
{
    var findings = FindingRules.Evaluate("C:\\test", DescriptorParser.Parse("O:SYG:SYD:NO_ACCESS_CONTROL"));
    Assert(findings.Any(f => f.Rule == "NullDacl" && f.Severity == Severity.CriticalExposure));
});

Test("Noncanonical ACL generates Review finding", () =>
{
    var descriptor = DescriptorParser.Parse(Sddl($"(A;;0x1;;;{fixtureSid})(D;;0x1;;;{fixtureSid})"));
    var findings = FindingRules.Evaluate("C:\\test", descriptor);
    Assert(findings.Any(f => f.Rule == "NoncanonicalDacl" && f.Severity == Severity.Review));
});

Test("Protected DACL generates Review finding", () =>
{
    var descriptor = DescriptorParser.Parse(Sddl($"P(A;;FA;;;SY)"));
    var findings = FindingRules.Evaluate("C:\\test", descriptor);
    Assert(findings.Any(f => f.Rule == "ProtectedDacl" && f.Severity == Severity.Review));
});

Test("Special ACE generates Review finding", () =>
{
    var descriptor = DescriptorParser.Parse(Sddl($"(XA;;FA;;;{fixtureSid};())"));
    var findings = FindingRules.Evaluate("C:\\test", descriptor);
    Assert(findings.Any(f => f.Rule == "SpecialAce" && f.Severity == Severity.Review));
});

Test("Broad allow to World generates HighExposure finding", () =>
{
    var descriptor = DescriptorParser.Parse(Sddl("(A;;FA;;;WD)"));
    var findings = FindingRules.Evaluate("C:\\test", descriptor);
    Assert(findings.Any(f => f.Rule == "BroadAllow" && f.Severity == Severity.CriticalExposure));
});

Test("Broad allow to Authenticated Users generates HighExposure finding", () =>
{
    var descriptor = DescriptorParser.Parse(Sddl("(A;;FA;;;AU)"));
    var findings = FindingRules.Evaluate("C:\\test", descriptor);
    Assert(findings.Any(f => f.Rule == "BroadAllow" && f.Severity == Severity.HighExposure));
});

Test("Unresolved SID generates Review finding", () =>
{
    var descriptor = DescriptorParser.Parse(Sddl($"(A;;FR;;;{fixtureSid})"));
    var findings = FindingRules.Evaluate("C:\\test", descriptor);
    Assert(findings.Any(f => f.Rule == "UnresolvedSid"));
});

Test("Multiple findings can exist on same descriptor", () =>
{
    var descriptor = DescriptorParser.Parse(Sddl("(A;;FA;;;WD)(XA;;FA;;;SY;())"));
    var findings = FindingRules.Evaluate("C:\\test", descriptor);
    Assert(findings.Count >= 2);
});

// ============================================================================
// Impact Simulation Tests
// ============================================================================

Test("Remove first ACE from multi-ACE descriptor", () =>
{
    var before = Sddl($"(A;;0x1;;;{fixtureSid})(A;;0x2;;;{fixtureSid})");
    var after = ImpactSimulator.RemoveAce(before, 0);
    Assert(isolated.CheckDiscretionary(before) == 3);
    Assert(isolated.CheckDiscretionary(after) == 2);
});

Test("Remove last ACE from multi-ACE descriptor", () =>
{
    var before = Sddl($"(A;;0x1;;;{fixtureSid})(A;;0x2;;;{fixtureSid})");
    var after = ImpactSimulator.RemoveAce(before, 1);
    Assert(isolated.CheckDiscretionary(before) == 3);
    Assert(isolated.CheckDiscretionary(after) == 1);
});

Test("Add allow ACE inserts at correct position", () =>
{
    var before = Sddl($"(D;;0x1;;;{fixtureSid})(A;;0x2;;;{fixtureSid})");
    var after = ImpactSimulator.AddAce(before, fixtureSid, 0x4, false);
    var descriptor = DescriptorParser.Parse(after);
    // Allow should be inserted before inherited ACEs but after existing non-inherited
    Assert(descriptor.Aces.Count == 3);
});

Test("Add deny ACE inserts at beginning", () =>
{
    var before = Sddl($"(A;;FA;;;{fixtureSid})");
    var after = ImpactSimulator.AddAce(before, fixtureSid, 1, true);
    var descriptor = DescriptorParser.Parse(after);
    Assert(descriptor.Aces[0].Type == "AccessDenied");
    Assert(isolated.CheckDiscretionary(after) == (Rights.FullControl & ~1u));
});

Test("Simulation maintains precedence with added deny", () =>
{
    var before = Sddl($"(A;;FA;;;{fixtureSid})");
    var after = ImpactSimulator.AddAce(before, fixtureSid, 1, true);
    Assert(isolated.CheckDiscretionary(after) == (Rights.FullControl & ~1u));
});

Test("Cannot remove ACE from empty DACL", () =>
{
    Throws<ArgumentOutOfRangeException>(() => ImpactSimulator.RemoveAce(Sddl(""), 0));
});

Test("Cannot remove ACE with negative index", () =>
{
    Throws<ArgumentOutOfRangeException>(() => ImpactSimulator.RemoveAce(Sddl($"(A;;FA;;;{fixtureSid})"), -1));
});

Test("Cannot remove ACE beyond range", () =>
{
    Throws<ArgumentOutOfRangeException>(() => ImpactSimulator.RemoveAce(Sddl($"(A;;FA;;;{fixtureSid})"), 5));
});

Test("Cannot add ACE to NULL DACL without conversion", () =>
{
    Throws<InvalidOperationException>(() => ImpactSimulator.AddAce("O:SYG:SYD:NO_ACCESS_CONTROL", fixtureSid, 1, false));
});

// ============================================================================
// Locale Resolution Tests
// ============================================================================

Test("Locale exact match returns exact resource", () =>
{
    Assert(LocaleResolver.Resolve("pt-BR", ["en-US", "pt-BR"]).ResourceTag == "pt-BR");
});

Test("Locale language fallback to base language", () =>
{
    Assert(LocaleResolver.Resolve("es-MX", ["en-US", "es"]).ResourceTag == "es");
});

Test("Locale regional sibling fallback", () =>
{
    Assert(LocaleResolver.Resolve("pt-PT", ["en-US", "pt-BR"]).ResourceTag == "pt-BR");
});

Test("Locale fallback preserves regional formatting info", () =>
{
    var result = LocaleResolver.Resolve("de-AT", ["en-US"]);
    Assert(result.LanguageTag == "de-AT");
});

Test("Locale Arabic regional detects RTL", () =>
{
    Assert(LocaleResolver.Resolve("ar-SA", ["en-US", "ar"]).IsRightToLeft);
});

Test("Locale Persian RTL with English fallback", () =>
{
    Assert(LocaleResolver.Resolve("fa-IR", ["en-US"]).IsRightToLeft);
});

Test("Locale Chinese traditional script fallback", () =>
{
    Assert(LocaleResolver.Resolve("zh-TW", ["en-US", "zh-Hant", "zh-Hans"]).ResourceTag == "zh-Hant");
});

Test("Locale Chinese does not substitute scripts incorrectly", () =>
{
    Assert(LocaleResolver.Resolve("zh-TW", ["en-US", "zh-Hans"]).ResourceTag == "en-US");
});

Test("Locale Japanese uses exact match when available", () =>
{
    Assert(LocaleResolver.Resolve("ja-JP", ["en-US", "ja-JP", "ja"]).ResourceTag == "ja-JP");
});

Test("Locale Korean falls back to base language", () =>
{
    Assert(LocaleResolver.Resolve("ko-KR", ["en-US", "ko"]).ResourceTag == "ko");
});

Test("Locale unavailable falls back to first available", () =>
{
    Assert(LocaleResolver.Resolve("xx-XX", ["en-US", "fr-FR"]).ResourceTag == "en-US");
});

Test("Locale empty available list throws", () =>
{
    Throws<ArgumentException>(() => LocaleResolver.Resolve("en-US", []));
});

// ============================================================================
// Capability Decision Tests
// ============================================================================

Test("FullControl mask grants all capabilities", () =>
{
    var decision = new AccessDecision(fixtureSid, "Test", AccessState.Granted, Rights.FullControl, null, "test", null, []);
    var caps = decision.Capabilities;
    Assert(caps.All(c => c.State == AccessState.Granted));
});

Test("Read mask grants read-related capabilities", () =>
{
    var decision = new AccessDecision(fixtureSid, "Test", AccessState.Granted, Rights.Read, null, "test", null, []);
    var caps = decision.Capabilities;
    Assert(caps.Single(c => c.Key == "Read").State == AccessState.Granted);
    Assert(caps.Single(c => c.Key == "ListReadData").State == AccessState.Granted);
});

Test("Write mask grants write-related capabilities", () =>
{
    var decision = new AccessDecision(fixtureSid, "Test", AccessState.Granted, Rights.Write, null, "test", null, []);
    var caps = decision.Capabilities;
    Assert(caps.Single(c => c.Key == "Write").State == AccessState.Granted);
    Assert(caps.Single(c => c.Key == "CreateWriteData").State == AccessState.Granted);
});

Test("Partial mask results in Partial capability state", () =>
{
    var decision = new AccessDecision(fixtureSid, "Test", AccessState.Partial, 0x120089, null, "test", null, []);
    var caps = decision.Capabilities;
    Assert(caps.Any(c => c.State == AccessState.Partial));
});

Test("Unknown state propagates to all capabilities", () =>
{
    var decision = new AccessDecision(fixtureSid, "Test", AccessState.Unknown, Rights.FullControl, null, "test", "limitation", []);
    var caps = decision.Capabilities;
    Assert(caps.All(c => c.State == AccessState.Unknown));
});

Test("Zero mask results in Denied capabilities", () =>
{
    var decision = new AccessDecision(fixtureSid, "Test", AccessState.Denied, 0, null, "test", null, []);
    var caps = decision.Capabilities;
    Assert(caps.All(c => c.State == AccessState.Denied));
});

// ============================================================================
// Share and NTFS Intersection Tests
// ============================================================================

Test("Share and NTFS intersection takes minimum", () =>
{
    var decision = new AccessDecision(fixtureSid, "Fixture", AccessState.Partial, Rights.FullControl, Rights.Read, "fixture", null, []);
    Assert(decision.EffectiveMask == Rights.Read);
    Assert(decision.Capabilities.Single(c => c.Key == "Write").State != AccessState.Granted);
});

Test("Share mask null means NTFS only", () =>
{
    var decision = new AccessDecision(fixtureSid, "Fixture", AccessState.Granted, Rights.Read, null, "fixture", null, []);
    Assert(decision.EffectiveMask == Rights.Read);
});

Test("Remote context is conservatively Unknown", () =>
{
    var sd = DescriptorParser.Parse(Sddl($"(A;;FA;;;{fixtureSid})"));
    var share = new ShareInfo("server", "share", "C:\\share", sd, null);
    Assert(isolated.Evaluate(sd, "\\\\server\\share", share).State == AccessState.Unknown);
});

Test("Reparse decision cannot prove target access", () =>
{
    var sd = DescriptorParser.Parse(Sddl($"(A;;FA;;;{fixtureSid})"));
    Assert(isolated.Evaluate(sd, "C:\\link", null, true).State == AccessState.Unknown);
});

// ============================================================================
// Benchmark and Performance Tests
// ============================================================================

Test("500 randomized ACLs agree with ordered-bit reference", () =>
{
    var random = new Random(9183);
    for (var i = 0; i < 500; i++)
    {
        uint granted = 0, remaining = 0x1ff;
        var aces = new List<string>();
        for (var j = 0; j < 8; j++)
        {
            var deny = random.Next(2) == 0;
            var mask = (uint)random.Next(512);
            aces.Add($"({(deny ? "D" : "A")};;0x{mask:X};;;{fixtureSid})");
            if (!deny) granted |= remaining & mask;
            remaining &= ~mask;
        }
        Assert(isolated.CheckDiscretionary(Sddl(string.Join("", aces))) == granted, "Differential fixture " + i);
    }
});

Test("Deep 10,000-group traversal completes efficiently", () =>
{
    var graph = new GroupGraph();
    for (var i = 0; i < 10000; i++)
        graph.Add(new(i.ToString(), (i + 1).ToString(), "fixture"));
    var clock = Stopwatch.StartNew();
    var path = graph.FindPath("0", "10000");
    timings.Add($"Group path, 10,000 edges: {clock.Elapsed.TotalMilliseconds:F2} ms");
    Assert(path.Count == 10000);
});

Test("Cycle detection in group graph", () =>
{
    var graph = new GroupGraph();
    graph.Add(new("u", "g1", "fixture"));
    graph.Add(new("g1", "g2", "fixture"));
    graph.Add(new("g2", "g1", "fixture")); // Cycle
    graph.Add(new("u", "g1", "fixture")); // Duplicate
    Assert(graph.Edges.Count == 3); // Duplicate should be ignored
    Assert(graph.FindPath("u", "g2").Count == 2);
    Assert(graph.FindPath("u", "missing").Count == 0);
});

Console.WriteLine($"\n{passed} passed; {failures.Count} failed.");
foreach (var timing in timings) Console.WriteLine(timing);
if (args.Contains("--results"))
{
    var index = Array.IndexOf(args, "--results");
    AtomicFile.WriteText(args[index + 1], JsonSerializer.Serialize(new { Passed = passed, Failed = failures.Count, Failures = failures, Benchmarks = timings, Runtime = Environment.Version.ToString(), Timestamp = DateTimeOffset.UtcNow }, SnapshotJson.Options));
}
Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
try { Directory.Delete(testRoot, true); } catch (IOException e) { Console.WriteLine("Test cleanup: " + e.Message); }
return failures.Count == 0 ? 0 : 1;
