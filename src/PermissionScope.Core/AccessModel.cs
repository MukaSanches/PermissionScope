using System.Security.AccessControl;
using System.Security.Cryptography;
using System.Text;

namespace PermissionScope.Core;

public enum AccessState { Granted, Denied, Partial, Unknown }
public enum Severity { Expected, Review, HighExposure, CriticalExposure }
public sealed record Identity(string Sid, string Name, string Kind, bool Resolved);
public sealed record GroupEdge(string MemberSid, string GroupSid, string Source);
public sealed record TokenSid(string Sid, uint Attributes)
{
    public bool DenyOnly => (Attributes & 0x10) != 0;
    public bool Enabled => (Attributes & 4) != 0;
}
public sealed record AccessIdentity(Identity Identity, IReadOnlyList<TokenSid> Sids, bool IsCurrentToken, string Context);
public sealed record AccessEvidence(int AceIndex, string Sid, string Identity, string Relation, uint Mask,
    uint ContributingMask, string Flags, string Source, IReadOnlyList<GroupEdge> MembershipPath);
public sealed record Capability(string Key, uint Mask);
public sealed record CapabilityDecision(string Key, uint Mask, AccessState State);
public sealed record AccessDecision(string Sid, string Identity, AccessState State, uint GrantedMask,
    uint? ShareMask, string Basis, string? Limitation, IReadOnlyList<AccessEvidence> Evidence)
{
    public uint EffectiveMask => ShareMask is { } share ? GrantedMask & share : GrantedMask;
    public IReadOnlyList<CapabilityDecision> Capabilities => Rights.Capabilities.Select(c => new CapabilityDecision(c.Key, c.Mask,
        State == AccessState.Unknown ? AccessState.Unknown : (EffectiveMask & c.Mask) == c.Mask ? AccessState.Granted :
        (EffectiveMask & c.Mask) == 0 ? AccessState.Denied : AccessState.Partial)).ToArray();
}
public sealed record AceEntry(int Index, string Sid, string Name, string Type, uint Mask, string Flags, bool Inherited, bool InheritOnly, bool Supported);
public sealed record DescriptorInfo(string Sddl, string Hash, string? Owner, string? PrimaryGroup,
    bool NullDacl, bool Protected, bool Canonical, bool HasSpecialAces, IReadOnlyList<AceEntry> Aces);
public sealed record RiskFinding(string Rule, Severity Severity, string Path, string Evidence, string Recommendation);
public sealed record ScanError(string Path, int Code, string Message);
public sealed record ShareInfo(string Server, string Share, string? LocalPath, DescriptorInfo? Descriptor, string? Error);
public sealed record ResourceAccess(string Path, bool IsDirectory, bool IsReparsePoint, DateTimeOffset ObservedAt,
    DescriptorInfo? Descriptor, ShareInfo? Share, AccessDecision? Decision, IReadOnlyList<RiskFinding> Findings, ScanError? Error);
public sealed record ScanProgress(long Analyzed, long Errors, TimeSpan Elapsed, string Path);
public sealed record PermissionSnapshot(int SchemaVersion, string AppVersion, string Id, string Root,
    DateTimeOffset CreatedAt, DateTimeOffset CompletedAt, bool Cancelled, string IdentitySid,
    IReadOnlyList<ResourceAccess> Resources, IReadOnlyList<GroupEdge> Groups)
{
    public const int CurrentSchema = 1;
    public IReadOnlyList<string> IdentityWarnings { get; init; } = [];
    public DirectoryIdentity? DirectoryIdentity { get; init; }
    public string? FixtureId { get; init; }
}
public sealed record DirectoryIdentity(string Sid, string DistinguishedName, bool Disabled, string? PrimaryGroupSid, IReadOnlyList<string> SidHistory, string Source);
public sealed record SnapshotChange(string Path, string Kind, string Detail, string? BeforeHash, string? AfterHash);

