# Accessibility: evidence and verification limits

PermissionScope treats accessibility as an engineering requirement, but it separates automated evidence from assistive-technology certification.

## Implemented native behavior

The WinUI application uses native controls, named navigation controls, keyboard accelerators, scrollable details and RTL layout. Theme brushes respond to Windows High Contrast. Current shortcuts include Ctrl+O for analysis, Ctrl+K/Ctrl+F for search, Ctrl+R for analysis, Ctrl+E for export and Escape for cancellation.

Critical controls expose stable AutomationIds so testing does not depend on translated button text. The Production Trust workflow launches the real native application against synthetic demo data across eight locales and Light/Dark themes. It verifies navigation, the analysis form, Access Path tabs, settings, comparison and simulation control reachability through Windows UI Automation. It also checks that obvious local host identifiers are not present in the synthetic accessibility tree.

These checks answer **“can automation reach and operate the intended native controls?”**. They do not answer every question a screen-reader user may encounter.

## Automated assurance

| Area | Automated evidence |
|---|---|
| Native control reachability | Windows UI Automation using stable AutomationIds |
| Locales | en-US, pt-BR, es, fr, de, ar, ja, zh-Hans |
| Themes | Light and Dark in Production Trust journeys |
| RTL publication surface | Arabic direction/layout is exercised by locale-specific application/site checks |
| Critical product journeys | Analyze surface, Access Path, Settings, Compare and Simulation |
| Responsive native layout | Existing UI regression resizes the application and reuses pages repeatedly |
| Website accessibility | axe checks, keyboard skip navigation, forced-colors and reduced-motion coverage |

## Manual acceptance still required

Before claiming screen-reader/accessibility certification, run and record representative manual journeys for:

- Windows Narrator reading order, labels, announcements and state changes;
- NVDA reading order and interactive-control announcements;
- keyboard-only operation without pointer input;
- 200% and 400% text/display scaling on representative screens;
- Windows High Contrast through the complete primary journey;
- focus visibility after navigation, dialogs, errors and asynchronous scan completion;
- long translated labels and Arabic RTL interaction;
- tables/lists, expanders, result states and Access Path evidence;
- error, cancellation and Unknown-state announcements.

A manual pass should record Windows build, PermissionScope commit/tag, locale, scaling, theme, assistive technology/version, tested journey and any blocked step. See [physical hardware validation](hardware-validation.md) for the evidence pattern.

## Website boundary

The static website is tested at multiple desktop/tablet/mobile widths in all eight languages, with Arabic RTL, keyboard skip navigation and automated axe WCAG A/AA checks. Forced-colors and reduced-motion modes are exercised. An automated pass does not prove complete WCAG conformance or replace assistive-technology testing.

## Reporting barriers

Report a reproducible barrier with locale, Windows version, scaling, theme, assistive technology and affected control. Avoid attaching personal paths, account names, SIDs or customer snapshots. Potential security issues belong in the private process described by `SECURITY.md`.

See also the [capability matrix](capability-matrix.md), [testing guide](testing.md) and [release scope](release-status.md).
