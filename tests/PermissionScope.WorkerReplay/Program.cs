using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using PermissionScope.Core;
using PermissionScope.Windows;

namespace PermissionScope.WorkerReplay;

internal static class Program
{
    private sealed record ReplayReport(
        int SchemaVersion, string Status, string Mode, long InputBytes, string? InputSha256, int? WorkerExitCode,
        int Lines, int ProgressMessages, int Snapshots, int Objects, int Errors, int Unknown, bool Cancelled,
        double ReadMilliseconds, double DeserializeMilliseconds, double ValidationMilliseconds, double TotalMilliseconds,
        long AllocatedBytes, long WorkingSetBeforeBytes, long WorkingSetAfterDeserializeBytes, long WorkingSetAfterBytes);

    private sealed record ReadResult(PermissionSnapshot? Snapshot, long InputBytes, int Lines, int ProgressMessages,
        int Snapshots, double ReadMilliseconds, double DeserializeMilliseconds);

    private sealed record WorkerControl(int SchemaVersion, int ProcessId, long StartTimeUtcTicks);

    public static async Task<int> Main(string[] args)
    {
        try
        {
            var input = Optional(args, "--input");
            var cli = Optional(args, "--cli");
            if ((input is null) == (cli is null)) return 2;
            var expectedObjects = int.Parse(Required(args, "--expected-objects"), System.Globalization.CultureInfo.InvariantCulture);
            var expectedRoot = Optional(args, "--expected-root");
            var maxBytes = long.Parse(Optional(args, "--max-bytes") ?? "134217728", System.Globalization.CultureInfo.InvariantCulture);
            if (expectedObjects < 1 || maxBytes < 1) return 2;

            return input is not null
                ? await ReplayFileAsync(input, Required(args, "--expected-sha256").ToUpperInvariant(), expectedObjects, expectedRoot, maxBytes)
                : await RunWorkerAsync(cli!, Required(args, "--scan-root"), Required(args, "--worker-pid-file"), expectedObjects,
                    expectedRoot, maxBytes, int.Parse(Optional(args, "--timeout-seconds") ?? "120", System.Globalization.CultureInfo.InvariantCulture));
        }
        catch (Exception error) when (error is ArgumentException or FormatException or OverflowException)
        {
            return 2;
        }
        catch (Exception)
        {
            return Fail();
        }
    }

    private static async Task<int> ReplayFileAsync(string input, string expectedSha256, int expectedObjects, string? expectedRoot, long maxBytes)
    {
        if (expectedSha256.Length != 64) return 2;
        await using var stream = new FileStream(input, FileMode.Open, FileAccess.Read, FileShare.Read, 1 << 20, FileOptions.SequentialScan);
        var inputBytes = stream.Length;
        if (inputBytes > maxBytes) return 2;
        var actualSha256 = Convert.ToHexString(await SHA256.HashDataAsync(stream));
        if (!StringComparer.Ordinal.Equals(actualSha256, expectedSha256)) return 2;
        stream.Position = 0;
        using var reader = new StreamReader(stream, new UTF8Encoding(false, true), true, 1 << 20, leaveOpen: true);
        var report = await ReadValidateAsync(reader, "file", inputBytes, actualSha256, null, expectedObjects, expectedRoot, maxBytes, CancellationToken.None);
        Console.WriteLine(JsonSerializer.Serialize(report));
        return 0;
    }

