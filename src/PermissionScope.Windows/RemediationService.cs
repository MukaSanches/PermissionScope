using Microsoft.Win32.SafeHandles;
using PermissionScope.Core;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Text.Json;

namespace PermissionScope.Windows;

public sealed record RemediationPlan(string Id, string Path, string FileIdentity, DateTimeOffset CreatedAt, string OriginalSddl, string OriginalHash, string ProposedSddl, string ProposedHash);
public sealed record RemediationReceipt(RemediationPlan Plan, string VerifiedHash, DateTimeOffset AppliedAt, bool RolledBack);

public static class RemediationService
{
    public static RemediationPlan Prepare(string path, string observedHash, string proposedSddl)
    {
        path = Path.GetFullPath(path);
        using var handle = OpenFile(path);
        var original = Read(handle);
        if (original.Hash != observedHash) throw new InvalidOperationException("Permissions changed since analysis. Rescan before applying a change.");
        var proposed = DescriptorParser.Parse(proposedSddl);
        if (proposed.Owner != original.Owner || proposed.PrimaryGroup != original.PrimaryGroup || proposed.HasSpecialAces || original.HasSpecialAces)
            throw new InvalidOperationException("This workflow supports ordinary DACL changes only; owner, group and conditional policies cannot be changed.");
        return new(Guid.NewGuid().ToString("N"), path, FileIdentity(handle), DateTimeOffset.UtcNow, original.Sddl, original.Hash, proposed.Sddl, proposed.Hash);
    }
    public static RemediationReceipt Apply(RemediationPlan plan, string confirmedPath, string? journalDirectory = null)
    {
        if (!string.Equals(plan.Path, confirmedPath, StringComparison.OrdinalIgnoreCase)) throw new InvalidOperationException("Confirm the exact file path before applying.");
        if (DescriptorParser.Parse(plan.OriginalSddl).Hash != plan.OriginalHash || DescriptorParser.Parse(plan.ProposedSddl).Hash != plan.ProposedHash)
            throw new InvalidDataException("Plan integrity check failed.");
        using var handle = OpenFile(plan.Path);
        if (FileIdentity(handle) != plan.FileIdentity) throw new InvalidOperationException("The original file has been replaced. Nothing was applied.");
        if (Read(handle).Hash != plan.OriginalHash) throw new InvalidOperationException("Permissions changed after the proposal. Nothing was applied.");
        var journal = JournalPath(plan.Id, journalDirectory);
        AtomicFile.WriteText(journal, JsonSerializer.Serialize(plan, SnapshotJson.Options));
        // A handle opened without FILE_SHARE_DELETE pins the object against replacement or rename.
        if (Read(handle).Hash != plan.OriginalHash) throw new InvalidOperationException("Permissions changed during preparation. Nothing was applied.");
        WriteDacl(handle, plan.ProposedSddl);
        var verified = Read(handle);
        if (!SameDacl(verified.Sddl, plan.ProposedSddl)) throw new InvalidOperationException("Write completed but verification differs. Original descriptor is recorded in " + journal);
        var receipt = new RemediationReceipt(plan, verified.Hash, DateTimeOffset.UtcNow, false);
        AtomicFile.WriteText(journal, JsonSerializer.Serialize(receipt, SnapshotJson.Options));
        return receipt;
    }
    public static RemediationReceipt Rollback(RemediationReceipt receipt, string confirmedPath, string? journalDirectory = null)
    {
        if (receipt.RolledBack || !string.Equals(receipt.Plan.Path, confirmedPath, StringComparison.OrdinalIgnoreCase)) throw new InvalidOperationException("Confirm the original file path for rollback.");
        using var handle = OpenFile(receipt.Plan.Path);
        if (FileIdentity(handle) != receipt.Plan.FileIdentity) throw new InvalidOperationException("The original file has been replaced. Rollback is not safe.");
        if (Read(handle).Hash != receipt.VerifiedHash) throw new InvalidOperationException("Permissions changed after application. Automatic rollback would overwrite a newer change.");
        WriteDacl(handle, receipt.Plan.OriginalSddl);
        var verified = Read(handle);
        if (!SameDacl(verified.Sddl, receipt.Plan.OriginalSddl)) throw new InvalidOperationException("Rollback could not be verified.");
        var rolledBack = receipt with { RolledBack = true, VerifiedHash = verified.Hash };
        AtomicFile.WriteText(JournalPath(receipt.Plan.Id, journalDirectory), JsonSerializer.Serialize(rolledBack, SnapshotJson.Options));
        return rolledBack;
    }
    private static string JournalPath(string id, string? directory)
    {
        if (!Guid.TryParseExact(id, "N", out _)) throw new InvalidDataException("Invalid remediation identifier.");
        return Path.Combine(directory ?? Path.Combine(SnapshotStore.DefaultDirectory, "changes"), id + ".json");
    }
    private static SafeFileHandle OpenFile(string path)
    {
        if (path.StartsWith("\\\\", StringComparison.Ordinal)) throw new InvalidOperationException("Remediation is restricted to local files; server identity and descendant propagation must be verified separately.");
        var handle = Native.CreateFile(AclReader.NativePath(path), 0x60080, 3, IntPtr.Zero, 3, 0x00200000, IntPtr.Zero);
        if (handle.IsInvalid) { var error = Marshal.GetLastWin32Error(); handle.Dispose(); throw new Win32Exception(error); }
        try
        {
            // Resolve the opened object: a drive letter or parent link can lead to a remote share.
            var finalPath = new System.Text.StringBuilder(32768);
            var length = Native.GetFinalPathNameByHandle(handle, finalPath, (uint)finalPath.Capacity, 1);
            if (length == 0 || length >= finalPath.Capacity || !finalPath.ToString().StartsWith(@"\\?\Volume{", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("The opened file could not be verified on a local volume. Nothing was changed.");
            if (!Native.GetFileInformationByHandle(handle, out var information)) throw new Win32Exception(Marshal.GetLastWin32Error());
            if ((information.Attributes & (0x10 | 0x400)) != 0 || information.Links != 1)
                throw new InvalidOperationException("Remediation supports ordinary files with one hard link. Directories and links require broader impact analysis.");
            return handle;
        }
        catch { handle.Dispose(); throw; }
    }
    private static DescriptorInfo Read(SafeFileHandle handle)
    {
        var result = Native.GetSecurityInfo(handle, 1, 7, out _, out _, out _, out _, out var descriptor);
        using (descriptor)
        {
            if (result != 0) throw new Win32Exception((int)result);
            return new AclReader(new IdentityResolver()).ReadPointer(descriptor.DangerousGetHandle());
        }
    }
    private static string FileIdentity(SafeFileHandle handle)
    {
        if (!Native.GetFileInformationByHandle(handle, out var info)) throw new Win32Exception(Marshal.GetLastWin32Error());
        return $"{info.VolumeSerial:X8}:{info.IndexHigh:X8}{info.IndexLow:X8}";
    }
    private static void WriteDacl(SafeFileHandle handle, string sddl)
    {
        var raw = new RawSecurityDescriptor(sddl); var bytes = new byte[raw.BinaryLength]; raw.GetBinaryForm(bytes, 0);
        var pinned = GCHandle.Alloc(bytes, GCHandleType.Pinned);
        try
        {
            if (!Native.GetSecurityDescriptorDacl(bytes, out _, out var dacl, out _)) throw new Win32Exception(Marshal.GetLastWin32Error());
            var protection = (raw.ControlFlags & ControlFlags.DiscretionaryAclProtected) != 0 ? 0x80000000u : 0x20000000u;
            var result = Native.SetSecurityInfo(handle, 1, 4 | protection, IntPtr.Zero, IntPtr.Zero, dacl, IntPtr.Zero);
            if (result != 0) throw new Win32Exception((int)result);
        }
        finally { pinned.Free(); }
    }
    private static bool SameDacl(string first, string second)
    {
        var a = new RawSecurityDescriptor(first); var b = new RawSecurityDescriptor(second);
        return a.GetSddlForm(AccessControlSections.Access) == b.GetSddlForm(AccessControlSections.Access);
    }
}
