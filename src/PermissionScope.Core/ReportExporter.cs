using System.Globalization;
using System.IO.Compression;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Xml;

namespace PermissionScope.Core;

public static class ReportExporter
{
    public static void Export(PermissionSnapshot snapshot, string path, string format)
    {
        switch (format.ToLowerInvariant())
        {
            case "json": AtomicFile.WriteText(path, JsonSerializer.Serialize(snapshot, SnapshotJson.Options)); break;
            case "csv": AtomicFile.WriteText(path, Csv(snapshot)); break;
            case "html": AtomicFile.WriteText(path, Html(snapshot)); break;
            case "xlsx": AtomicFile.Write(path, stream => WriteWorkbook(snapshot, stream)); break;
            case "pdf": AtomicFile.Write(path, stream => PdfReport.Write(snapshot, stream)); break;
            default: throw new ArgumentException("Supported formats: html, csv, json, xlsx, pdf.");
        }
    }
    public static string Csv(PermissionSnapshot snapshot)
    {
        var output = new StringBuilder("\uFEFFPath,Identity,Decision,NTFS mask,Share mask,Effective mask,Observed UTC,Limitation,Error,Fixture\r\n");
        foreach (var r in snapshot.Resources)
            output.AppendLine(string.Join(',', new[] { r.Path, r.Decision?.Identity, r.Decision?.State.ToString(),
                Hex(r.Decision?.GrantedMask), Hex(r.Decision?.ShareMask), r.Decision?.State == AccessState.Unknown ? "Unknown" : Hex(r.Decision?.EffectiveMask),
                r.ObservedAt.ToString("O"), r.Decision?.Limitation, r.Error?.Message, snapshot.FixtureId }.Select(Cell)));
        return output.ToString();
    }
    private static string Cell(string? value)
    {
        value ??= "";
        if (value.Length > 0 && "=+-@\t\r\n".Contains(value[0])) value = "'" + value;
        return '"' + value.Replace("\"", "\"\"") + '"';
    }
    private static string Hex(uint? mask) => mask is { } value ? $"0x{value:X8}" : "";
    public static string Html(PermissionSnapshot snapshot)
    {
        static string H(string? value) => WebUtility.HtmlEncode(value ?? "");
        var html = new StringBuilder("<!doctype html><html lang=\"en\"><meta charset=\"utf-8\"><meta name=\"viewport\" content=\"width=device-width,initial-scale=1\"><title>PermissionScope · Access report</title><style>body{font:15px 'Segoe UI',sans-serif;color:#17191d;margin:48px auto;max-width:1120px;padding:0 24px}header{border-bottom:3px solid #17315c;padding-bottom:24px}h1{font-size:36px;font-weight:600;letter-spacing:-1px}h2{margin-top:40px;font-size:22px}h3{font-size:16px}p,li{line-height:1.6}small{color:#646b76}table{border-collapse:collapse;width:100%;font-size:13px;margin:16px 0}th,td{text-align:left;vertical-align:top;border-bottom:1px solid #d9dee7;padding:10px;overflow-wrap:anywhere}th{background:#f7f8fa}code{font:12px Consolas,monospace;overflow-wrap:anywhere}details{border-top:1px solid #d9dee7;padding:16px 0}summary{cursor:pointer;overflow-wrap:anywhere}.note{padding:16px;background:#f7f8fa;border-left:3px solid #2764e7}footer{margin-top:48px;border-top:1px solid #d9dee7;padding-top:16px;color:#646b76}@media print{body{margin:0;max-width:none;font-size:10pt}thead{display:table-header-group}tr{break-inside:avoid}details{display:block}h2,h3{break-after:avoid}a{color:inherit}}</style><header><small>PERMISSIONSCOPE / WINDOWS ACCESS INTELLIGENCE</small><h1>Access report</h1>");
        html.Append($"<p>{H(snapshot.Root)}</p><small>{H(snapshot.CreatedAt.ToString("u"))} · Snapshot {H(snapshot.Id)}</small></header><h2>Scope</h2><p class=\"note\">Read-only observations. Decisions describe discretionary permissions for one selected Windows identity. ACL principals are not a complete list of people. Share projections are not proof of a remote logon. Integrity policy, encryption, file locks and administrative overrides are outside this decision.</p>");
        html.Append($"<h2>Summary</h2><p>{snapshot.Resources.Count.ToString("N0", CultureInfo.InvariantCulture)} objects observed · {snapshot.Resources.Count(r => r.Error != null)} read errors · {snapshot.Resources.Sum(r => r.Findings.Count)} findings · {(snapshot.Cancelled ? "Cancelled; incomplete scope" : "Scan completed")}</p><h2>Findings</h2><table><thead><tr><th>Category</th><th>Path</th><th>Evidence</th><th>Review</th></tr></thead><tbody>");
        foreach (var finding in snapshot.Resources.SelectMany(r => r.Findings)) html.Append($"<tr><td>{H(finding.Severity.ToString())}</td><td>{H(finding.Path)}</td><td>{H(finding.Evidence)}</td><td>{H(finding.Recommendation)}</td></tr>");
        html.Append("</tbody></table><h2>Access for the selected identity</h2><table><thead><tr><th>Path / identity</th><th>Decision</th><th>NTFS / share projection</th><th>Limitations</th></tr></thead><tbody>");
        foreach (var r in snapshot.Resources) html.Append($"<tr><td>{H(r.Path)}<br><small>{H(r.Decision?.Identity)}</small></td><td>{H(r.Decision?.State.ToString() ?? "Unknown")}</td><td><code>{Hex(r.Decision?.GrantedMask)} / {Hex(r.Decision?.ShareMask)}</code></td><td>{H(r.Decision?.Limitation ?? r.Error?.Message)}</td></tr>");
        html.Append("</tbody></table><h2>Why · Technical evidence</h2>");
        if (snapshot.FixtureId != null) html.Append("<p class=\"note\">Synthetic demonstration: fictional identities, resources and permissions evaluated by Windows Authz. No machine inventory was collected.</p>");
        foreach (var r in snapshot.Resources)
        {
            html.Append($"<details><summary>{H(r.Path)}</summary><p>{H(r.Decision?.Basis)}</p><table><tr><th>Principal</th><th>Relation</th><th>Mask / contribution</th><th>Source</th></tr>");
            foreach (var e in r.Decision?.Evidence ?? []) html.Append($"<tr><td>{H(e.Identity)}<br><code>{H(e.Sid)}</code></td><td>{H(e.Relation)} · ACE {e.AceIndex}</td><td><code>{Hex(e.Mask)} / {Hex(e.ContributingMask)}</code></td><td>{H(e.Source)}<br>{H(e.Flags)}<br>{H(string.Join(" → ", e.MembershipPath.Select(p => p.MemberSid + " → " + p.GroupSid + " [" + p.Source + "]")))}</td></tr>");
            html.Append($"</table><h3>Security descriptor</h3><code>{H(r.Descriptor?.Sddl)}</code><p><small>SHA-256: {H(r.Descriptor?.Hash)}</small></p></details>");
        }
        html.Append("<h2>Errors / unknowns</h2><ul>");
        foreach (var r in snapshot.Resources.Where(r => r.Error != null || r.Decision?.State == AccessState.Unknown)) html.Append($"<li>{H(r.Path)}: {H(r.Error?.Message ?? r.Decision?.Limitation)}</li>");
        html.Append("</ul><footer>Generated with PermissionScope · Free forever · No file contents collected.</footer></html>");
        return html.ToString();
    }

