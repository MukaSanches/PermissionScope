using System.Globalization;
using System.Text.Json;
using PermissionScope.Core;

namespace PermissionScope.App;

internal sealed record Preferences(string Language = "system", string Appearance = "System");
internal static class Localization
{
    private static readonly string SettingsFile = Path.Combine(SnapshotStore.DefaultDirectory, "settings.json");
    private static readonly CultureInfo SystemCulture = CultureInfo.CurrentCulture;
    private static readonly CultureInfo SystemUiCulture = CultureInfo.CurrentUICulture;
    private static Dictionary<string, string> baseline = [];
    private static Dictionary<string, string> selected = [];
    public static Preferences Preferences { get; private set; } = new();
    public static string Locale { get; private set; } = "en-US";
    public static bool IsRtl { get; private set; }
    public static string[] Available => Directory.GetFiles(Path.Combine(AppContext.BaseDirectory, "Locales"), "*.json").Select(Path.GetFileNameWithoutExtension).OfType<string>().Order().ToArray();
    public static void Initialize()
    {
        try { if (File.Exists(SettingsFile)) Preferences = JsonSerializer.Deserialize<Preferences>(File.ReadAllText(SettingsFile)) ?? new(); }
        catch (Exception e) when (e is IOException or JsonException or UnauthorizedAccessException) { Preferences = new(); }
        Load(Preferences.Language);
    }
    public static void Load(string language)
    {
        baseline = Read("en-US");
        var requested = language == "system" ? SystemUiCulture.Name : language;
        var pseudo = requested is "qps-ploc" or "qps-rtl";
        var resolved = LocaleResolver.Resolve(pseudo ? "en-US" : requested, Available);
        Locale = pseudo ? requested : resolved.LanguageTag;
        IsRtl = requested == "qps-rtl" || resolved.IsRightToLeft;
        selected = pseudo ? baseline.ToDictionary(p => p.Key, p => "[ " + p.Value + " ····· ]") : Read(resolved.ResourceTag);
        if (!pseudo && language != "system")
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(Locale);
            CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo(Locale);
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.CurrentCulture;
            CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.CurrentUICulture;
        }
        else if (language == "system")
        {
            CultureInfo.CurrentCulture = SystemCulture;
            CultureInfo.CurrentUICulture = SystemUiCulture;
            CultureInfo.DefaultThreadCurrentCulture = SystemCulture;
            CultureInfo.DefaultThreadCurrentUICulture = SystemUiCulture;
        }
    }
    private static Dictionary<string, string> Read(string locale) => JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Locales", locale + ".json"))) ?? [];
    public static string T(string key) => selected.GetValueOrDefault(key) ?? baseline.GetValueOrDefault(key) ?? key;
    public static void Save(string language, string appearance)
    {
        var preferences = new Preferences(language, appearance);
        AtomicFile.WriteText(SettingsFile, JsonSerializer.Serialize(preferences));
        Preferences = preferences;
        Load(language);
    }
}
