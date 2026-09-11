<!-- Generated from docs/content/locales.json by build/Build-Documentation.mjs. Do not edit generated localized READMEs by hand. -->
<p align="center"><img src="https://raw.githubusercontent.com/MukaSanches/PermissionScope/main/docs/brand/repository-banner.png" alt="PermissionScope — See who has access. Understand why." width="100%"></p>

<p align="center"><img src="https://raw.githubusercontent.com/MukaSanches/PermissionScope/main/docs/brand/project-avatar.png" alt="PermissionScope mark" width="72" height="72"></p>

<p align="center">
  <a href="https://github.com/MukaSanches/PermissionScope/actions/workflows/build.yml"><img alt="Windows build" src="https://github.com/MukaSanches/PermissionScope/actions/workflows/build.yml/badge.svg"></a>
  <a href="https://github.com/MukaSanches/PermissionScope/actions/workflows/codeql.yml"><img alt="CodeQL" src="https://github.com/MukaSanches/PermissionScope/actions/workflows/codeql.yml/badge.svg"></a>
  <a href="https://github.com/MukaSanches/PermissionScope/blob/main/LICENSE"><img alt="Apache-2.0" src="https://img.shields.io/badge/license-Apache--2.0-2764E7"></a>
  <a href="https://github.com/MukaSanches/PermissionScope/releases"><img alt="Release" src="https://img.shields.io/github/v/release/MukaSanches/PermissionScope?display_name=tag&color=2764E7"></a>
</p>

<p align="center"><strong>Windows-Berechtigungen, nachvollziehbar gemacht.</strong><br>Verstehen Sie, wer auf einen Ordner zugreifen kann, und prüfen Sie die Regeln hinter dem Ergebnis.</p>
<table><tr><td align="center"><a href="https://mukasanches.github.io/PermissionScope/index.de.html"><strong>Website</strong></a></td><td align="center"><a href="https://github.com/MukaSanches/PermissionScope/releases/latest"><strong>Versionsdownloads</strong></a></td><td align="center"><a href="https://github.com/MukaSanches/PermissionScope/blob/main/docs/TRUST-CENTER.md"><strong>Vertrauenszentrum</strong></a></td></tr><tr><td align="center"><a href="https://github.com/MukaSanches/PermissionScope/blob/main/docs/courses/README.md">Akademie</a></td><td align="center"><a href="https://github.com/MukaSanches/PermissionScope/blob/main/ROADMAP.md">Roadmap</a></td><td align="center"><a href="https://github.com/MukaSanches/PermissionScope/blob/main/SECURITY.md">Sicherheit</a></td></tr></table>

<p align="center"><sub>Sprachen</sub></p>
<table><tr><td align="center" width="25%"><a href="https://github.com/MukaSanches/PermissionScope/blob/main/README.md"><strong>English</strong></a><br><sub>en-US</sub></td><td align="center" width="25%"><a href="https://github.com/MukaSanches/PermissionScope/blob/main/README.pt-BR.md"><strong>Português</strong></a><br><sub>pt-BR</sub></td><td align="center" width="25%"><a href="https://github.com/MukaSanches/PermissionScope/blob/main/README.es.md"><strong>Español</strong></a><br><sub>es</sub></td><td align="center" width="25%"><a href="https://github.com/MukaSanches/PermissionScope/blob/main/README.fr.md"><strong>Français</strong></a><br><sub>fr</sub></td></tr><tr><td align="center" width="25%"><a href="https://github.com/MukaSanches/PermissionScope/blob/main/README.de.md"><strong>Deutsch</strong></a><br><sub>de</sub></td><td align="center" width="25%"><a href="https://github.com/MukaSanches/PermissionScope/blob/main/README.ar.md"><strong>العربية</strong></a><br><sub>ar</sub></td><td align="center" width="25%"><a href="https://github.com/MukaSanches/PermissionScope/blob/main/README.ja.md"><strong>日本語</strong></a><br><sub>ja</sub></td><td align="center" width="25%"><a href="https://github.com/MukaSanches/PermissionScope/blob/main/README.zh-Hans.md"><strong>简体中文</strong></a><br><sub>zh-Hans</sub></td></tr></table>

---

<table><tr><td width="33%"><strong>Local-first</strong><br><sub>Kein PermissionScope-Konto, keine App-Telemetrie und kein erforderlicher Cloud-Dienst.</sub></td><td width="33%"><strong>Windows-native Entscheidung</strong><br><sub>Effektiver Zugriff wird mit Windows Authz ausgewertet, nicht mit einer handgeschriebenen Näherung.</sub></td><td width="33%"><strong>Unbekannt bleibt Unbekannt</strong><br><sub>Fehlender Kontext wird nie stillschweigend in Erlaubt oder Verweigert umgewandelt.</sub></td></tr></table>

