using System.Diagnostics;
using System.IO.Compression;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text.Json;
using System.Xml.Linq;
using PermissionScope.Core;
using PermissionScope.Windows;

var failures = new List<string>();
var timings = new List<string>();
var passed = 0;
var testRoot = Path.Combine(Path.GetTempPath(), "PermissionScope-tests-" + Guid.NewGuid().ToString("N"));
Directory.CreateDirectory(testRoot);
void Assert(bool condition, string message = "Assertion failed") { if (!condition) throw new InvalidOperationException(message); }
void Test(string name, Action test)
{
    var clock = Stopwatch.StartNew();
    try { test(); passed++; Console.WriteLine($"PASS {name} ({clock.Elapsed.TotalMilliseconds:F1} ms)"); }
    catch (Exception e) { failures.Add(name + ": " + e); Console.WriteLine($"FAIL {name}: {e.Message}"); }
}
async Task TestAsync(string name, Func<Task> test)
{
    var clock = Stopwatch.StartNew();
    try { await test(); passed++; Console.WriteLine($"PASS {name} ({clock.Elapsed.TotalMilliseconds:F1} ms)"); }
    catch (Exception e) { failures.Add(name + ": " + e); Console.WriteLine($"FAIL {name}: {e.Message}"); }
}
void Throws<T>(Action action) where T : Exception { try { action(); } catch (T) { return; } throw new InvalidOperationException("Expected " + typeof(T).Name); }
const string fixtureSid = "S-1-5-21-111111111-222222222-333333333-1001";
Test("Locale exact match", () => Assert(LocaleResolver.Resolve("pt-BR", ["en-US", "pt-BR"]).ResourceTag == "pt-BR"));
Test("Locale language fallback", () => Assert(LocaleResolver.Resolve("es-MX", ["en-US", "es"]).ResourceTag == "es"));
Test("Locale regional sibling fallback", () => Assert(LocaleResolver.Resolve("pt-PT", ["en-US", "pt-BR"]).ResourceTag == "pt-BR"));
Test("Locale fallback preserves regional formatting", () => Assert(LocaleResolver.Resolve("de-AT", ["en-US"]).LanguageTag == "de-AT"));
Test("Locale Arabic regional RTL", () => Assert(LocaleResolver.Resolve("ar-SA", ["en-US", "ar"]).IsRightToLeft));
Test("Locale Persian RTL with English fallback", () => Assert(LocaleResolver.Resolve("fa-IR", ["en-US"]).IsRightToLeft));
Test("Locale Chinese script fallback", () => Assert(LocaleResolver.Resolve("zh-TW", ["en-US", "zh-Hant", "zh-Hans"]).ResourceTag == "zh-Hant"));
Test("Locale Chinese does not substitute scripts", () => Assert(LocaleResolver.Resolve("zh-TW", ["en-US", "zh-Hans"]).ResourceTag == "en-US"));
var resolver = new IdentityResolver();
Test("Isolated data directory rejects relative paths", () =>
{
    var previous = Environment.GetEnvironmentVariable("PERMISSIONSCOPE_DATA_DIR");
    try { Environment.SetEnvironmentVariable("PERMISSIONSCOPE_DATA_DIR", "relative"); Throws<ArgumentException>(() => _ = SnapshotStore.DefaultDirectory); }
    finally { Environment.SetEnvironmentVariable("PERMISSIONSCOPE_DATA_DIR", previous); }
});
Test("Isolated data directory accepts an explicit absolute path", () =>
{
    var previous = Environment.GetEnvironmentVariable("PERMISSIONSCOPE_DATA_DIR");
    try { Environment.SetEnvironmentVariable("PERMISSIONSCOPE_DATA_DIR", testRoot); Assert(SnapshotStore.DefaultDirectory == testRoot); }
    finally { Environment.SetEnvironmentVariable("PERMISSIONSCOPE_DATA_DIR", previous); }
});
using var isolated = new EffectiveAccessResolver(resolver, fixtureSid, true);
using (var corpus = JsonDocument.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "corpus", "discretionary-v1.json"))))
{
    Assert(corpus.RootElement.GetProperty("identitySid").GetString() == fixtureSid);
    foreach (var fixture in corpus.RootElement.GetProperty("cases").EnumerateArray())
        Test("Corpus: " + fixture.GetProperty("name").GetString(), () => Assert(isolated.CheckDiscretionary(fixture.GetProperty("sddl").GetString()!) == Convert.ToUInt32(fixture.GetProperty("mask").GetString(), 16)));
}
string Sddl(string aces) => "O:SYG:SYD:" + aces;
Test("NULL DACL is unrestricted", () => Assert(isolated.CheckDiscretionary("O:SYG:SYD:NO_ACCESS_CONTROL") == Rights.FullControl));
Test("Empty DACL denies non-owner", () => Assert(isolated.CheckDiscretionary(Sddl("")) == 0));
Test("Parser preserves NULL versus empty", () => { Assert(DescriptorParser.Parse("O:SYG:SYD:NO_ACCESS_CONTROL").NullDacl); Assert(!DescriptorParser.Parse(Sddl("")).NullDacl); });
Test("Specific allow", () => Assert(isolated.CheckDiscretionary(Sddl($"(A;;0x1;;;{fixtureSid})")) == 1));
Test("Generic read mapping", () => Assert(isolated.CheckDiscretionary(Sddl($"(A;;GR;;;{fixtureSid})")) == Rights.Read));
Test("Generic all mapping", () => Assert(isolated.CheckDiscretionary(Sddl($"(A;;GA;;;{fixtureSid})")) == Rights.FullControl));
Test("Deny delete preserves read and write", () => Assert(isolated.CheckDiscretionary(Sddl($"(D;;SD;;;{fixtureSid})(A;;FA;;;{fixtureSid})")) == (Rights.FullControl & ~0x10000u)));
Test("Noncanonical allow before deny follows Windows order", () => Assert(isolated.CheckDiscretionary(Sddl($"(A;;0x1;;;{fixtureSid})(D;;0x1;;;{fixtureSid})")) == 1));
Test("Noncanonical descriptor detected", () => Assert(!DescriptorParser.Parse(Sddl($"(A;;0x1;;;{fixtureSid})(D;;0x1;;;{fixtureSid})")).Canonical));
Test("Inherit-only does not grant current object", () => Assert(isolated.CheckDiscretionary(Sddl($"(A;CIIO;FA;;;{fixtureSid})")) == 0));
Test("Inherited allow applies", () => Assert(isolated.CheckDiscretionary(Sddl($"(A;ID;0x1;;;{fixtureSid})")) == 1));
Test("Inherited deny applies", () => Assert(isolated.CheckDiscretionary(Sddl($"(D;ID;0x1;;;{fixtureSid})(A;ID;FA;;;{fixtureSid})")) == (Rights.FullControl & ~1u)));
Test("Creator Owner is not current user on existing object", () => Assert(isolated.CheckDiscretionary(Sddl("(A;;FA;;;CO)")) == 0));
Test("Owner gets implicit READ_CONTROL and WRITE_DAC", () => Assert((isolated.CheckDiscretionary($"O:{fixtureSid}G:SYD:") & 0x60000) == 0x60000));
Test("Owner Rights SID overrides implicit owner permissions", () => Assert((isolated.CheckDiscretionary($"O:{fixtureSid}G:SYD:(D;;WD;;;OW)") & 0x40000) == 0));
Test("Different SID cannot grant access", () => Assert(isolated.CheckDiscretionary(Sddl("(A;;FA;;;S-1-5-21-111-222-333-999)")) == 0));
Test("Owner Rights allow has auditable ACE evidence", () =>
{
    var descriptor = DescriptorParser.Parse($"O:{fixtureSid}G:SYD:(A;;0x1;;;OW)");
    var mask = isolated.CheckDiscretionary(descriptor.Sddl);
    var evidence = AccessPathBuilder.Explain(descriptor, isolated.Identity, mask, "fixture", new());
    Assert((mask & 1) == 1);
    Assert(evidence.Any(e => e.Sid == "S-1-3-4" && e.Relation == "GrantedBy" && (e.ContributingMask & 1) == 1));
});
Test("Owner Rights deny has auditable ACE evidence", () =>
{
    var descriptor = DescriptorParser.Parse($"O:{fixtureSid}G:SYD:(D;;WD;;;OW)");
    var mask = isolated.CheckDiscretionary(descriptor.Sddl);
    var evidence = AccessPathBuilder.Explain(descriptor, isolated.Identity, mask, "fixture", new());
    Assert(evidence.Any(e => e.Sid == "S-1-3-4" && e.Relation == "DeniedBy" && (e.ContributingMask & 0x40000) != 0));
});
Test("Owner Rights ACE does not apply to a non-owner", () =>
{
    var descriptor = DescriptorParser.Parse("O:SYG:SYD:(A;;FA;;;OW)");
    Assert(AccessPathBuilder.Explain(descriptor, isolated.Identity, 0, "fixture", new()).Count == 0);
});
Test("Protected inheritance retained", () => Assert(DescriptorParser.Parse("O:SYG:SYD:P(A;;FA;;;SY)").Protected));
Test("Conditional ACE becomes Unknown", () =>
{
    var descriptor = DescriptorParser.Parse(Sddl($"(XA;;FR;;;{fixtureSid};(@User.department == \"Finance\"))"));
    Assert(descriptor.HasSpecialAces);
    Assert(isolated.Evaluate(descriptor, "C:\\fixture").State == AccessState.Unknown);
});
Test("Unknown context never exposes granted capabilities", () =>
{
    var decision = new AccessDecision(fixtureSid, "Fixture", AccessState.Unknown, Rights.FullControl, null, "fixture", "unavailable", []);
    Assert(decision.Capabilities.All(c => c.State == AccessState.Unknown));
});
Test("Share and NTFS intersection", () =>
{
    var decision = new AccessDecision(fixtureSid, "Fixture", AccessState.Partial, Rights.FullControl, Rights.Read, "fixture", null, []);
    Assert(decision.EffectiveMask == Rights.Read); Assert(decision.Capabilities.Single(c => c.Key == "Write").State != AccessState.Granted);
});
Test("Remote context is conservatively Unknown", () =>
{
    var sd = DescriptorParser.Parse(Sddl($"(A;;FA;;;{fixtureSid})"));
    Assert(isolated.Evaluate(sd, "\\\\server\\share", new("server", "share", "C:\\share", sd, null)).State == AccessState.Unknown);
});
Test("Reparse decision cannot prove target access", () => Assert(isolated.Evaluate(DescriptorParser.Parse(Sddl($"(A;;FA;;;{fixtureSid})")), "C:\\link", null, true).State == AccessState.Unknown));
Test("Nested group graph with cycle and duplicate", () =>
{
    var graph = new GroupGraph(); graph.Add(new("u", "g1", "fixture")); graph.Add(new("g1", "g2", "fixture")); graph.Add(new("g2", "g1", "fixture")); graph.Add(new("u", "g1", "fixture"));
    Assert(graph.Edges.Count == 3); Assert(graph.FindPath("u", "g2").Count == 2); Assert(graph.FindPath("u", "missing").Count == 0);
});
Test("Multiple ACE contributors", () =>
{
    var descriptor = DescriptorParser.Parse(Sddl($"(A;;0x1;;;{fixtureSid})(A;;0x2;;;{fixtureSid})"));
    var evidence = AccessPathBuilder.Explain(descriptor, isolated.Identity, 3, "fixture", new());
    Assert(evidence.Count == 2 && evidence.Aggregate(0u, (m, e) => m | e.ContributingMask) == 3);
});
Test("Deny-only groups never contribute allows", () =>
{
    var identity = new AccessIdentity(new("u", "u", "User", true), [new(fixtureSid, 0x10)], true, "fixture");
    var descriptor = DescriptorParser.Parse(Sddl($"(D;;0x1;;;{fixtureSid})(A;;0x2;;;{fixtureSid})"));
    var evidence = AccessPathBuilder.Explain(descriptor, identity, 0, "fixture", new());
    Assert(evidence.Count == 1 && evidence[0].Relation == "DeniedBy");
});
Test("Disabled token SID contributes nothing", () =>
{
    var identity = new AccessIdentity(new("u", "u", "User", true), [new(fixtureSid, 0)], true, "fixture");
    Assert(AccessPathBuilder.Explain(DescriptorParser.Parse(Sddl($"(A;;FA;;;{fixtureSid})")), identity, 0, "fixture", new()).Count == 0);
});
Test("Broad ACE exposure is deterministic", () => Assert(FindingRules.Evaluate("fixture", DescriptorParser.Parse(Sddl("(A;;FA;;;WD)"))).Any(f => f.Rule == "BroadAllow" && f.Severity == Severity.CriticalExposure)));
Test("Unresolved SID is not claimed deleted", () => Assert(FindingRules.Evaluate("fixture", DescriptorParser.Parse(Sddl($"(A;;FR;;;{fixtureSid})"))).Any(f => f.Rule == "UnresolvedSid" && f.Evidence.Contains("does not prove"))));
Test("Simulation removes only chosen ACE", () =>
{
    var before = Sddl($"(A;;0x1;;;{fixtureSid})(A;;0x2;;;{fixtureSid})"); var after = ImpactSimulator.RemoveAce(before, 0);
    Assert(isolated.CheckDiscretionary(before) == 3); Assert(isolated.CheckDiscretionary(after) == 2);
});
Test("Simulation add deny maintains precedence", () => Assert(isolated.CheckDiscretionary(ImpactSimulator.AddAce(Sddl($"(A;;FA;;;{fixtureSid})"), fixtureSid, 1, true)) == (Rights.FullControl & ~1u)));
Test("Malformed SID rejected", () => Throws<ArgumentException>(() => resolver.Find("S-1-invalid")));
Test("Out of range ACE rejected", () => Throws<ArgumentOutOfRangeException>(() => ImpactSimulator.RemoveAce(Sddl(""), 0)));
Test("Malformed descriptor rejected", () => Throws<ArgumentException>(() => DescriptorParser.Parse("not-sddl")));
Test("500 randomized ACLs agree with ordered-bit reference", () =>
{
    var random = new Random(9183);
    for (var i = 0; i < 500; i++)
    {
        uint granted = 0, remaining = 0x1ff;
        var aces = new List<string>();
        for (var j = 0; j < 8; j++)
        {
            var deny = random.Next(2) == 0; var mask = (uint)random.Next(512);
            aces.Add($"({(deny ? "D" : "A")};;0x{mask:X};;;{fixtureSid})");
            if (!deny) granted |= remaining & mask;
            remaining &= ~mask;
        }
        Assert(isolated.CheckDiscretionary(Sddl(string.Join("", aces))) == granted, "Differential fixture " + i);
    }
});

