# Accessibility: implemented behavior and verification limits

The native UI uses WinUI controls, named navigation buttons, keyboard accelerators, scrollable details and RTL layout. Theme brushes respond to system high contrast. Shortcut keys include Ctrl+O for analysis, Ctrl+K/Ctrl+F for search, Ctrl+R for analysis, Ctrl+E for export and Escape for cancellation.

Desktop automation exercises language selection and application journeys. The documentation pipeline invokes actual controls and captures native windows in eight languages and light/dark themes. These are functional checks, not screen-reader certification. Full Narrator reading-order, 200% text scaling and high-contrast journeys still require an interactive manual audit on representative displays.

The static website is tested at 375, 768 and 1440 pixels in all eight languages, with matching images, Arabic RTL, keyboard skip navigation and automated axe WCAG A/AA checks. Forced-colors and reduced-motion modes are exercised. An automated pass does not prove complete WCAG conformance or replace assistive-technology testing.

Report a reproducible barrier with locale, Windows version, scaling, theme, assistive technology and the affected control. Avoid attaching personal paths or account names. See [release scope](release-status.md).
