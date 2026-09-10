using Microsoft.Data.Sqlite;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PermissionScope.Core;

public static class SnapshotJson
{
    public static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        MaxDepth = 64,
        Converters = { new JsonStringEnumConverter() }
    };
    public static PermissionSnapshot Parse(string json)
    {
        var snapshot = JsonSerializer.Deserialize<PermissionSnapshot>(json, Options) ?? throw new InvalidDataException("Empty snapshot.");
        if (snapshot.SchemaVersion != PermissionSnapshot.CurrentSchema) throw new InvalidDataException("Unsupported snapshot schema.");
        if (snapshot.Resources is null || snapshot.Groups is null || string.IsNullOrWhiteSpace(snapshot.Id) || string.IsNullOrWhiteSpace(snapshot.Root))
            throw new InvalidDataException("Snapshot metadata is incomplete.");
        var paths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var item in snapshot.Resources)
        {
            if (item is null || string.IsNullOrWhiteSpace(item.Path) || !paths.Add(item.Path)) throw new InvalidDataException("Invalid or duplicate resource path.");
            if (item.Descriptor is { } descriptor && DescriptorParser.Parse(descriptor.Sddl).Hash != descriptor.Hash)
                throw new InvalidDataException("Security descriptor integrity check failed.");
        }
        return snapshot;
    }
}

public sealed record SnapshotSummary(string Id, string Root, DateTimeOffset CreatedAt, int Objects, bool Cancelled);
public sealed class SnapshotStore
{
    public static string DefaultDirectory
    {
        get
        {
            var configured = Environment.GetEnvironmentVariable("PERMISSIONSCOPE_DATA_DIR");
            if (string.IsNullOrWhiteSpace(configured)) return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "PermissionScope");
            if (!Path.IsPathFullyQualified(configured)) throw new ArgumentException("PERMISSIONSCOPE_DATA_DIR must be an absolute path.");
            return Path.GetFullPath(configured);
        }
    }
    private readonly string connectionString;
    static SnapshotStore() => SQLitePCL.Batteries_V2.Init();
    public SnapshotStore(string? path = null)
    {
        path ??= Path.Combine(DefaultDirectory, "snapshots.db");
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path))!);
        connectionString = new SqliteConnectionStringBuilder { DataSource = path, Mode = SqliteOpenMode.ReadWriteCreate, DefaultTimeout = 5 }.ToString();
        using var connection = Open();
        using var version = connection.CreateCommand();
        version.CommandText = "PRAGMA user_version";
        var schema = Convert.ToInt32(version.ExecuteScalar());
        if (schema > 1) throw new InvalidDataException("Snapshot database was created by a newer application.");
        using var command = connection.CreateCommand();
        command.CommandText = "PRAGMA journal_mode=WAL; CREATE TABLE IF NOT EXISTS snapshots (id TEXT PRIMARY KEY, root TEXT NOT NULL, created TEXT NOT NULL, objects INTEGER NOT NULL, cancelled INTEGER NOT NULL, payload TEXT NOT NULL, hash TEXT NOT NULL); CREATE INDEX IF NOT EXISTS ix_snapshots_created ON snapshots(created); PRAGMA user_version=1;";
        command.ExecuteNonQuery();
    }
    private SqliteConnection Open() { var connection = new SqliteConnection(connectionString); connection.Open(); return connection; }
    public void Save(PermissionSnapshot snapshot)
    {
        var json = JsonSerializer.Serialize(snapshot, SnapshotJson.Options);
        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(json)));
        using var connection = Open();
        using var transaction = connection.BeginTransaction();
        using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = "INSERT INTO snapshots(id,root,created,objects,cancelled,payload,hash) VALUES($id,$root,$created,$objects,$cancelled,$payload,$hash)";
        command.Parameters.AddWithValue("$id", snapshot.Id);
        command.Parameters.AddWithValue("$root", snapshot.Root);
        command.Parameters.AddWithValue("$created", snapshot.CreatedAt.ToString("O"));
        command.Parameters.AddWithValue("$objects", snapshot.Resources.Count);
        command.Parameters.AddWithValue("$cancelled", snapshot.Cancelled ? 1 : 0);
        command.Parameters.AddWithValue("$payload", json);
        command.Parameters.AddWithValue("$hash", hash);
        command.ExecuteNonQuery();
        transaction.Commit();
    }
    public IReadOnlyList<SnapshotSummary> List(int limit = 200, int offset = 0)
    {
        using var connection = Open();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT id,root,created,objects,cancelled FROM snapshots ORDER BY created DESC LIMIT $limit OFFSET $offset";
        command.Parameters.AddWithValue("$limit", Math.Clamp(limit, 1, 1000));
        command.Parameters.AddWithValue("$offset", Math.Max(offset, 0));
        using var reader = command.ExecuteReader();
        var result = new List<SnapshotSummary>();
        while (reader.Read()) result.Add(new(reader.GetString(0), reader.GetString(1), DateTimeOffset.Parse(reader.GetString(2)), reader.GetInt32(3), reader.GetInt32(4) != 0));
        return result;
    }
    public PermissionSnapshot Load(string id)
    {
        using var connection = Open();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT payload,hash FROM snapshots WHERE id=$id";
        command.Parameters.AddWithValue("$id", id);
        using var reader = command.ExecuteReader();
        if (!reader.Read()) throw new FileNotFoundException("Snapshot not found.", id);
        var json = reader.GetString(0);
        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(json)));
        if (hash != reader.GetString(1)) throw new InvalidDataException("Snapshot checksum mismatch.");
        return SnapshotJson.Parse(json);
    }
}

public static class AtomicFile
{
    public static void Write(string path, Action<Stream> write)
    {
        path = Path.GetFullPath(path);
        var directory = Path.GetDirectoryName(path)!;
        Directory.CreateDirectory(directory);
        var temporary = Path.Combine(directory, $".permissionscope-{Guid.NewGuid():N}.tmp");
        try
        {
            using (var stream = new FileStream(temporary, FileMode.CreateNew, FileAccess.Write, FileShare.None)) { write(stream); stream.Flush(true); }
            File.Move(temporary, path, true);
        }
        finally { if (File.Exists(temporary)) File.Delete(temporary); }
    }
    public static void WriteText(string path, string text) => Write(path, stream =>
    { using var writer = new StreamWriter(stream, new UTF8Encoding(false), leaveOpen: true); writer.Write(text); });
}