<p align="center"><img src="https://raw.githubusercontent.com/MukaSanches/PermissionScope/main/docs/screenshots/de/access-light.png" alt="LAB\Alex erhält Ändern über LAB\Finance. Native Aufnahmen eines synthetischen Authz-Szenarios, ohne echte Unternehmensdaten oder nachträglich veränderte Ergebnisse." width="94%"></p>

## In zwei Minuten starten

1. Laden Sie das vollständige Installationspaket oder portable ZIP unter Releases herunter. x64 für Intel/AMD, ARM64 für ARM-PCs.
2. Installieren Sie für Ihr Konto oder entpacken Sie das gesamte ZIP und öffnen Sie PermissionScope.exe.
3. Erkunden Sie die Demo mit LAB\Alex. Für eigene Dateien wählen Sie Analysieren und einen Ordner.
4. Lassen Sie die Identität leer, um Ihr aktuelles Windows-Token zu verwenden. Prüfen Sie die Nachweise unter Access Path.

## Vom Ergebnis zur Evidenz

Windows Authz berechnet die Maske. Access Path zeigt beitragende Einträge und erfasste Mitgliedschaften. Das Vererbungsmerkmal belegt nicht den Ursprungsordner. Vollzugriff in der ACL garantiert nicht, dass eine Datei geöffnet werden kann.

```text
Windows-Identität
      ↓
SID + erfasster Mitgliedschaftskontext
      ↓
ACL / diskretionäre Berechtigungseinträge
      ↓
Windows-Authz-Auswertung
      ↓
Erlaubt · Teilweise · Verweigert · Unbekannt
      ↓
Access Path → beitragende Evidenz
```

## Ergebnis verstehen

Erlaubt gestattet die Aktion nach den ausgewerteten diskretionären Regeln. Teilweise erlaubt bestimmte Aktionen. Verweigert gewährt keinen Zugriff. Unbekannt bedeutet, dass der Kontext keine zuverlässige Aussage erlaubt; es zählt niemals als Erlaubnis.

## Erklärung prüfen

Windows Authz berechnet die Maske. Access Path zeigt beitragende Einträge und erfasste Mitgliedschaften. Das Vererbungsmerkmal belegt nicht den Ursprungsordner. Vollzugriff in der ACL garantiert nicht, dass eine Datei geöffnet werden kann.

<p align="center"><img src="https://raw.githubusercontent.com/MukaSanches/PermissionScope/main/docs/screenshots/de/access-path-light.png" alt="Access Path" width="94%"></p>

## Produktoberfläche

<table><tr><td><strong>Analysieren</strong><br><sub>Effektiven Zugriff für Ordner und Identität prüfen.</sub></td><td><strong>Erklären</strong><br><sub>Access Path und beitragende Evidenz verfolgen.</sub></td><td><strong>Snapshot</strong><br><sub>Lokale Beobachtungen zur späteren Prüfung speichern.</sub></td></tr><tr><td><strong>Vergleichen</strong><br><sub>Beobachtungen vergleichen, ohne fehlende Ressourcen als gelöscht anzunehmen.</sub></td><td><strong>Simulieren</strong><br><sub>Entfernung einer Regel im Speicher modellieren, bevor eine echte Änderung erwogen wird.</sub></td><td><strong>Exportieren</strong><br><sub>HTML-, CSV-, JSON-, XLSX- und PDF-Ausgaben.</sub></td></tr></table>

## Nachweise aufbewahren

Speichern Sie lokale Momentaufnahmen, vergleichen Sie Beobachtungen, simulieren Sie das Entfernen einer Regel im Speicher und exportieren Sie HTML, CSV, JSON, XLSX oder PDF. Später nicht beobachtete Ressourcen gelten nicht automatisch als gelöscht.

```powershell
./permissionscope-cli.exe demo --output ./demo-reports
./permissionscope-cli.exe explain "C:\Finance"
./permissionscope-cli.exe scan "C:\Finance" --save
./permissionscope-cli.exe export <snapshot-id> --format html --output report.html
```

## Umfang und Datenschutz

Remote-Anmeldungen, S4U-Kontexte, bedingte Regeln und ungeprüfte Linkziele bleiben Unbekannt. Integrität, Verschlüsselung, Sperren und Privilegien sind nicht erfasst. Keine Telemetrie, Konten oder Cloud. Echte Exporte können sensible Pfade und Kontonamen enthalten.

> Analyse und Simulation lesen nur. Anwenden ist ein separater, ausdrücklich bestätigter Ablauf für normale lokale Dateien mit Momentaufnahme, dauerhaftem Journal, Prüfung und Rücknahme. Ordner, Links und Dateien mit mehreren Hardlinks sind ausgeschlossen.

## Herunterladen und prüfen

