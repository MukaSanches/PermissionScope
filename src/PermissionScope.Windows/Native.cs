using Microsoft.Win32.SafeHandles;
using System.Runtime.InteropServices;

namespace PermissionScope.Windows;

internal static class Native
{
    [StructLayout(LayoutKind.Sequential)] internal struct Luid { public uint Low; public int High; }
    [StructLayout(LayoutKind.Sequential)]
    internal struct AccessRequest
    { public uint DesiredAccess; public IntPtr PrincipalSelfSid; public IntPtr ObjectTypeList; public uint ObjectTypeListLength; public IntPtr OptionalArguments; }
    [StructLayout(LayoutKind.Sequential)]
    internal struct AccessReply
    { public uint ResultListLength; public IntPtr GrantedAccessMask; public IntPtr SaclEvaluationResults; public IntPtr Error; }
    [StructLayout(LayoutKind.Sequential)] internal struct SidAndAttributes { public IntPtr Sid; public uint Attributes; }
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    internal struct Share502
    {
        [MarshalAs(UnmanagedType.LPWStr)] public string Name;
        public uint Type;
        [MarshalAs(UnmanagedType.LPWStr)] public string Remark;
        public uint Permissions, MaxUses, CurrentUses;
        [MarshalAs(UnmanagedType.LPWStr)] public string Path;
        [MarshalAs(UnmanagedType.LPWStr)] public string Password;
        public uint Reserved;
        public IntPtr Descriptor;
    }
    [StructLayout(LayoutKind.Sequential)] internal struct Group0 { public IntPtr Name; }

    [DllImport("advapi32.dll", CharSet = CharSet.Unicode)]
    internal static extern uint GetNamedSecurityInfo(string name, int type, uint info, out IntPtr owner, out IntPtr group, out IntPtr dacl, out IntPtr sacl, out LocalMemory descriptor);
    [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool ConvertSecurityDescriptorToStringSecurityDescriptor(IntPtr descriptor, uint revision, uint info, out LocalMemory text, out uint length);
    [DllImport("kernel32.dll")] internal static extern IntPtr LocalFree(IntPtr memory);
    [DllImport("authz.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool AuthzInitializeResourceManager(uint flags, IntPtr access, IntPtr compute, IntPtr free, string name, out ResourceManagerHandle handle);
    [DllImport("authz.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool AuthzInitializeContextFromToken(uint flags, SafeAccessTokenHandle token, ResourceManagerHandle manager, IntPtr expiration, Luid id, IntPtr args, out ContextHandle context);
    [DllImport("authz.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool AuthzInitializeContextFromSid(uint flags, byte[] sid, ResourceManagerHandle manager, IntPtr expiration, Luid id, IntPtr args, out ContextHandle context);
    [DllImport("authz.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool AuthzAddSidsToContext(ContextHandle context, IntPtr sids, uint count, IntPtr restricted, uint restrictedCount, out ContextHandle result);
    [DllImport("authz.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool AuthzAccessCheck(uint flags, ContextHandle context, ref AccessRequest request, IntPtr audit, byte[] descriptor, IntPtr optional, uint optionalCount, ref AccessReply reply, IntPtr results);
    [DllImport("authz.dll")][return: MarshalAs(UnmanagedType.Bool)] internal static extern bool AuthzFreeResourceManager(IntPtr handle);
    [DllImport("authz.dll")][return: MarshalAs(UnmanagedType.Bool)] internal static extern bool AuthzFreeContext(IntPtr handle);
    [DllImport("authz.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool AuthzGetInformationFromContext(ContextHandle context, int info, uint size, out uint needed, IntPtr buffer);
    [DllImport("netapi32.dll", CharSet = CharSet.Unicode)] internal static extern int NetShareGetInfo(string server, string share, int level, out NetMemory buffer);
    [DllImport("netapi32.dll", CharSet = CharSet.Unicode)] internal static extern int NetLocalGroupEnum(string? server, int level, out NetMemory buffer, uint max, out uint read, out uint total, ref nuint resume);
    [DllImport("netapi32.dll", CharSet = CharSet.Unicode)] internal static extern int NetLocalGroupGetMembers(string? server, string group, int level, out NetMemory buffer, uint max, out uint read, out uint total, ref nuint resume);
    [DllImport("netapi32.dll")] internal static extern int NetApiBufferFree(IntPtr buffer);
    [DllImport("advapi32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool GetTokenInformation(SafeAccessTokenHandle token, int info, IntPtr buffer, uint length, out uint needed);
    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    internal static extern SafeFileHandle CreateFile(string name, uint access, uint share, IntPtr security, uint creation, uint flags, IntPtr template);
    [DllImport("advapi32.dll")] internal static extern uint GetSecurityInfo(SafeFileHandle handle, int type, uint info, out IntPtr owner, out IntPtr group, out IntPtr dacl, out IntPtr sacl, out LocalMemory descriptor);
    [DllImport("advapi32.dll")] internal static extern uint SetSecurityInfo(SafeFileHandle handle, int type, uint info, IntPtr owner, IntPtr group, IntPtr dacl, IntPtr sacl);
    [DllImport("advapi32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool GetSecurityDescriptorDacl(byte[] descriptor, [MarshalAs(UnmanagedType.Bool)] out bool present, out IntPtr dacl, [MarshalAs(UnmanagedType.Bool)] out bool defaulted);
    [StructLayout(LayoutKind.Sequential)]
    internal struct FileInformation
    { public uint Attributes; public System.Runtime.InteropServices.ComTypes.FILETIME Creation, Access, Write; public uint VolumeSerial, SizeHigh, SizeLow, Links, IndexHigh, IndexLow; }
    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool GetFileInformationByHandle(SafeFileHandle handle, out FileInformation information);
    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    internal static extern uint GetFinalPathNameByHandle(SafeFileHandle handle, System.Text.StringBuilder path, uint length, uint flags);
    [DllImport("mpr.dll", CharSet = CharSet.Unicode)] internal static extern int WNetGetUniversalName(string path, uint level, IntPtr buffer, ref uint size);
}

internal sealed class LocalMemory : SafeHandleZeroOrMinusOneIsInvalid
{
    public LocalMemory() : base(true) { }
    protected override bool ReleaseHandle() => Native.LocalFree(handle) == IntPtr.Zero;
}
internal sealed class NetMemory : SafeHandleZeroOrMinusOneIsInvalid
{
    public NetMemory() : base(true) { }
    protected override bool ReleaseHandle() => Native.NetApiBufferFree(handle) == 0;
}
internal sealed class ResourceManagerHandle : SafeHandleZeroOrMinusOneIsInvalid
{
    public ResourceManagerHandle() : base(true) { }
    protected override bool ReleaseHandle() => Native.AuthzFreeResourceManager(handle);
}
internal sealed class ContextHandle : SafeHandleZeroOrMinusOneIsInvalid
{
    public ContextHandle() : base(true) { }
    protected override bool ReleaseHandle() => Native.AuthzFreeContext(handle);
}