PermissionSnapshot? captured = null;
await TestAsync("Real NTFS tree / Unicode / no mutation", async () =>
{
    var path = Path.Combine(testRoot, "tree"); Directory.CreateDirectory(path);
    Directory.CreateDirectory(Path.Combine(path, "Finance")); Directory.CreateDirectory(Path.Combine(path, "日本語-العربية-😀"));
    var reader = new AclReader(resolver); var before = reader.Read(path).Hash;
    var clock = Stopwatch.StartNew(); captured = await new Scanner().ScanAsync(new(path));
    timings.Add($"Local scan, 3 directories: {clock.Elapsed.TotalMilliseconds:F2} ms");
    Assert(captured.Resources.Count == 3); Assert(captured.Resources.All(r => r.Error == null)); Assert(reader.Read(path).Hash == before);
});
await TestAsync("Invalid path produces a read error", async () =>
{
    var snapshot = await new Scanner().ScanAsync(new(Path.Combine(testRoot, "missing"), Recursive: false)); Assert(snapshot.Resources.Count == 1 && snapshot.Resources[0].Error != null);
});
await TestAsync("Long paths retain Windows descriptor access", async () =>
{
    var path = Path.Combine(testRoot, new string('a', 110), new string('b', 110), new string('c', 90));
    Directory.CreateDirectory(path);
    var observation = await new Scanner().ScanAsync(new(path, Recursive: false));
    Assert(observation.Resources.Single().Descriptor != null && observation.Resources.Single().Error == null);
});
await TestAsync("Junction cycle is not traversed", async () =>
{
    var path = Path.Combine(testRoot, "junction-root"); Directory.CreateDirectory(path);
    var link = Path.Combine(path, "cycle");
    var info = new ProcessStartInfo("cmd.exe") { UseShellExecute = false, CreateNoWindow = true, RedirectStandardOutput = true, RedirectStandardError = true };
    info.Arguments = $"/c mklink /J \"{link}\" \"{path}\"";
    using var process = Process.Start(info)!; var stdout = process.StandardOutput.ReadToEndAsync(); var stderr = process.StandardError.ReadToEndAsync(); await process.WaitForExitAsync();
    Assert(process.ExitCode == 0, await stdout + await stderr);
    try
    {
        var observation = await new Scanner().ScanAsync(new(path));
        Assert(observation.Resources.Count == 2); Assert(observation.Resources.Single(r => r.Path == link).IsReparsePoint);
        Assert(observation.Resources.Single(r => r.Path == link).Decision?.State == AccessState.Unknown);
    }
    finally { Directory.Delete(link, false); }
});
await TestAsync("Cancelled scan returns incomplete scope", async () =>
{
    using var cts = new CancellationTokenSource(); cts.Cancel(); var snapshot = await new Scanner().ScanAsync(new(testRoot), null, cts.Token); Assert(snapshot.Cancelled && snapshot.Resources.Count == 0);
});
await TestAsync("Real explicit / inherited ACLs and icacls differential", async () =>
{
    var folder = Directory.CreateDirectory(Path.Combine(testRoot, "acl-fixture"));
    var original = folder.GetAccessControl();
    try
    {
        using var identity = WindowsIdentity.GetCurrent();
        var acl = new DirectorySecurity(); acl.SetOwner(identity.User!); acl.SetAccessRuleProtection(true, false);
        acl.AddAccessRule(new FileSystemAccessRule(identity.User!, FileSystemRights.FullControl, InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit, PropagationFlags.None, AccessControlType.Allow));
        acl.AddAccessRule(new FileSystemAccessRule(new SecurityIdentifier(fixtureSid), FileSystemRights.Read, InheritanceFlags.ContainerInherit, PropagationFlags.None, AccessControlType.Allow));
        folder.SetAccessControl(acl);
        var child = Directory.CreateDirectory(Path.Combine(folder.FullName, "child"));
        var grandchild = Directory.CreateDirectory(Path.Combine(child.FullName, "grandchild"));
        var reader = new AclReader(resolver);
        Assert(reader.Read(folder.FullName).Protected);
        Assert(reader.Read(child.FullName).Aces.Any(a => a.Sid == fixtureSid && a.Inherited));
        Assert(reader.Read(grandchild.FullName).Aces.Any(a => a.Sid == fixtureSid && a.Inherited));
        using var access = new EffectiveAccessResolver(resolver);
        Assert(access.CheckDiscretionary(reader.Read(folder.FullName).Sddl) == Rights.FullControl);
        var psi = new ProcessStartInfo("icacls.exe") { RedirectStandardOutput = true, UseShellExecute = false, CreateNoWindow = true }; psi.ArgumentList.Add(child.FullName);
        using var process = Process.Start(psi)!; var output = await process.StandardOutput.ReadToEndAsync(); await process.WaitForExitAsync();
        Assert(process.ExitCode == 0 && output.Contains(fixtureSid));
    }
    finally { folder.SetAccessControl(original); }
});
if (captured is { } snapshot)
{
    Test("SQLite roundtrip and duplicate protection", () =>
    {
        var store = new SnapshotStore(Path.Combine(testRoot, "snapshots.db")); var clock = Stopwatch.StartNew(); store.Save(snapshot); var loaded = store.Load(snapshot.Id);
        timings.Add($"Snapshot write/read, {snapshot.Resources.Count} objects: {clock.Elapsed.TotalMilliseconds:F2} ms");
        Assert(loaded.Resources.Count == snapshot.Resources.Count && loaded.Root == snapshot.Root); Assert(store.List().Count == 1);
        Throws<Microsoft.Data.Sqlite.SqliteException>(() => store.Save(snapshot));
    });
    Test("Snapshot corruption fails closed", () =>
    {
        var json = JsonSerializer.Serialize(snapshot, SnapshotJson.Options); var hash = snapshot.Resources[0].Descriptor!.Hash;
        Throws<InvalidDataException>(() => SnapshotJson.Parse(json.Replace(hash, new string('0', 64))));
    });
    Test("Snapshot newer schema rejected", () => Throws<InvalidDataException>(() => SnapshotJson.Parse(JsonSerializer.Serialize(snapshot with { SchemaVersion = 99 }, SnapshotJson.Options))));
    Test("Snapshot malformed JSON rejected", () => Throws<JsonException>(() => SnapshotJson.Parse("{ invalid")));
    Test("Compare unchanged snapshots", () => Assert(SnapshotComparer.Compare(snapshot, snapshot).Count == 0));
    Test("Compare membership impact without a descriptor change", () =>
    {
        var r = snapshot.Resources[0]; var updated = r with { Decision = r.Decision! with { GrantedMask = 1, State = AccessState.Partial } };
        Assert(SnapshotComparer.Compare(snapshot with { Resources = [r] }, snapshot with { Resources = [updated] }).Single().Kind == "AccessChanged");
    });
    Test("Compare absence is not deletion", () => Assert(SnapshotComparer.Compare(snapshot, snapshot with { Resources = [] }).All(c => c.Kind == "NotObserved")));
    Test("Compare descriptor change", () =>
    {
        var r = snapshot.Resources[0]; var changed = r with { Descriptor = DescriptorParser.Parse(Sddl("")) };
        Assert(SnapshotComparer.Compare(snapshot with { Resources = [r] }, snapshot with { Resources = [changed] }).Single().Kind == "Changed");
    });
    Test("HTML escapes paths and includes limitations", () =>
    {
        var modified = snapshot with { Root = "<script>alert(1)</script>" }; var html = ReportExporter.Html(modified);
        Assert(!html.Contains("<script>")); Assert(html.Contains("&lt;script&gt;")); Assert(html.Contains("remote logon"));
    });
    Test("CSV formula injection protection", () =>
    {
        var modified = snapshot with { Resources = [snapshot.Resources[0] with { Path = "=HYPERLINK(\"bad\")" }] };
        Assert(ReportExporter.Csv(modified).Contains("\"'=HYPERLINK"));
    });
    foreach (var format in new[] { "html", "csv", "json", "xlsx" }) Test("Export " + format, () =>
    {
        var path = Path.Combine(testRoot, "report." + format); ReportExporter.Export(snapshot, path, format); Assert(new FileInfo(path).Length > 100);
        if (format == "xlsx")
        {
            using var zip = ZipFile.OpenRead(path); Assert(zip.Entries.Count(e => e.FullName.StartsWith("xl/worksheets/")) == 6);
            foreach (var entry in zip.Entries) { using var stream = entry.Open(); XDocument.Load(stream); }
        }
    });
    Test("PDF export is a text document", () =>
    {
        var latin = snapshot with { Resources = [snapshot.Resources[0]] }; var path = Path.Combine(testRoot, "report.pdf"); ReportExporter.Export(latin, path, "pdf");
        Assert(File.ReadAllBytes(path).Take(4).SequenceEqual("%PDF"u8.ToArray()));
    });
    Test("PDF refuses unsupported complex-script shaping", () => Throws<InvalidOperationException>(() => ReportExporter.Export(snapshot, Path.Combine(testRoot, "complex.pdf"), "pdf")));
    Test("Export denied destination preserves existing file", () =>
    {
        var path = Path.Combine(testRoot, "locked.json"); File.WriteAllText(path, "original"); using var locked = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.None);
        try { ReportExporter.Export(snapshot, path, "json"); throw new InvalidOperationException("Export should have failed."); }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException) { }
    });
    Test("100,000 object comparison benchmark", () =>
    {
        var resources = Enumerable.Range(0, 100000).Select(i => snapshot.Resources[0] with { Path = "C:\\fixture\\" + i }).ToArray(); var large = snapshot with { Resources = resources };
        var clock = Stopwatch.StartNew(); Assert(SnapshotComparer.Compare(large, large).Count == 0); timings.Add($"Compare 100,000 objects: {clock.Elapsed.TotalMilliseconds:F2} ms");
    });
}
Test("Deep 10,000-group traversal benchmark", () =>
{
    var graph = new GroupGraph(); for (var i = 0; i < 10000; i++) graph.Add(new(i.ToString(), (i + 1).ToString(), "fixture"));
    var clock = Stopwatch.StartNew(); Assert(graph.FindPath("0", "10000").Count == 10000); timings.Add($"Group path, 10,000 edges: {clock.Elapsed.TotalMilliseconds:F2} ms");
});
Test("Verified file remediation and rollback", () =>
{
    var path = Path.Combine(testRoot, "remediation.txt"); File.WriteAllText(path, "permission fixture");
    var reader = new AclReader(resolver); var before = reader.Read(path);
    var proposed = ImpactSimulator.AddAce(before.Sddl, fixtureSid, 1, false);
    var plan = RemediationService.Prepare(path, before.Hash, proposed);
    var receipt = RemediationService.Apply(plan, path, Path.Combine(testRoot, "changes"));
    Assert(reader.Read(path).Aces.Any(a => a.Sid == fixtureSid));
    var rollback = RemediationService.Rollback(receipt, path, Path.Combine(testRoot, "changes"));
    Assert(rollback.RolledBack); Assert(!reader.Read(path).Aces.Any(a => a.Sid == fixtureSid));
});
Test("Stale remediation refuses overwrite", () =>
{
    var path = Path.Combine(testRoot, "stale.txt"); File.WriteAllText(path, "fixture");
    var reader = new AclReader(resolver); var before = reader.Read(path);
    var plan = RemediationService.Prepare(path, before.Hash, ImpactSimulator.AddAce(before.Sddl, fixtureSid, 1, false));
    var file = new FileInfo(path); var acl = file.GetAccessControl(); acl.AddAccessRule(new FileSystemAccessRule(new SecurityIdentifier(fixtureSid), FileSystemRights.Write, AccessControlType.Allow)); file.SetAccessControl(acl);
    Throws<InvalidOperationException>(() => RemediationService.Apply(plan, path, Path.Combine(testRoot, "changes")));
});
Test("Remediation requires exact confirmation", () =>
{
    var path = Path.Combine(testRoot, "confirm.txt"); File.WriteAllText(path, "fixture"); var before = new AclReader(resolver).Read(path);
    var plan = RemediationService.Prepare(path, before.Hash, ImpactSimulator.AddAce(before.Sddl, fixtureSid, 1, false));
    Throws<InvalidOperationException>(() => RemediationService.Apply(plan, path + "x", Path.Combine(testRoot, "changes")));
});
Test("100,000 descriptor parses benchmark", () =>
{
    var sddl = Sddl($"(A;;FR;;;{fixtureSid})"); var clock = Stopwatch.StartNew(); for (var i = 0; i < 100000; i++) DescriptorParser.Parse(sddl);
    timings.Add($"Parse 100,000 descriptors: {clock.Elapsed.TotalMilliseconds:F2} ms");
});
Console.WriteLine($"\n{passed} passed; {failures.Count} failed.");
foreach (var timing in timings) Console.WriteLine(timing);
if (args.Contains("--results"))
{
    var index = Array.IndexOf(args, "--results");
    AtomicFile.WriteText(args[index + 1], JsonSerializer.Serialize(new { Passed = passed, Failed = failures.Count, Failures = failures, Benchmarks = timings, Runtime = Environment.Version.ToString(), Timestamp = DateTimeOffset.UtcNow }, SnapshotJson.Options));
}
Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
try { Directory.Delete(testRoot, true); } catch (IOException e) { Console.WriteLine("Test cleanup: " + e.Message); }
return failures.Count == 0 ? 0 : 1;
