using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Principal;
using PermissionScope.Core;

namespace PermissionScope.Windows;

public sealed class EffectiveAccessResolver : IDisposable
{
    private readonly ResourceManagerHandle manager;
    private readonly ContextHandle context;
    public AccessIdentity Identity { get; }
    public GroupGraph Groups { get; } = new();

    public EffectiveAccessResolver(IdentityResolver resolver, string? identity = null, bool isolatedSid = false, IReadOnlyList<string>? fixtureGroups = null)
    {
        if (fixtureGroups is { Count: > 0 } && (!isolatedSid || fixtureGroups.Count > 128))
            throw new ArgumentException("Synthetic groups are restricted to bounded isolated test contexts.");
        if (!Native.AuthzInitializeResourceManager(1, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, "PermissionScope", out manager))
            throw new Win32Exception(Marshal.GetLastWin32Error());
        try
        {
            using var current = WindowsIdentity.GetCurrent();
            var selected = identity is null ? resolver.Resolve(current.User!.Value) : resolver.Find(identity);
            var fromToken = !isolatedSid && selected.Sid == current.User!.Value;
            bool initialized;
            if (fromToken)
                initialized = Native.AuthzInitializeContextFromToken(0, current.AccessToken, manager, IntPtr.Zero, default, IntPtr.Zero, out context);
            else
            {
                var sid = new SecurityIdentifier(selected.Sid);
                var bytes = new byte[sid.BinaryLength];
                sid.GetBinaryForm(bytes, 0);
                initialized = Native.AuthzInitializeContextFromSid(isolatedSid ? 2u : 4u, bytes, manager, IntPtr.Zero, default, IntPtr.Zero, out context);
            }
            if (!initialized) throw new Win32Exception(Marshal.GetLastWin32Error());
            if (fixtureGroups is { Count: > 0 })
            {
                var size = Marshal.SizeOf<Native.SidAndAttributes>();
                var entries = Marshal.AllocHGlobal(size * fixtureGroups.Count);
                var pins = new List<GCHandle>();
                try
                {
                    for (var i = 0; i < fixtureGroups.Count; i++)
                    {
                        var group = new SecurityIdentifier(fixtureGroups[i]);
                        var bytes = new byte[group.BinaryLength]; group.GetBinaryForm(bytes, 0);
                        var pin = GCHandle.Alloc(bytes, GCHandleType.Pinned); pins.Add(pin);
                        Marshal.StructureToPtr(new Native.SidAndAttributes { Sid = pin.AddrOfPinnedObject(), Attributes = 4 }, entries + i * size, false);
                    }
                    if (!Native.AuthzAddSidsToContext(context, entries, (uint)fixtureGroups.Count, IntPtr.Zero, 0, out var expanded))
                        throw new Win32Exception(Marshal.GetLastWin32Error());
                    context.Dispose(); context = expanded;
                }
                finally { foreach (var pin in pins) pin.Free(); Marshal.FreeHGlobal(entries); }
            }
            Native.AuthzGetInformationFromContext(context, 2, 0, out var needed, IntPtr.Zero);
            if (needed > 16 * 1024 * 1024) throw new InvalidDataException("Authz group buffer exceeds the safety bound.");
            var buffer = Marshal.AllocHGlobal(checked((int)needed));
            IReadOnlyList<TokenSid> sids;
            try
            {
                if (!Native.AuthzGetInformationFromContext(context, 2, needed, out _, buffer)) throw new Win32Exception(Marshal.GetLastWin32Error());
                sids = IdentityResolver.ReadGroups(buffer, needed);
            }
            finally { Marshal.FreeHGlobal(buffer); }
            Identity = new(selected, sids, fromToken, fromToken ? "Current Windows access token" : isolatedSid ? "Isolated SID fixture" : "Windows S4U context; logon characteristics may differ");
        }
        catch { context?.Dispose(); manager.Dispose(); throw; }
    }

    public uint CheckDiscretionary(string sddl)
    {
        var raw = new RawSecurityDescriptor(sddl);
        if (raw.DiscretionaryAcl is { } acl)
            foreach (GenericAce ace in acl)
                if (ace is KnownAce known) known.AccessMask = unchecked((int)Rights.MapGeneric(unchecked((uint)known.AccessMask)));
        var bytes = new byte[raw.BinaryLength];
        raw.GetBinaryForm(bytes, 0);
        var buffer = Marshal.AllocHGlobal(12);
        try
        {
            Marshal.WriteInt32(buffer, 0, 0);
            Marshal.WriteInt32(buffer, 4, 0);
            Marshal.WriteInt32(buffer, 8, 0);
            var request = new Native.AccessRequest { DesiredAccess = 0x02000000 };
            var reply = new Native.AccessReply { ResultListLength = 1, GrantedAccessMask = buffer, SaclEvaluationResults = buffer + 4, Error = buffer + 8 };
            if (!Native.AuthzAccessCheck(0, context, ref request, IntPtr.Zero, bytes, IntPtr.Zero, 0, ref reply, IntPtr.Zero))
                throw new Win32Exception(Marshal.GetLastWin32Error());
            var error = Marshal.ReadInt32(buffer, 8);
            if (error != 0 && error != 5) throw new Win32Exception(error);
            return unchecked((uint)Marshal.ReadInt32(buffer)) & Rights.FullControl;
        }
        finally { Marshal.FreeHGlobal(buffer); }
    }

    public AccessDecision Evaluate(DescriptorInfo descriptor, string path, ShareInfo? share = null, bool reparse = false)
    {
        var mask = CheckDiscretionary(descriptor.Sddl);
        uint? shareMask = share?.Descriptor is { } shareDescriptor ? CheckDiscretionary(shareDescriptor.Sddl) : null;
        var limitation = descriptor.HasSpecialAces ? "Conditional or object-specific ACEs require resource claims or object context." :
            reparse ? "Reparse target was not followed; target access is not established." :
            share != null ? "Share and NTFS masks are local-context projections. The server logon, server-local groups and DFS target policies are not verified." :
            !Identity.IsCurrentToken ? "S4U context differs from an actual target logon. Interactive/network groups, claims and account restrictions can affect access." : null;
        var effective = mask & (shareMask ?? Rights.FullControl);
        var state = limitation != null ? AccessState.Unknown : effective == Rights.FullControl ? AccessState.Granted : effective == 0 ? AccessState.Denied : AccessState.Partial;
        var evidence = AccessPathBuilder.Explain(descriptor, Identity, mask, path, Groups).ToList();
        if (share?.Descriptor is { } sd)
            evidence.AddRange(AccessPathBuilder.Explain(sd, Identity, shareMask!.Value, $"\\\\{share.Server}\\{share.Share} [share]", Groups));
        return new(Identity.Identity.Sid, Identity.Identity.Name, state, mask, shareMask,
            $"AuthzAccessCheck / {Identity.Context}. Discretionary permissions only; not an actual file-open test. Integrity policy, privileges, parent traversal, encryption and file locks are outside this decision.", limitation, evidence);
    }
    public void Dispose() { context.Dispose(); manager.Dispose(); }
}
