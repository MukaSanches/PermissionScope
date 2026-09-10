using System.Globalization;

namespace PermissionScope.Core;

public sealed record ResolvedLocale(string LanguageTag, string ResourceTag, bool IsRightToLeft);

public static class LocaleResolver
{
    public static ResolvedLocale Resolve(string requested, IEnumerable<string> available)
    {
        var resources = available.ToHashSet(StringComparer.OrdinalIgnoreCase);
        CultureInfo culture;
        try { culture = CultureInfo.GetCultureInfo(requested); }
        catch (CultureNotFoundException) { culture = CultureInfo.GetCultureInfo("en-US"); }
        if (culture.Equals(CultureInfo.InvariantCulture)) culture = CultureInfo.GetCultureInfo("en-US");
        for (var candidate = culture; !candidate.Equals(CultureInfo.InvariantCulture); candidate = candidate.Parent)
        {
            if (resources.Contains(candidate.Name)) return new(culture.Name, candidate.Name, culture.TextInfo.IsRightToLeft);
        }
        // Do not substitute a different Chinese script when its resources are absent.
        var sibling = culture.TwoLetterISOLanguageName == "zh" ? null : resources.Order(StringComparer.OrdinalIgnoreCase)
            .FirstOrDefault(tag => tag.StartsWith(culture.TwoLetterISOLanguageName + "-", StringComparison.OrdinalIgnoreCase));
        return new(culture.Name, sibling ?? "en-US", culture.TextInfo.IsRightToLeft);
    }
}
