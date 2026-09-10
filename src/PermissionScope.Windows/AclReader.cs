using System.ComponentModel;
using System.Runtime.InteropServices;
using PermissionScope.Core;

namespace PermissionScope.Windows;

public sealed class AclReader(IdentityResolver identities)
{
    public DescriptorInfo ReadReparsePoint(string path)
    {
        using var handle = Native.CreateFile(NativePath(path), 0x20000, 7, IntPtr.Zero, 3, 0x02200000, IntPtr.Zero);
        if (handle.IsInvalid) throw new Win32Exception(Marshal.GetLastWin32Error());
        var error = Native.GetSecurityInfo(handle, 1, 7, out _, out _, out _, out _, out var descriptor);
        using (descriptor)
        {
            if (error != 0) throw new Win32Exception((int)error);
            return ReadPointer(descriptor.DangerousGetHandle());
        }
    }
    public DescriptorInfo Read(string path)
    {
        var error = Native.GetNamedSecurityInfo(NativePath(path), 1, 7, out _, out _, out _, out _, out var buffer);
        using (buffer)
        {
            if (error != 0) throw new Win32Exception(unchecked((int)error));
            return ReadPointer(buffer.DangerousGetHandle());
        }
    }
    internal DescriptorInfo ReadPointer(IntPtr pointer)
    {
        if (pointer == IntPtr.Zero) throw new InvalidDataException("Windows returned no security descriptor.");
        if (!Native.ConvertSecurityDescriptorToStringSecurityDescriptor(pointer, 1, 7, out var text, out _))
            throw new Win32Exception(Marshal.GetLastWin32Error());
        using (text)
            return DescriptorParser.Parse(Marshal.PtrToStringUni(text.DangerousGetHandle()) ?? throw new InvalidDataException("Empty security descriptor."), sid => identities.Resolve(sid).Name);
    }
    internal static string NativePath(string path) => path.Length < 248 || path.StartsWith("\\\\?\\", StringComparison.Ordinal)
        ? path : path.StartsWith("\\\\", StringComparison.Ordinal) ? "\\\\?\\UNC\\" + path[2..] : "\\\\?\\" + Path.GetFullPath(path);
}

public sealed class ShareResolver(AclReader reader)
{
    private readonly Dictionary<string, ShareInfo> cache = new(StringComparer.OrdinalIgnoreCase);
    public ShareInfo? Resolve(string path)
    {
        var driveRoot = Path.GetPathRoot(path);
        if (driveRoot is { Length: >= 2 } && driveRoot[1] == ':' && new DriveInfo(driveRoot).DriveType == DriveType.Network)
        {
            uint size = 0;
            var result = Native.WNetGetUniversalName(path, 1, IntPtr.Zero, ref size);
            if (result != 234 || size > 1048576) throw new Win32Exception(result);
            var universalBuffer = Marshal.AllocHGlobal(checked((int)size));
            try
            {
                result = Native.WNetGetUniversalName(path, 1, universalBuffer, ref size);
                if (result != 0) throw new Win32Exception(result);
                path = Marshal.PtrToStringUni(Marshal.ReadIntPtr(universalBuffer)) ?? throw new InvalidDataException("Mapped drive has no UNC target.");
            }
            finally { Marshal.FreeHGlobal(universalBuffer); }
        }
        var normalized = path.StartsWith("\\\\?\\UNC\\", StringComparison.OrdinalIgnoreCase) ? "\\\\" + path[8..] : path;
        if (!normalized.StartsWith("\\\\", StringComparison.Ordinal) || normalized.StartsWith("\\\\?\\", StringComparison.Ordinal)) return null;
        var parts = normalized[2..].Split('\\', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 2) throw new ArgumentException("A UNC path must contain a server and share.");
        var key = $"{parts[0]}\\{parts[1]}";
        if (cache.TryGetValue(key, out var cached)) return cached;
        var error = Native.NetShareGetInfo($"\\\\{parts[0]}", parts[1], 502, out var buffer);
        using (buffer)
        {
            ShareInfo info;
            if (error != 0) info = new(parts[0], parts[1], null, null, $"Win32 {error}: {new Win32Exception(error).Message}");
            else
            {
                var share = Marshal.PtrToStructure<Native.Share502>(buffer.DangerousGetHandle());
                info = new(parts[0], parts[1], share.Path, reader.ReadPointer(share.Descriptor), null);
            }
            cache.Add(key, info);
            return info;
        }
    }
}