public static class Rights
{
    public const uint FullControl = 0x1F01FF;
    public const uint Read = 0x120089;
    public const uint Write = 0x120116;
    public const uint Execute = 0x1200A0;
    public static readonly Capability[] Capabilities =
    [
        new("ListReadData", 1), new("TraverseExecute", 0x20), new("Read", Read),
        new("CreateWriteData", 2), new("CreateFoldersAppend", 4), new("Write", Write),
        new("Modify", 0x1301BF), new("Delete", 0x10000), new("DeleteChildren", 0x40),
        new("ReadPermissions", 0x20000), new("ChangePermissions", 0x40000),
        new("TakeOwnership", 0x80000), new("FullControl", FullControl)
    ];
    public static uint MapGeneric(uint mask)
    {
        var result = mask & 0x0FFFFFFF;
        if ((mask & 0x80000000) != 0) result |= Read;
        if ((mask & 0x40000000) != 0) result |= Write;
        if ((mask & 0x20000000) != 0) result |= Execute;
        if ((mask & 0x10000000) != 0) result |= FullControl;
        return result;
    }
    public static string Describe(uint mask) => mask == 0 ? "None" : (mask & FullControl) == FullControl ? "Full control" :
        (mask & 0x1301BF) == 0x1301BF ? "Modify" : $"0x{mask:X8}";
}

public static class AccessSummary
{
    public static string Key(AccessDecision? decision)
    {
        if (decision is null || decision.State == AccessState.Unknown) return "SummaryUnknown";
        var mask = decision.EffectiveMask;
        if ((mask & Rights.FullControl) == Rights.FullControl) return "SummaryFull";
        if ((mask & 0x1301BF) == 0x1301BF) return "SummaryModify";
        if ((mask & Rights.Read) == Rights.Read) return "SummaryRead";
        return mask == 0 ? "SummaryDenied" : "SummaryPartial";
    }

    public static string UnknownReason(ResourceAccess resource) => resource.Error != null ? "UnknownRead" :
        resource.Descriptor?.HasSpecialAces == true ? "UnknownConditional" : resource.IsReparsePoint ? "UnknownLink" :
        resource.Share != null ? "UnknownRemote" : "UnknownContext";

    public static string Diagnostic(string operation, int? errorCode, AccessState? state, bool remote) =>
        $"PermissionScope 1.0.0\nWindows: {Environment.OSVersion.Version}\nArchitecture: {System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture}\nOperation: {operation}\nError code: {errorCode?.ToString() ?? "none"}\nDecision: {state?.ToString() ?? "unavailable"}\nResource kind: {(remote ? "remote" : "local")}\nPaths, account names, SIDs and descriptors are intentionally excluded.";
}

public static class DescriptorParser
{
    public static bool SameDacl(string first, string second)
    {
        var a = new RawSecurityDescriptor(first); var b = new RawSecurityDescriptor(second);
        const ControlFlags semanticFlags = ControlFlags.DiscretionaryAclPresent | ControlFlags.DiscretionaryAclProtected;
        if ((a.ControlFlags & semanticFlags) != (b.ControlFlags & semanticFlags)) return false;
        if (a.DiscretionaryAcl is null || b.DiscretionaryAcl is null) return a.DiscretionaryAcl is null && b.DiscretionaryAcl is null;
        var left = new byte[a.DiscretionaryAcl.BinaryLength]; var right = new byte[b.DiscretionaryAcl.BinaryLength];
        a.DiscretionaryAcl.GetBinaryForm(left, 0); b.DiscretionaryAcl.GetBinaryForm(right, 0);
        return left.AsSpan().SequenceEqual(right);
    }

    public static DescriptorInfo Parse(string sddl, Func<string, string>? resolve = null)
    {
        var raw = new RawSecurityDescriptor(sddl);
        var aces = new List<AceEntry>();
        var special = false;
        if (raw.DiscretionaryAcl is { } acl)
            for (var i = 0; i < acl.Count; i++)
            {
                var ace = acl[i];
                var qualified = ace as QualifiedAce;
                var sid = qualified?.SecurityIdentifier.Value ?? "";
                var supported = ace is CommonAce { IsCallback: false } common &&
                    common.AceQualifier is AceQualifier.AccessAllowed or AceQualifier.AccessDenied;
                special |= !supported;
                aces.Add(new(i, sid, resolve?.Invoke(sid) ?? sid, ace.AceType.ToString(),
                    ace is KnownAce known ? unchecked((uint)known.AccessMask) : 0,
                    ace.AceFlags.ToString(), ace.IsInherited, (ace.AceFlags & AceFlags.InheritOnly) != 0, supported));
            }
        var canonical = new CommonSecurityDescriptor(false, false, raw).IsDiscretionaryAclCanonical;
        var bytes = new byte[raw.BinaryLength];
        raw.GetBinaryForm(bytes, 0);
        return new(sddl, Convert.ToHexString(SHA256.HashData(bytes)), raw.Owner?.Value, raw.Group?.Value,
            raw.DiscretionaryAcl is null, (raw.ControlFlags & ControlFlags.DiscretionaryAclProtected) != 0,
            canonical, special, aces);
    }
}

