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
        int SchemaVersion,
        string Status,
        long InputBytes,
        string InputSha256,
        int Lines,
        int ProgressMessages,
        int Snapshots,
        int Objects,
        int Errors,
        int Unknown,
        bool Cancelled,
        double ReadMilliseconds,
        double DeserializeMilliseconds,
        double ValidationMilliseconds,
        double TotalMilliseconds,
        long AllocatedBytes,
        long WorkingSetBeforeBytes,
        long WorkingSetAfterDeserializeBytes,
        long WorkingSetAfterBytes);

    public static async Task<int> Main(string[] args)
    {
        try
        {
            var input = Required(args, "--input");
            var expectedSha256 = Required(args, "--expected-sha256").ToUpperInvariant();
            var expectedObjects = int.Parse(Required(args, "--expected-objects"), System.Globalization.CultureInfo.InvariantCulture);
            var expectedRoot = Optional(args, "--expected-root");
            var maxBytes = long.Parse(Optional(args, "--max-bytes") ?? "134217728", System.Globalization.CultureInfo.InvariantCulture);
            if (expectedObjects < 1 || maxBytes < 1 || expectedSha256.Length != 64) return 2;

            await using var stream = new FileStream(input, FileMode.Open, FileAccess.Read, FileShare.Read, 1 << 20, FileOptions.SequentialScan);
            var inputBytes = stream.Length;
            if (inputBytes > maxBytes) return 2;
            var actualSha256 = Convert.ToHexString(await SHA256.HashDataAsync(stream));
            if (!StringComparer.Ordinal.Equals(actualSha256, expectedSha256)) return 2;
            stream.Position = 0;

            var process = Process.GetCurrentProcess();
            process.Refresh();
            var workingSetBefore = process.WorkingSet64;
            var allocatedBefore = GC.GetTotalAllocatedBytes(false);
            var totalClock = Stopwatch.StartNew();
            var phaseClock = new Stopwatch();
            double readMilliseconds = 0, deserializeMilliseconds = 0;
            var lines = 0;
            var progressMessages = 0;
            var snapshots = 0;
            var afterSnapshot = false;
            PermissionSnapshot? snapshot = null;

            using (var reader = new StreamReader(stream, new UTF8Encoding(false, true), true, 1 << 20, leaveOpen: true))
            {
                while (true)
                {
                    phaseClock.Restart();
                    var line = await reader.ReadLineAsync(CancellationToken.None);
                    readMilliseconds += phaseClock.Elapsed.TotalMilliseconds;
                    if (line is null) break;
                    lines++;
                    if (afterSnapshot) return Fail();

                    phaseClock.Restart();
                    var message = JsonSerializer.Deserialize<ScanMessage>(line, SnapshotJson.Options);
                    deserializeMilliseconds += phaseClock.Elapsed.TotalMilliseconds;
                    if (message is null || (message.Progress is null) == (message.Snapshot is null)) return Fail();
                    if (message.Progress is not null) progressMessages++;
                    if (message.Snapshot is not null)
                    {
                        snapshots++;
                        snapshot = message.Snapshot;
                        afterSnapshot = true;
                    }
                }
            }

            process.Refresh();
            var workingSetAfterDeserialize = process.WorkingSet64;
            phaseClock.Restart();
            if (snapshot is null || snapshots != 1 || snapshot.SchemaVersion != PermissionSnapshot.CurrentSchema ||
                snapshot.Cancelled || snapshot.Resources.Count != expectedObjects || string.IsNullOrWhiteSpace(snapshot.Root) ||
                (expectedRoot is not null && !StringComparer.Ordinal.Equals(snapshot.Root, expectedRoot)) ||
                snapshot.Resources.Any(resource => resource is null || string.IsNullOrWhiteSpace(resource.Path) || resource.Findings is null) ||
                snapshot.Resources.Select(resource => resource.Path).Distinct(StringComparer.OrdinalIgnoreCase).Count() != snapshot.Resources.Count) return Fail();
            var errors = snapshot.Resources.Count(resource => resource.Error is not null);
            var unknown = snapshot.Resources.Count(resource => resource.Decision is null || resource.Decision.State == AccessState.Unknown);
            var validationMilliseconds = phaseClock.Elapsed.TotalMilliseconds;
            totalClock.Stop();
            process.Refresh();
            var report = new ReplayReport(1, "passed", inputBytes, actualSha256, lines, progressMessages, snapshots,
                snapshot.Resources.Count, errors, unknown, snapshot.Cancelled, readMilliseconds,
                deserializeMilliseconds, validationMilliseconds, totalClock.Elapsed.TotalMilliseconds,
                GC.GetTotalAllocatedBytes(false) - allocatedBefore, workingSetBefore, workingSetAfterDeserialize, process.WorkingSet64);
            Console.WriteLine(JsonSerializer.Serialize(report));
            return 0;
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
