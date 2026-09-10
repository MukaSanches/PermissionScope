using System.Collections.Concurrent;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Security.Principal;
using PermissionScope.Core;

namespace PermissionScope.Windows;

public sealed class IdentityResolver
{
    private readonly ConcurrentDictionary<string, Identity> cache = new(StringComparer.OrdinalIgnoreCase);
    public Identity Resolve(string sid) => cache.GetOrAdd(sid, static value =>
    {
        try
        {
            var parsed = new SecurityIdentifier(value);
            var name = parsed.Translate(typeof(NTAccount)).Value;
            return new(value, name, "Principal", true);
        }
        catch (IdentityNotMappedException) { return new(value, value, "Unresolved", false); }
        catch (SystemException e) when (e is ArgumentException or System.Security.SecurityException) { return new(value, value, "Unresolved", false); }
    });
    public Identity Find(string name)
    {
        if (name.StartsWith("S-1-", StringComparison.OrdinalIgnoreCase)) return Resolve(new SecurityIdentifier(name).Value);
        var sid = (SecurityIdentifier)new NTAccount(name).Translate(typeof(SecurityIdentifier));
        return Resolve(sid.Value);
    }
    public static IReadOnlyList<TokenSid> ReadGroups(IntPtr buffer, uint length)
    {
        if (length < 4) throw new InvalidDataException("Token group buffer is truncated.");
        var count = Marshal.ReadInt32(buffer);
        var offset = IntPtr.Size == 8 ? 8 : 4;
        var stride = Marshal.SizeOf<Native.SidAndAttributes>();
        if (count < 0 || (long)offset + (long)count * stride > length) throw new InvalidDataException("Invalid token group count.");
        var result = new List<TokenSid>(count);
        for (var i = 0; i < count; i++)
        {
            var entry = Marshal.PtrToStructure<Native.SidAndAttributes>(buffer + offset + i * stride);
            result.Add(new(new SecurityIdentifier(entry.Sid).Value, entry.Attributes));
        }
        return result;
    }
    public IReadOnlyList<string> ReadLocalGroups(GroupGraph graph, CancellationToken cancellation, string? server = null)
    {
        var errors = new List<string>();
        nuint resume = 0;
        int status;
        do
        {
            cancellation.ThrowIfCancellationRequested();
            status = Native.NetLocalGroupEnum(server, 0, out var buffer, 65536, out var read, out _, ref resume);
            using (buffer)
            {
                if (status is not (0 or 234)) { errors.Add(new Win32Exception(status).Message); break; }
                for (var i = 0; i < read; i++)
                {
                    cancellation.ThrowIfCancellationRequested();
                    var item = Marshal.PtrToStructure<Native.Group0>(buffer.DangerousGetHandle() + i * IntPtr.Size);
                    var name = Marshal.PtrToStringUni(item.Name) ?? "";
                    Identity group;
                    try { group = Find($"{(server?.TrimStart('\\') ?? Environment.MachineName)}\\{name}"); }
                    catch (IdentityNotMappedException) { errors.Add($"Group could not be resolved: {name}"); continue; }
                    nuint memberResume = 0;
                    int memberStatus;
                    do
                    {
                        cancellation.ThrowIfCancellationRequested();
                        memberStatus = Native.NetLocalGroupGetMembers(server, name, 0, out var members, 65536, out var memberRead, out _, ref memberResume);
                        using (members)
                        {
                            if (memberStatus is not (0 or 234)) { errors.Add($"{name}: {new Win32Exception(memberStatus).Message}"); break; }
                            for (var j = 0; j < memberRead; j++)
                            {
                                var sidPointer = Marshal.ReadIntPtr(members.DangerousGetHandle(), j * IntPtr.Size);
                                graph.Add(new(new SecurityIdentifier(sidPointer).Value, group.Sid, $"NetLocalGroupGetMembers: {server ?? Environment.MachineName} / {name}"));
                            }
                        }
                    } while (memberStatus == 234);
                }
            }
        } while (status == 234);
        return errors;
    }
}