    private static async Task<int> RunWorkerAsync(string cli, string scanRoot, string workerPidFile, int expectedObjects,
        string? expectedRoot, long maxBytes, int timeoutSeconds)
    {
        if (timeoutSeconds is < 1 or > 600 || !File.Exists(cli) || !Directory.Exists(scanRoot)) return 2;
        var start = new ProcessStartInfo(cli)
        {
            UseShellExecute = false, CreateNoWindow = true, RedirectStandardOutput = true, RedirectStandardError = true,
            StandardOutputEncoding = new UTF8Encoding(false, true), StandardErrorEncoding = new UTF8Encoding(false, true)
        };
        foreach (var argument in new[] { "scan", scanRoot, "--files", "--worker" }) start.ArgumentList.Add(argument);
        using var worker = new Process { StartInfo = start };
        if (!worker.Start()) return Fail();
        var errorOutput = worker.StandardError.ReadToEndAsync();
        try
        {
            WriteWorkerControl(workerPidFile, worker);
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutSeconds));
            var report = await ReadValidateAsync(worker.StandardOutput, "concurrent-worker", null, null, null,
                expectedObjects, expectedRoot ?? scanRoot, maxBytes, timeout.Token);
            await worker.WaitForExitAsync(timeout.Token);
            var errors = await errorOutput.WaitAsync(timeout.Token);
            if (worker.ExitCode != 0 || !string.IsNullOrEmpty(errors)) return Fail();
            Console.WriteLine(JsonSerializer.Serialize(report with { WorkerExitCode = worker.ExitCode }));
            return 0;
        }
        finally
        {
            if (!worker.HasExited)
            {
                try
                {
                    worker.Kill(entireProcessTree: true);
                    using var cleanupTimeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                    await worker.WaitForExitAsync(cleanupTimeout.Token);
                }
                catch (InvalidOperationException) when (worker.HasExited) { }
                catch (OperationCanceledException) { }
            }
        }
    }

    private static async Task<ReplayReport> ReadValidateAsync(StreamReader reader, string mode, long? knownInputBytes,
        string? inputSha256, int? workerExitCode, int expectedObjects, string? expectedRoot, long maxBytes, CancellationToken cancellation)
    {
        var process = Process.GetCurrentProcess();
        process.Refresh();
        var workingSetBefore = process.WorkingSet64;
        var allocatedBefore = GC.GetTotalAllocatedBytes(false);
        var totalClock = Stopwatch.StartNew();
        var phaseClock = new Stopwatch();
        var result = await ReadSnapshotAsync(reader, knownInputBytes, maxBytes, phaseClock, cancellation);
        process.Refresh();
        var workingSetAfterDeserialize = process.WorkingSet64;
        phaseClock.Restart();
        var snapshot = result.Snapshot;
        if (snapshot is null || result.Snapshots != 1 || snapshot.SchemaVersion != PermissionSnapshot.CurrentSchema ||
            snapshot.Cancelled || snapshot.Resources.Count != expectedObjects || string.IsNullOrWhiteSpace(snapshot.Root) ||
            (expectedRoot is not null && !StringComparer.Ordinal.Equals(snapshot.Root, expectedRoot)) ||
            snapshot.Resources.Any(resource => resource is null || string.IsNullOrWhiteSpace(resource.Path) || resource.Findings is null) ||
            snapshot.Resources.Select(resource => resource.Path).Distinct(StringComparer.OrdinalIgnoreCase).Count() != snapshot.Resources.Count) throw new InvalidDataException();
        var errors = snapshot.Resources.Count(resource => resource.Error is not null);
        var unknown = snapshot.Resources.Count(resource => resource.Decision is null || resource.Decision.State == AccessState.Unknown);
        var validationMilliseconds = phaseClock.Elapsed.TotalMilliseconds;
        totalClock.Stop();
        process.Refresh();
        return new ReplayReport(2, "passed", mode, result.InputBytes, inputSha256, workerExitCode, result.Lines,
            result.ProgressMessages, result.Snapshots, snapshot.Resources.Count, errors, unknown, snapshot.Cancelled,
            result.ReadMilliseconds, result.DeserializeMilliseconds, validationMilliseconds, totalClock.Elapsed.TotalMilliseconds,
            GC.GetTotalAllocatedBytes(false) - allocatedBefore, workingSetBefore, workingSetAfterDeserialize, process.WorkingSet64);
    }

    private static async Task<ReadResult> ReadSnapshotAsync(StreamReader reader, long? knownInputBytes, long maxBytes,
        Stopwatch phaseClock, CancellationToken cancellation)
    {
        double readMilliseconds = 0, deserializeMilliseconds = 0;
        long inputBytes = knownInputBytes ?? 0;
        var newlineBytes = Encoding.UTF8.GetByteCount(Environment.NewLine);
        var lines = 0;
        var progressMessages = 0;
        var snapshots = 0;
        var afterSnapshot = false;
        PermissionSnapshot? snapshot = null;
        while (true)
        {
            phaseClock.Restart();
            var line = await reader.ReadLineAsync(cancellation);
            readMilliseconds += phaseClock.Elapsed.TotalMilliseconds;
            if (line is null) break;
            if (knownInputBytes is null)
            {
                inputBytes = checked(inputBytes + Encoding.UTF8.GetByteCount(line) + newlineBytes);
                if (inputBytes > maxBytes) throw new InvalidDataException();
            }
            lines++;
            if (afterSnapshot) throw new InvalidDataException();
            phaseClock.Restart();
            var message = JsonSerializer.Deserialize<ScanMessage>(line, SnapshotJson.Options);
            deserializeMilliseconds += phaseClock.Elapsed.TotalMilliseconds;
            if (message is null || (message.Progress is null) == (message.Snapshot is null)) throw new InvalidDataException();
            if (message.Progress is not null) progressMessages++;
            if (message.Snapshot is not null)
            {
                snapshots++;
                snapshot = message.Snapshot;
                afterSnapshot = true;
            }
        }
        return new ReadResult(snapshot, inputBytes, lines, progressMessages, snapshots, readMilliseconds, deserializeMilliseconds);
    }

    private static void WriteWorkerControl(string path, Process worker)
    {
        var fullPath = Path.GetFullPath(path);
        var directory = Path.GetDirectoryName(fullPath) ?? throw new ArgumentException();
        if (!Directory.Exists(directory)) throw new ArgumentException();
        var temporary = Path.Combine(directory, $".{Path.GetFileName(fullPath)}.{Guid.NewGuid():N}.tmp");
        try
        {
            worker.Refresh();
            File.WriteAllText(temporary, JsonSerializer.Serialize(new WorkerControl(1, worker.Id, worker.StartTime.ToUniversalTime().Ticks)), new UTF8Encoding(false));
            File.Move(temporary, fullPath);
        }
        finally
        {
            if (File.Exists(temporary)) File.Delete(temporary);
        }
    }

    private static string Required(string[] args, string name) => Optional(args, name) ?? throw new ArgumentException();
    private static int Fail()
    {
        Console.Error.WriteLine("Worker replay failed without exposing input content.");
        return 1;
    }
    private static string? Optional(string[] args, string name)
    {
        var index = Array.IndexOf(args, name);
        return index >= 0 && index + 1 < args.Length && !args[index + 1].StartsWith("--", StringComparison.Ordinal) ? args[index + 1] : null;
    }
}