Windows 10 1809 oder neuer. Vollständige Pakete enthalten die Laufzeitumgebungen. Installer sind noch unsigniert; SHA-256 prüfen. ARM64 wird im CI kompiliert, ohne Zertifizierung auf ARM-Hardware. Store- und WinGet-Verfügbarkeit im Versionsstatus prüfen.

[Versionsdownloads](https://github.com/MukaSanches/PermissionScope/releases/latest) · [SHA-256-Prüfsummen](https://github.com/MukaSanches/PermissionScope/releases/latest/download/SHA256SUMS.txt) · [Versionsstatus](https://github.com/MukaSanches/PermissionScope/blob/main/docs/release-status.md)

## Für Überprüfbarkeit gebaut

| Für Überprüfbarkeit gebaut | |
|---|---|
| Quellcode | Öffentliches Repository und Commit-Historie |
| Releases | Versionierte Artefakte und SHA-256-Prüfsummen |
| Lieferkette | CycloneDX-SBOM und dokumentierte, gepinnte Automatisierung |
| Sicherheit | Sicherheitsmodell, Responsible Disclosure und CodeQL |
| Plattform | x64- und ARM64-Buildpfade |
| Dokumentation | Acht lokalisierte Handbücher, visuelle Guides und Academy-Kurse |
| Datenschutz | Local-first-Design und klare Hinweise zu sensiblen Exportdaten |

[Trust Center öffnen](https://github.com/MukaSanches/PermissionScope/blob/main/docs/TRUST-CENTER.md)

## Das Modell lernen, nicht nur die Schaltflächen

- [Berechtigungsgrundlagen](https://github.com/MukaSanches/PermissionScope/blob/main/docs/courses/fundamentals.md)
- [Access Path lesen](https://github.com/MukaSanches/PermissionScope/blob/main/docs/courses/access-path.md)
- [Sichere Diagnose](https://github.com/MukaSanches/PermissionScope/blob/main/docs/courses/troubleshooting.md)
- [PDF-Handbücher · 8 Sprachen](https://github.com/MukaSanches/PermissionScope/tree/main/docs/handbooks)

## Nächsten Schritt wählen

- [Visuelle Anleitung](https://github.com/MukaSanches/PermissionScope/blob/main/docs/guides/de.md)
- [Technisches Modell](https://github.com/MukaSanches/PermissionScope/blob/main/docs/access-model.md)
- [Fragen und Fehlerbehebung](https://github.com/MukaSanches/PermissionScope/blob/main/docs/faq.md)
- [Einfaches Glossar](https://github.com/MukaSanches/PermissionScope/blob/main/docs/glossary.md)
- [Datenschutz](https://github.com/MukaSanches/PermissionScope/blob/main/docs/privacy.md)
- [Versionsstatus](https://github.com/MukaSanches/PermissionScope/blob/main/docs/release-status.md)
- [Governance](https://github.com/MukaSanches/PermissionScope/blob/main/GOVERNANCE.md)
- [Support](https://github.com/MukaSanches/PermissionScope/blob/main/SUPPORT.md)
- [Zitierung](https://github.com/MukaSanches/PermissionScope/blob/main/CITATION.cff)

## Technische Grenzen

PermissionScope behauptet nicht, dass die diskretionäre ACL jedes mögliche Ergebnis beim Öffnen einer Datei erklärt. Integritätsrichtlinien, Verschlüsselung, Sperren, einige Remote-/S4U-Kontexte, bedingte Regeln, Administratorprivilegien und ungeprüfte Reparse-Ziele können außerhalb des verfügbaren Kontexts liegen. Diese Grenzen werden dokumentiert statt versteckt.

## Erstellen und testen

Windows und .NET 10 SDK verwenden. Tests sind ein ausführbares Integrationsprogramm, nicht dotnet test. Das Entwicklerhandbuch erklärt Paketierung und Dokumentationsprüfung.

[Entwicklung](https://github.com/MukaSanches/PermissionScope/blob/main/docs/development.md) · [Benchmarks](https://github.com/MukaSanches/PermissionScope/blob/main/docs/benchmarks.md) · [Roadmap](https://github.com/MukaSanches/PermissionScope/blob/main/ROADMAP.md)

## Hilfe und Mitarbeit

Version, Windows-Version, Vorgang und Fehlercode angeben. Technik kopiert eine Diagnose ohne Pfade, Konten oder SIDs. Übersetzungen sind vorläufig; technische Nachweise können Englisch bleiben. Keine behauptete muttersprachliche Prüfung oder Assistenztechnik-Zertifizierung.

---

<p align="center"><sub>Erstellt von Samuel Sanches · ssanches011@gmail.com · <a href="https://github.com/MukaSanches/PermissionScope/blob/main/LICENSE">Apache-2.0</a> · <a href="https://github.com/MukaSanches/PermissionScope/blob/main/SECURITY.md">Security</a> · <a href="https://github.com/MukaSanches/PermissionScope/blob/main/SUPPORT.md">Support</a></sub></p>
