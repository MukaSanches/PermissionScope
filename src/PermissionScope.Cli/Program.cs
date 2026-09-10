using System.ComponentModel;
using System.Text.Json;
using PermissionScope.Core;
using PermissionScope.Windows;

namespace PermissionScope.Cli;

internal static class Program
{
    public static async Task<int> Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        using var cancellation = new CancellationTokenSource();
        Console.CancelKeyPress += (_, e) => { e.Cancel = true; cancellation.Cancel(); };
        try
        {
            if (args.Length == 0 || args[0] is "--help" or "help")
            {
                Console.WriteLine("PermissionScope 1.0.0 — Windows access analysis\n\nscan <path> [--user <name-or-SID>] [--files] [--shallow] [--save] [--output <file.json>] [--json]\nexplain <path> [--user <name-or-SID>] [--json]\nlist [--json]\nexport <snapshot-id-or-json> --format html|csv|json|xlsx|pdf --output <file>\ncompare <before-id-or-json> <after-id-or-json> [--json]\nfindings <snapshot-id-or-json>\nsimulate <path> --remove-ace <index> [--user <name-or-SID>]\ndemo --output <directory> (synthetic examples in five formats)\n\nExit: 0 success; 1 failure; 2 invalid arguments; 3 incomplete/unknown; 130 cancelled.\nDecisions cover discretionary permissions, not actual file-open success. No ACLs are modified.");
                return 0;
            }
            string? Option(string key)
            {
                var index = Array.IndexOf(args, key);
                if (index < 0) return null;
                if (index + 1 >= args.Length || args[index + 1].StartsWith("--")) throw new ArgumentException($"Missing value for {key}.");
                return args[index + 1];
            }
            string Pos(int index) => args.Length > index && !args[index].StartsWith("--") ? args[index] : throw new ArgumentException("Missing required argument. Run --help.");
            PermissionSnapshot Load(string value) => File.Exists(value) ? SnapshotJson.Parse(File.ReadAllText(value)) : new SnapshotStore().Load(value);
            void Json(object value) => Console.WriteLine(JsonSerializer.Serialize(value, SnapshotJson.Options));
            var json = args.Contains("--json");
            switch (args[0])
            {
                case "demo":
                    {
                        var directory = Path.GetFullPath(Option("--output") ?? throw new ArgumentException("demo requires --output <directory>."));
                        Directory.CreateDirectory(directory);
                        var fixture = DemoFixture.Create();
                        foreach (var format in new[] { "html", "csv", "json", "xlsx", "pdf" })
                            ReportExporter.Export(fixture, Path.Combine(directory, "permissionscope-demo." + format), format);
                        ReportExporter.Export(DemoFixture.Create(true), Path.Combine(directory, "permissionscope-demo-after.json"), "json");
                        Console.WriteLine("Synthetic Authz demonstration exported. No accounts, groups or ACLs were created or changed.");
                        return 0;
                    }
                case "scan":
                case "explain":
                    {
                        var worker = args.Contains("--worker");
                        var compact = new JsonSerializerOptions(SnapshotJson.Options) { WriteIndented = false };
                        var progressClock = System.Diagnostics.Stopwatch.StartNew();
                        IProgress<ScanProgress>? progress = worker ? new InlineProgress<ScanProgress>(value =>
                        {
                            if (progressClock.ElapsedMilliseconds < 100) return;
                            progressClock.Restart(); Console.WriteLine(JsonSerializer.Serialize(new ScanMessage(Progress: value), compact));
                        }) : null;
                        var snapshot = await new Scanner().ScanAsync(new(Pos(1), Option("--user"), args[0] != "explain" && !args.Contains("--shallow"), args.Contains("--files")), progress, cancellation.Token);
                        if (worker) { Console.WriteLine(JsonSerializer.Serialize(new ScanMessage(Snapshot: snapshot), compact)); return 0; }
                        if (args.Contains("--save")) new SnapshotStore().Save(snapshot);
                        if (Option("--output") is { } output) ReportExporter.Export(snapshot, output, "json");
                        if (json) Json(snapshot);
                        else
                        {
                            Console.WriteLine($"{snapshot.Root}\nSnapshot: {snapshot.Id}\n{snapshot.Resources.Count:N0} objects · {snapshot.Resources.Count(r => r.Error != null)} errors · {snapshot.Resources.Sum(r => r.Findings.Count)} findings");
                            foreach (var r in snapshot.Resources)
                            {
                                Console.WriteLine($"{r.Decision?.State.ToString() ?? "Unknown",-8} {r.Path}  {r.Decision?.Identity}  NTFS=0x{r.Decision?.GrantedMask:X8}");
                                if (r.Error != null) Console.WriteLine($"  Win32/HRESULT {r.Error.Code}: {r.Error.Message}");
                                if (r.Decision?.Limitation != null) Console.WriteLine("  " + r.Decision.Limitation);
                                if (args[0] == "explain" && r.Decision is { } decision)
                                {
                                    Console.WriteLine(decision.Basis);
                                    foreach (var c in decision.Capabilities) Console.WriteLine($"  {c.Key,-24} {c.State}");
                                    foreach (var e in decision.Evidence) Console.WriteLine($"  ACE #{e.AceIndex}: {e.Identity} → {e.Relation} 0x{e.Mask:X8}, contributes 0x{e.ContributingMask:X8} [{e.Flags}]");
                                }
                            }
                        }
                        return snapshot.Cancelled ? 130 : snapshot.Resources.Any(r => r.Error != null || r.Decision?.State == AccessState.Unknown) ? 3 : 0;
                    }
                case "list":
                    {
                        var list = new SnapshotStore().List();
                        if (json) Json(list); else foreach (var s in list) Console.WriteLine($"{s.Id}  {s.CreatedAt:g}  {s.Objects,7}  {s.Root}");
                        return 0;
                    }
                case "export": ReportExporter.Export(Load(Pos(1)), Option("--output") ?? throw new ArgumentException("--output is required."), Option("--format") ?? "html"); return 0;
                case "compare":
                    {
                        var changes = SnapshotComparer.Compare(Load(Pos(1)), Load(Pos(2)));
                        if (json) Json(changes); else foreach (var c in changes) Console.WriteLine($"{c.Kind,-12} {c.Path}\n  {c.Detail}");
                        return 0;
                    }
                case "findings": Json(Load(Pos(1)).Resources.SelectMany(r => r.Findings)); return 0;
                case "simulate":
                    {
                        var identities = new IdentityResolver();
                        using var access = new EffectiveAccessResolver(identities, Option("--user"));
                        var path = Path.GetFullPath(Pos(1));
                        var reader = new AclReader(identities);
                        var reparse = File.GetAttributes(path).HasFlag(FileAttributes.ReparsePoint);
                        var before = reparse ? reader.ReadReparsePoint(path) : reader.Read(path);
                        var share = new ShareResolver(reader).Resolve(path);
                        var after = DescriptorParser.Parse(ImpactSimulator.RemoveAce(before.Sddl, int.Parse(Option("--remove-ace") ?? throw new ArgumentException("--remove-ace is required."))));
                        var beforeDecision = access.Evaluate(before, path, share, reparse);
                        var afterDecision = access.Evaluate(after, path, share, reparse);
                        Json(new { Mode = "Simulation only; no ACL changes", Path = path, Before = beforeDecision, After = afterDecision, Scope = access.Identity.Context, ProposedSddl = after.Sddl });
                        return afterDecision.State == AccessState.Unknown ? 3 : 0;
                    }
                default: throw new ArgumentException("Unknown command. Run --help.");
            }
        }
        catch (OperationCanceledException) { return 130; }
        catch (ArgumentException e) { Console.Error.WriteLine(e.Message); return 2; }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException or Win32Exception or JsonException or InvalidOperationException or System.Security.Principal.IdentityNotMappedException or Microsoft.Data.Sqlite.SqliteException)
        { Console.Error.WriteLine(e.Message); return 1; }
    }
}

internal sealed class InlineProgress<T>(Action<T> report) : IProgress<T>
{
    public void Report(T value) => report(value);
}