public sealed class GroupGraph
{
    private readonly Dictionary<string, List<GroupEdge>> parents = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<GroupEdge> edges = [];
    public IReadOnlyList<GroupEdge> Edges => edges.ToArray();
    public void Add(GroupEdge edge)
    {
        if (!edges.Add(edge)) return;
        if (!parents.TryGetValue(edge.MemberSid, out var list)) parents[edge.MemberSid] = list = [];
        list.Add(edge);
    }
    public IReadOnlyList<GroupEdge> FindPath(string member, string group, CancellationToken cancellation = default)
    {
        if (member == group) return [];
        var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { member };
        var prior = new Dictionary<string, GroupEdge>(StringComparer.OrdinalIgnoreCase);
        var queue = new Queue<string>();
        queue.Enqueue(member);
        while (queue.TryDequeue(out var current))
        {
            cancellation.ThrowIfCancellationRequested();
            if (!parents.TryGetValue(current, out var list)) continue;
            foreach (var edge in list)
            {
                if (!visited.Add(edge.GroupSid)) continue;
                prior[edge.GroupSid] = edge;
                if (string.Equals(edge.GroupSid, group, StringComparison.OrdinalIgnoreCase))
                {
                    var path = new List<GroupEdge>();
                    for (var cursor = group; prior.TryGetValue(cursor, out var step); cursor = step.MemberSid) path.Add(step);
                    path.Reverse();
                    return path;
                }
                queue.Enqueue(edge.GroupSid);
            }
        }
        return [];
    }
}

public static class AccessPathBuilder
{
    public static IReadOnlyList<AccessEvidence> Explain(DescriptorInfo descriptor, AccessIdentity identity,
        uint granted, string source, GroupGraph graph)
    {
        var sids = identity.Sids.GroupBy(s => s.Sid).ToDictionary(g => g.Key, g => g.First());
        sids[identity.Identity.Sid] = new(identity.Identity.Sid, 4);
        if (descriptor.Owner == identity.Identity.Sid) sids["S-1-3-4"] = new("S-1-3-4", 4);
        var remaining = Rights.FullControl;
        var evidence = new List<AccessEvidence>();
        foreach (var ace in descriptor.Aces)
        {
            if (ace.InheritOnly || !ace.Supported || !sids.TryGetValue(ace.Sid, out var tokenSid)) continue;
            var deny = ace.Type == "AccessDenied";
            if (deny ? !tokenSid.Enabled && !tokenSid.DenyOnly : !tokenSid.Enabled || tokenSid.DenyOnly) continue;
            var mask = Rights.MapGeneric(ace.Mask);
            var contribution = mask & remaining & (deny ? ~granted : granted);
            remaining &= ~mask;
            evidence.Add(new(ace.Index, ace.Sid, ace.Name, deny ? "DeniedBy" : "GrantedBy", mask, contribution,
                ace.Flags, source, graph.FindPath(identity.Identity.Sid, ace.Sid)));
        }
        if (descriptor.NullDacl)
            evidence.Add(new(-1, "S-1-1-0", "Everyone", "NullDacl", Rights.FullControl, granted, "", source, []));
        if (descriptor.Owner == identity.Identity.Sid && !descriptor.Aces.Any(a => a.Sid == "S-1-3-4"))
            evidence.Add(new(-1, identity.Identity.Sid, identity.Identity.Name, "OwnerRights", 0x60000,
                granted & 0x60000, "", source, []));
        return evidence;
    }
}

