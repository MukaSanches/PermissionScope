using System.Diagnostics;
using System.Text.Json;
using PermissionScope.Core;

namespace PermissionScope.Windows;

public sealed record ScanMessage(ScanProgress? Progress = null, PermissionSnapshot? Snapshot = null);
public static class ScanWorker
{
    public static async Task<PermissionSnapshot> ScanAsync(ScanOptions options, IProgress<ScanProgress>? progress, CancellationToken cancellation)
    {
        var executable = Path.Combine(AppContext.BaseDirectory, "permissionscope-cli.exe");
        if (!File.Exists(executable))
        {
#if DEBUG
            return await new Scanner().ScanAsync(options, progress, cancellation);
#else
            throw new FileNotFoundException("The scan worker is missing. Extract or install the complete PermissionScope package.", executable);
#endif
        }
        var start = new ProcessStartInfo(executable)
        { UseShellExecute = false, CreateNoWindow = true, RedirectStandardOutput = true, RedirectStandardError = true, StandardOutputEncoding = System.Text.Encoding.UTF8, StandardErrorEncoding = System.Text.Encoding.UTF8 };
        foreach (var argument in new[] { "scan", options.Root, "--worker" }) start.ArgumentList.Add(argument);
        if (options.Identity != null) { start.ArgumentList.Add("--user"); start.ArgumentList.Add(options.Identity); }
        if (!options.Recursive) start.ArgumentList.Add("--shallow");
        if (options.IncludeFiles) start.ArgumentList.Add("--files");
        using var process = Process.Start(start) ?? throw new InvalidOperationException("Could not start the scan worker.");
        var errorOutput = process.StandardError.ReadToEndAsync();
        try
        {
            PermissionSnapshot? snapshot = null;
            while (await process.StandardOutput.ReadLineAsync(cancellation) is { } line)
            {
                var message = JsonSerializer.Deserialize<ScanMessage>(line, SnapshotJson.Options) ?? throw new InvalidDataException("The scan worker returned an empty message.");
                if (message.Progress is { } update) progress?.Report(update);
                if (message.Snapshot is { } result) snapshot = result;
            }
            await process.WaitForExitAsync(cancellation);
            var errors = await errorOutput;
            if (snapshot == null) throw new InvalidOperationException(string.IsNullOrWhiteSpace(errors) ? "The scan worker ended before producing a result." : errors.Trim());
            return snapshot;
        }
        finally
        {
            if (!process.HasExited) { process.Kill(entireProcessTree: true); await process.WaitForExitAsync(); }
        }
    }
}
