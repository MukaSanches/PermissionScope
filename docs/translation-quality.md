# Language coverage

English and Brazilian Portuguese are the original UI catalogs. Additional Spanish, French, German, Arabic, Japanese and Simplified Chinese catalogs are explicitly labeled translation previews. They were not independently reviewed by native-language translators. Missing keys fall back to English; backend evidence and reports still contain English technical text.

`node build/validate-resources.mjs` checks duplicate/unknown keys, required safety messages and placeholders, and writes exact coverage to `artifacts/localization-coverage.json`. Coverage is not a fluency score or a claim of human verification.

Resource discovery is automatic. Add a valid Windows culture JSON file to `src/PermissionScope.App/Locales` without editing a language list. Resolution uses the requested culture, its parents, a same-language regional resource when appropriate, then en-US. Simplified and Traditional Chinese are not substituted for one another. Regional formatting remains separate from resource fallback. RTL cultures are identified through CultureInfo rather than a hand-maintained list.

Paths and SIDs retain left-to-right presentation. Debug builds expose pseudo-localization options; release menus do not expose synthetic languages. Verify compact/wide layouts, CJK font fallback and actual RTL interactions before promoting a preview to reviewed status.

The public website currently provides English and Brazilian Portuguese. Full worldwide translation and native-language review remain release boundaries, not completed claims.