    private static void WriteWorkbook(PermissionSnapshot snapshot, Stream output)
    {
        var sheets = new (string Name, string[] Headers, IEnumerable<string[]> Rows)[]
        {
            ("Summary", ["Field", "Value"], new[] { new[] { "Root", snapshot.Root }, ["Snapshot", snapshot.Id], ["Fixture", snapshot.FixtureId ?? "Not a synthetic fixture"], ["Observed UTC", snapshot.CreatedAt.ToString("O")], ["Objects", snapshot.Resources.Count.ToString(CultureInfo.InvariantCulture)], ["Cancelled", snapshot.Cancelled.ToString()], ["Scope", "Discretionary permissions; remote masks are projections."] }),
            ("Access", ["Path", "Identity", "State", "NTFS mask", "Share mask", "Limitation"], snapshot.Resources.Select(r => new[] { r.Path, r.Decision?.Identity ?? "", r.Decision?.State.ToString() ?? "Unknown", Hex(r.Decision?.GrantedMask), Hex(r.Decision?.ShareMask), r.Decision?.Limitation ?? "" })),
            ("Findings", ["Path", "Rule", "Category", "Evidence", "Recommendation"], snapshot.Resources.SelectMany(r => r.Findings).Select(f => new[] { f.Path, f.Rule, f.Severity.ToString(), f.Evidence, f.Recommendation })),
            ("Identities", ["SID", "Name"], snapshot.Resources.SelectMany(r => r.Descriptor?.Aces ?? []).DistinctBy(a => a.Sid).Select(a => new[] { a.Sid, a.Name })),
            ("Errors", ["Path", "Code", "Message"], snapshot.Resources.Where(r => r.Error != null).Select(r => new[] { r.Path, r.Error!.Code.ToString(CultureInfo.InvariantCulture), r.Error.Message })),
            ("Technical Details", ["Path", "Owner", "Protected", "NULL DACL", "SDDL", "SHA-256"], snapshot.Resources.Select(r => new[] { r.Path, r.Descriptor?.Owner ?? "", r.Descriptor?.Protected.ToString() ?? "", r.Descriptor?.NullDacl.ToString() ?? "", r.Descriptor?.Sddl ?? "", r.Descriptor?.Hash ?? "" }))
        };
        using var zip = new ZipArchive(output, ZipArchiveMode.Create, true);
        void Entry(string name, string value) { using var writer = new StreamWriter(zip.CreateEntry(name).Open(), new UTF8Encoding(false)); writer.Write(value); }
        Entry("[Content_Types].xml", "<Types xmlns=\"http://schemas.openxmlformats.org/package/2006/content-types\"><Default Extension=\"rels\" ContentType=\"application/vnd.openxmlformats-package.relationships+xml\"/><Default Extension=\"xml\" ContentType=\"application/xml\"/><Override PartName=\"/xl/workbook.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml\"/><Override PartName=\"/xl/styles.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.styles+xml\"/>" + string.Join("", Enumerable.Range(1, sheets.Length).Select(i => $"<Override PartName=\"/xl/worksheets/sheet{i}.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml\"/>")) + "</Types>");
        Entry("_rels/.rels", "<Relationships xmlns=\"http://schemas.openxmlformats.org/package/2006/relationships\"><Relationship Id=\"rId1\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument\" Target=\"xl/workbook.xml\"/></Relationships>");
        Entry("xl/workbook.xml", "<workbook xmlns=\"http://schemas.openxmlformats.org/spreadsheetml/2006/main\" xmlns:r=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships\"><sheets>" + string.Join("", sheets.Select((s, i) => $"<sheet name=\"{s.Name}\" sheetId=\"{i + 1}\" r:id=\"rId{i + 1}\"/>")) + "</sheets></workbook>");
        Entry("xl/_rels/workbook.xml.rels", "<Relationships xmlns=\"http://schemas.openxmlformats.org/package/2006/relationships\">" + string.Join("", Enumerable.Range(1, sheets.Length).Select(i => $"<Relationship Id=\"rId{i}\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet\" Target=\"worksheets/sheet{i}.xml\"/>")) + "<Relationship Id=\"rStyles\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/styles\" Target=\"styles.xml\"/></Relationships>");
        Entry("xl/styles.xml", "<styleSheet xmlns=\"http://schemas.openxmlformats.org/spreadsheetml/2006/main\"><fonts count=\"2\"><font><sz val=\"11\"/><name val=\"Calibri\"/></font><font><b/><sz val=\"11\"/><color rgb=\"FFFFFFFF\"/><name val=\"Calibri\"/></font></fonts><fills count=\"3\"><fill><patternFill patternType=\"none\"/></fill><fill><patternFill patternType=\"gray125\"/></fill><fill><patternFill patternType=\"solid\"><fgColor rgb=\"FF17315C\"/></patternFill></fill></fills><borders count=\"1\"><border/></borders><cellStyleXfs count=\"1\"><xf/></cellStyleXfs><cellXfs count=\"2\"><xf fontId=\"0\" fillId=\"0\" borderId=\"0\" xfId=\"0\"/><xf fontId=\"1\" fillId=\"2\" borderId=\"0\" xfId=\"0\" applyFill=\"1\" applyFont=\"1\"/></cellXfs></styleSheet>");
        const string ns = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
        for (var i = 0; i < sheets.Length; i++)
        {
            using var stream = zip.CreateEntry($"xl/worksheets/sheet{i + 1}.xml").Open();
            using var xml = XmlWriter.Create(stream, new XmlWriterSettings { Encoding = new UTF8Encoding(false), CheckCharacters = true });
            xml.WriteStartElement("worksheet", ns);
            xml.WriteStartElement("sheetViews", ns); xml.WriteStartElement("sheetView", ns); xml.WriteAttributeString("workbookViewId", "0");
            xml.WriteStartElement("pane", ns); xml.WriteAttributeString("ySplit", "1"); xml.WriteAttributeString("topLeftCell", "A2"); xml.WriteAttributeString("state", "frozen"); xml.WriteEndElement(); xml.WriteEndElement(); xml.WriteEndElement();
            xml.WriteStartElement("cols", ns); xml.WriteStartElement("col", ns); xml.WriteAttributeString("min", "1"); xml.WriteAttributeString("max", sheets[i].Headers.Length.ToString()); xml.WriteAttributeString("width", "38"); xml.WriteAttributeString("customWidth", "1"); xml.WriteEndElement(); xml.WriteEndElement();
            xml.WriteStartElement("sheetData", ns);
            var rowNumber = 0;
            foreach (var row in sheets[i].Rows.Prepend(sheets[i].Headers))
            {
                rowNumber++;
                if (rowNumber > 1048576) throw new InvalidOperationException("Worksheet row limit exceeded; use JSON or CSV.");
                xml.WriteStartElement("row", ns); xml.WriteAttributeString("r", rowNumber.ToString());
                for (var c = 0; c < row.Length; c++)
                {
                    xml.WriteStartElement("c", ns); xml.WriteAttributeString("r", $"{(char)('A' + c)}{rowNumber}"); xml.WriteAttributeString("t", "inlineStr"); if (rowNumber == 1) xml.WriteAttributeString("s", "1");
                    xml.WriteStartElement("is", ns); xml.WriteStartElement("t", ns); xml.WriteString(new string(row[c].Where(ch => XmlConvert.IsXmlChar(ch) || char.IsSurrogate(ch)).ToArray())); xml.WriteEndElement(); xml.WriteEndElement(); xml.WriteEndElement();
                }
                xml.WriteEndElement();
            }
            xml.WriteEndElement(); xml.WriteStartElement("autoFilter", ns); xml.WriteAttributeString("ref", $"A1:{(char)('A' + sheets[i].Headers.Length - 1)}{rowNumber}"); xml.WriteEndElement(); xml.WriteEndElement();
        }
    }
}
