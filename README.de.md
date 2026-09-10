<!-- Generated from docs/content/locales.json by build/Build-Documentation.mjs. -->
# PermissionScope

[English](README.md) · [Português](README.pt-BR.md) · [Español](README.es.md) · [Français](README.fr.md) · [Deutsch](README.de.md) · [العربية](README.ar.md) · [日本語](README.ja.md) · [简体中文](README.zh-Hans.md)

Verstehen Sie, wer auf einen Ordner zugreifen kann, und prüfen Sie die Regeln hinter dem Ergebnis.

[Versionsdownloads](https://github.com/MukaSanches/PermissionScope/releases) · [Visuelle Anleitung](docs/guides/de.md) · [Synthetischen HTML-Bericht ausprobieren](https://mukasanches.github.io/PermissionScope/reports/permissionscope-demo.html)

![LAB\Alex erhält Ändern über LAB\Finance. Native Aufnahmen eines synthetischen Authz-Szenarios, ohne echte Unternehmensdaten oder nachträglich veränderte Ergebnisse.](docs/screenshots/de/access-light.png)

## In zwei Minuten starten

1. Laden Sie das vollständige Installationspaket oder portable ZIP unter Releases herunter. x64 für Intel/AMD, ARM64 für ARM-PCs.
2. Installieren Sie für Ihr Konto oder entpacken Sie das gesamte ZIP und öffnen Sie PermissionScope.exe.
3. Erkunden Sie die Demo mit LAB\Alex. Für eigene Dateien wählen Sie Analysieren und einen Ordner.
4. Lassen Sie die Identität leer, um Ihr aktuelles Windows-Token zu verwenden. Prüfen Sie die Nachweise unter Access Path.

## Ergebnis verstehen

Erlaubt gestattet die Aktion nach den ausgewerteten diskretionären Regeln. Teilweise erlaubt bestimmte Aktionen. Verweigert gewährt keinen Zugriff. Unbekannt bedeutet, dass der Kontext keine zuverlässige Aussage erlaubt; es zählt niemals als Erlaubnis.

## Erklärung prüfen

Windows Authz berechnet die Maske. Access Path zeigt beitragende Einträge und erfasste Mitgliedschaften. Das Vererbungsmerkmal belegt nicht den Ursprungsordner. Vollzugriff in der ACL garantiert nicht, dass eine Datei geöffnet werden kann.

![LAB\Alex erhält Ändern über LAB\Finance. Native Aufnahmen eines synthetischen Authz-Szenarios, ohne echte Unternehmensdaten oder nachträglich veränderte Ergebnisse.](docs/screenshots/de/access-path-light.png)

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

Analyse und Simulation lesen nur. Anwenden ist ein separater, ausdrücklich bestätigter Ablauf für normale lokale Dateien mit Momentaufnahme, dauerhaftem Journal, Prüfung und Rücknahme. Ordner, Links und Dateien mit mehreren Hardlinks sind ausgeschlossen.

## Herunterladen und prüfen

Windows 10 1809 oder neuer. Vollständige Pakete enthalten die Laufzeitumgebungen. Installer sind noch unsigniert; SHA-256 prüfen. ARM64 wird im CI kompiliert, ohne Zertifizierung auf ARM-Hardware. Store- und WinGet-Verfügbarkeit im Versionsstatus prüfen.

## Nächsten Schritt wählen

- [Visuelle Anleitung](docs/guides/de.md)
- [Technisches Modell](docs/access-model.md)
- [Fragen und Fehlerbehebung](docs/faq.md)
- [Einfaches Glossar](docs/glossary.md)
- [Datenschutz](docs/privacy.md)
- [Versionsstatus](docs/release-status.md)

## Erstellen und testen

Windows und .NET 10 SDK verwenden. Tests sind ein ausführbares Integrationsprogramm, nicht dotnet test. Das Entwicklerhandbuch erklärt Paketierung und Dokumentationsprüfung.

[Development](docs/development.md)

## Hilfe und Mitarbeit

Version, Windows-Version, Vorgang und Fehlercode angeben. Technik kopiert eine Diagnose ohne Pfade, Konten oder SIDs. Übersetzungen sind vorläufig; technische Nachweise können Englisch bleiben. Keine behauptete muttersprachliche Prüfung oder Assistenztechnik-Zertifizierung.

Erstellt von Samuel Sanches · ssanches011@gmail.com · [Apache-2.0](LICENSE) · [Security](SECURITY.md)