public static class FindingRules
{
    public static IReadOnlyList<RiskFinding> Evaluate(string path, DescriptorInfo descriptor)
    {
        var findings = new List<RiskFinding>();
        void Add(string rule, Severity severity, string evidence, string recommendation) =>
            findings.Add(new(rule, severity, path, evidence, recommendation));
        if (descriptor.NullDacl) Add("NullDacl", Severity.CriticalExposure, "DACL is NULL; discretionary access is unrestricted.", "Review whether unrestricted access is intended.");
        if (!descriptor.Canonical) Add("NoncanonicalDacl", Severity.Review, "ACE order is not canonical. Windows evaluates the stored order.", "Review ordering before changing permissions.");
        if (descriptor.Protected) Add("ProtectedDacl", Severity.Review, "Inheritance is disabled on this object.", "Confirm this permission boundary is intentional.");
        if (descriptor.HasSpecialAces) Add("SpecialAce", Severity.Review, "The descriptor contains conditional, object-specific or unsupported ACEs.", "Evaluate with the actual logon and resource claims.");
        foreach (var ace in descriptor.Aces.Where(a => !a.InheritOnly))
        {
            if (ace.Name == ace.Sid && ace.Sid.StartsWith("S-1-5-21-", StringComparison.Ordinal))
                Add("UnresolvedSid", Severity.Review, $"ACE #{ace.Index}: {ace.Sid}. Resolution failed; this does not prove the account was deleted.", "Check identity availability and domain connectivity.");
            if (ace.Type != "AccessAllowed" || ace.Sid is not ("S-1-1-0" or "S-1-5-11" or "S-1-5-32-545")) continue;
            var mask = Rights.MapGeneric(ace.Mask);
            if ((mask & 0xD0156) == 0) continue;
            Add("BroadAllow", (mask & 0xC0000) != 0 ? Severity.CriticalExposure : Severity.HighExposure,
                $"ACE #{ace.Index}: {ace.Name} has an allow entry for 0x{mask:X8}. This is ACL exposure, not a count of effective users.",
                "Review the broad principal and any applicable deny or share restrictions.");
        }
        return findings;
    }
}

public static class SnapshotComparer
{
    public static IReadOnlyList<SnapshotChange> Compare(PermissionSnapshot before, PermissionSnapshot after)
    {
        var result = new List<SnapshotChange>();
        var old = before.Resources.ToDictionary(r => r.Path, StringComparer.OrdinalIgnoreCase);
        foreach (var item in after.Resources)
        {
            if (!old.Remove(item.Path, out var prior))
            { result.Add(new(item.Path, "Added", "Object first observed in the later scan.", null, item.Descriptor?.Hash)); continue; }
            if (item.Error != null || prior.Error != null)
            { result.Add(new(item.Path, "Unknown", "One observation could not be read.", prior.Descriptor?.Hash, item.Descriptor?.Hash)); continue; }
            var sameDescriptor = item.Descriptor?.Hash == prior.Descriptor?.Hash && item.Share?.Descriptor?.Hash == prior.Share?.Descriptor?.Hash;
            var changedAccess = before.IdentitySid == after.IdentitySid && (prior.Decision?.EffectiveMask != item.Decision?.EffectiveMask || prior.Decision?.State != item.Decision?.State);
            if (sameDescriptor && !changedAccess) continue;
            var detail = sameDescriptor ? "The evaluated context changed while the descriptor remained unchanged." : "Security descriptor changed.";
            if (prior.Descriptor?.Protected != item.Descriptor?.Protected) detail += " Inheritance protection changed.";
            if (before.IdentitySid == after.IdentitySid && prior.Decision is { State: not AccessState.Unknown } a && item.Decision is { State: not AccessState.Unknown } b)
                detail += $" Gained: 0x{(b.EffectiveMask & ~a.EffectiveMask):X8}; lost: 0x{(a.EffectiveMask & ~b.EffectiveMask):X8}.";
            result.Add(new(item.Path, sameDescriptor ? "AccessChanged" : "Changed", detail, prior.Descriptor?.Hash, item.Descriptor?.Hash));
        }
        foreach (var item in old.Values) result.Add(new(item.Path, "NotObserved", "Not observed in the later scan; this does not prove deletion.", item.Descriptor?.Hash, null));
        return result;
    }
}

public static class ImpactSimulator
{
    public static string RemoveAce(string sddl, int index)
    {
        var raw = new RawSecurityDescriptor(sddl);
        if (raw.DiscretionaryAcl is not { } acl || index < 0 || index >= acl.Count) throw new ArgumentOutOfRangeException(nameof(index));
        acl.RemoveAce(index);
        return raw.GetSddlForm(AccessControlSections.All);
    }
    public static string AddAce(string sddl, string sid, uint mask, bool deny)
    {
        var raw = new RawSecurityDescriptor(sddl);
        if (raw.DiscretionaryAcl is null) throw new InvalidOperationException("Convert the NULL DACL explicitly before adding an entry.");
        var acl = raw.DiscretionaryAcl;
        var ace = new CommonAce(AceFlags.None, deny ? AceQualifier.AccessDenied : AceQualifier.AccessAllowed,
            unchecked((int)mask), new System.Security.Principal.SecurityIdentifier(sid), false, null);
        var index = 0;
        if (!deny) while (index < acl.Count && !acl[index].IsInherited) index++;
        acl.InsertAce(index, ace);
        return raw.GetSddlForm(AccessControlSections.All);
    }
}
