# Localization and accessibility

UI catalogs live in `src/PermissionScope.App/Locales`. `en-US` is the fallback; `pt-BR` covers the same keys. A locale can be added as a JSON catalog without a code change. Technical evidence and report headings currently remain in English. These are the two shipped translations; worldwide translation coverage is not claimed.

`qps-ploc` expands text; `qps-rtl` also mirrors layout. Technical paths and SIDs remain left-to-right. Dates and numbers use the Windows culture. Native controls provide Unicode shaping, CJK fonts, text selection, accessible names and focus behavior.

The navigation collapses below 1060 logical pixels; result panes stack below 680 content pixels. Controls wrap instead of relying on fixed horizontal rows. Initial window sizing accounts for monitor DPI. Application styling follows Light/Dark/System, and standard controls retain Windows accessibility behavior. Full high-contrast and screen-reader certification remain unverified.

Shortcuts: Ctrl+O path, Ctrl+K/Ctrl+F filter, Ctrl+E export, Ctrl+R rescan, Esc cancel. Evidence expanders and tabs are keyboard accessible. No decorative animation is used.
