using PdfSharp.Drawing;
using PdfSharp.Fonts;
using PdfSharp.Pdf;

namespace PermissionScope.Core;

internal static class PdfReport
{
    private static readonly object FontLock = new();
    public static void Write(PermissionSnapshot snapshot, Stream output)
    {
        // Complex-script shaping requires a text layout engine. Do not silently emit missing glyphs.
        if (snapshot.Resources.Any(r => r.Path.Any(c => c > 0x052F) || r.Decision?.Identity.Any(c => c > 0x052F) == true))
            throw new InvalidOperationException("This PDF font path supports Latin, Greek and Cyrillic text. Export HTML and print to PDF for complex scripts, CJK or emoji.");
        lock (FontLock) { if (GlobalFontSettings.FontResolver == null) GlobalFontSettings.FontResolver = new WindowsReportFonts(); }
        using var document = new PdfDocument();
        document.Info.Title = "PermissionScope — Access report";
        document.Info.Creator = "PermissionScope 1.0.0";
        var font = new XFont("ScopeReport", 10);
        var bold = new XFont("ScopeReport", 10, XFontStyleEx.Bold);
        var heading = new XFont("ScopeReport", 23, XFontStyleEx.Bold);
        var ink = new XSolidBrush(XColor.FromArgb(23, 25, 29));
        var navy = new XSolidBrush(XColor.FromArgb(23, 49, 92));
        PdfPage? page = null;
        XGraphics? graphics = null;
        double y = 0;
        void NewPage()
        {
            graphics?.Dispose(); page = document.AddPage(); page.Size = PdfSharp.PageSize.A4;
            graphics = XGraphics.FromPdfPage(page); y = 58;
            graphics.DrawString("PERMISSIONSCOPE / ACCESS REPORT", bold, navy, 42, 30);
            graphics.DrawLine(new XPen(XColor.FromArgb(217, 222, 231)), 42, 39, page.Width.Point - 42, 39);
            graphics.DrawString($"Generated with PermissionScope  ·  {document.PageCount}", font, ink, 42, page.Height.Point - 28);
        }
        void Paragraph(string value, XFont? style = null)
        {
            style ??= font;
            if (graphics == null) NewPage();
            var width = page!.Width.Point - 84;
            foreach (var paragraph in value.Replace("\r", "").Split('\n'))
            {
                var line = "";
                foreach (var ch in paragraph)
                {
                    if (graphics!.MeasureString(line + ch, style).Width > width)
                    { if (y > page.Height.Point - 64) NewPage(); graphics!.DrawString(line, style, ink, 42, y); y += style.Size * 1.45; line = ""; }
                    line += ch;
                }
                if (y > page.Height.Point - 64) NewPage(); graphics!.DrawString(line, style, ink, 42, y); y += style.Size * 1.45;
            }
            y += 6;
        }
        try
        {
            NewPage(); Paragraph("Access report", heading); Paragraph(snapshot.Root, bold);
            Paragraph($"Observed: {snapshot.CreatedAt:u}\nSnapshot: {snapshot.Id}\nObjects: {snapshot.Resources.Count:N0} · Read errors: {snapshot.Resources.Count(r => r.Error != null):N0}\nStatus: {(snapshot.Cancelled ? "Cancelled; partial scope" : "Completed")}");
            Paragraph("Scope", bold); Paragraph("Read-only observations for the selected identity. Authz evaluates discretionary permissions. Remote masks are projections, not verified server logons. Integrity policy, privileges, encryption, parent traversal and locks are outside this decision.");
            foreach (var resource in snapshot.Resources)
            {
                Paragraph(resource.Path, bold);
                if (resource.Decision is { } decision)
                {
                    Paragraph($"{decision.Identity}\nDecision: {decision.State} · NTFS: 0x{decision.GrantedMask:X8}" + (decision.ShareMask is { } share ? $" · Share: 0x{share:X8}" : ""));
                    if (decision.Limitation != null) Paragraph(decision.Limitation);
                    foreach (var c in decision.Capabilities) Paragraph($"{c.Key}: {c.State}");
                    foreach (var e in decision.Evidence) Paragraph($"ACE #{e.AceIndex}: {e.Identity}\n{e.Relation} 0x{e.Mask:X8}; contribution 0x{e.ContributingMask:X8}\n{e.Flags} · {e.Sid}");
                }
                foreach (var f in resource.Findings) Paragraph($"{f.Severity}: {f.Evidence}\n{f.Recommendation}");
                if (resource.Error != null) Paragraph($"Error {resource.Error.Code}: {resource.Error.Message}");
                if (resource.Descriptor != null) Paragraph($"Descriptor SHA-256: {resource.Descriptor.Hash}");
            }
            graphics?.Dispose(); graphics = null; document.Save(output, false);
        }
        finally { graphics?.Dispose(); }
    }
    private sealed class WindowsReportFonts : IFontResolver
    {
        public FontResolverInfo ResolveTypeface(string familyName, bool bold, bool italic) => new(bold ? "arialbd" : "arial");
        public byte[] GetFont(string faceName) => File.ReadAllBytes(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), faceName + ".ttf"));
    }
}
