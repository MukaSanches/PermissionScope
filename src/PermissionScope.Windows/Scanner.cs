using System.ComponentModel;
using System.Diagnostics;
using System.Security;
using PermissionScope.Core;

namespace PermissionScope.Windows;

public sealed record ScanOptions(string Root, string? Identity = null, bool Recursive = true, bool IncludeFiles = false, bool ResolveLocalGroups = true);

public sealed class Scanner
{
    public Task<PermissionSnapshot> ScanAsync(ScanOptions options, IProgress<ScanProgress>? progress = null, CancellationToken cancellation = default) =>
        Task.Run(() => Scan(options, progress, cancellation), CancellationToken.None);

    private static PermissionSnapshot Scan(ScanOptions options, IProgress<ScanProgress>? progress, CancellationToken cancellation)
    {
        if (string.IsNullOrWhiteSpace(options.Root)) throw new ArgumentException("Enter a file or folder path.");
        var root = Path.GetFullPath(options.Root.Trim().Trim('"'));
        if (root.StartsWith("\\\\.\\", StringComparison.Ordinal)) throw new ArgumentException("Device paths are not supported.");
        var created = DateTimeOffset.UtcNow;
        var clock = Stopwatch.StartNew();
        var identities = new IdentityResolver();
        using var access = new EffectiveAccessResolver(identities, options.Identity);
        var reader = new AclReader(identities);
        var shares = new ShareResolver(reader);
        var results = new List<ResourceAccess>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var pending = new Stack<string>();
        pending.Push(root);
        long errors = 0;
        var cancelled = false;
        var identityWarnings = new List<string>();
        DirectoryIdentity? directoryIdentity = null;
        try
        {
            if (options.ResolveLocalGroups)
            {
                identityWarnings.AddRange(identities.ReadLocalGroups(access.Groups, cancellation));
                directoryIdentity = DirectoryGroupResolver.Resolve(access.Identity.Identity, access.Groups, identityWarnings, cancellation);
            }
            while (pending.TryPop(out var path))
            {
                cancellation.ThrowIfCancellationRequested();
                if (!seen.Add(path)) continue;
                var isDirectory = false;
                var reparse = false;
                try
                {
                    var attributes = File.GetAttributes(path);
                    isDirectory = (attributes & FileAttributes.Directory) != 0;
                    reparse = (attributes & FileAttributes.ReparsePoint) != 0;
                    cancellation.ThrowIfCancellationRequested();
                    var descriptor = reparse ? reader.ReadReparsePoint(path) : reader.Read(path);
                    cancellation.ThrowIfCancellationRequested();
                    var share = shares.Resolve(path);
                    var decision = access.Evaluate(descriptor, path, share, reparse);
                    results.Add(new(path, isDirectory, reparse, DateTimeOffset.UtcNow, descriptor, share, decision, FindingRules.Evaluate(path, descriptor), null));
                }
                catch (Exception error) when (IsExpected(error))
                {
                    errors++;
                    results.Add(new(path, isDirectory, reparse, DateTimeOffset.UtcNow, null, null, null, [],
                        new(path, ErrorCode(error), error.Message)));
                }
                progress?.Report(new(results.Count, errors, clock.Elapsed, path));
                if (!options.Recursive || !isDirectory || reparse) continue;
                try
                {
                    foreach (var child in Directory.EnumerateFileSystemEntries(path, "*", new EnumerationOptions { RecurseSubdirectories = false, IgnoreInaccessible = false, AttributesToSkip = 0 }))
                    {
                        cancellation.ThrowIfCancellationRequested();
                        if (options.IncludeFiles || (File.GetAttributes(child) & FileAttributes.Directory) != 0) pending.Push(child);
                    }
                }
                catch (Exception error) when (IsExpected(error))
                {
                    errors++;
                    var index = results.Count - 1;
                    results[index] = results[index] with { Error = new(path, ErrorCode(error), "Enumeration: " + error.Message) };
                }
            }
        }
        catch (OperationCanceledException) { cancelled = true; }
        return new(PermissionSnapshot.CurrentSchema, "1.0.0", Guid.NewGuid().ToString("N"), root, created,
            DateTimeOffset.UtcNow, cancelled, access.Identity.Identity.Sid, results, access.Groups.Edges)
        { IdentityWarnings = identityWarnings, DirectoryIdentity = directoryIdentity };
    }
    private static int ErrorCode(Exception error) => error is Win32Exception win32 ? win32.NativeErrorCode : error.HResult;
    private static bool IsExpected(Exception error) => error is IOException or UnauthorizedAccessException or Win32Exception or ArgumentException or SecurityException;
}
