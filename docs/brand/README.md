# PermissionScope brand system

PermissionScope should look precise, calm and verifiable. The visual system is intentionally closer to a security/product engineering brand than to a generic “cybersecurity” aesthetic.

## Core idea

**See who has access. Understand why.**

The mark combines two ideas: a shield for authorization boundaries and a scope for inspection. The product should never imply certainty where the Windows context is incomplete; the brand should communicate the same restraint.

## Primary palette

| Token | Value | Role |
|---|---|---|
| Permission Blue | `#2764E7` | Primary action, evidence links, focus accents |
| Signal Blue | `#6D96F0` | Depth, gradients, secondary accents |
| Scope Cyan | `#72D5FF` | Sparse highlights and active signal |
| Deep Navy | `#17315C` | Trust surfaces and institutional sections |
| Night | `#111722` | Dark presentation surfaces |
| Paper | `#F7F8FA` | Light background |

Use blue as evidence/action, not decoration. Avoid neon-heavy “hacker” styling, aggressive red/green palettes, fake certification seals, glossy stock imagery, excessive glassmorphism, and decorative animations that compete with content.

## Typography

Primary UI and web typography: `Segoe UI Variable`, then `Segoe UI`, then the system sans-serif fallback. Technical evidence may use `Cascadia Mono` or `Consolas`.

Typography should use a small number of strong sizes, tight display letter spacing, generous line height for body copy, and restrained all-caps only for short section kickers.

## Motion

Motion is functional: short hover elevation, restrained spatial depth on the product capture, and gentle ambient geometry. The website respects `prefers-reduced-motion` and should remain fully understandable with all animation disabled.

## Project assets

- `project-avatar.svg` — square project mark for profiles, documentation and future organization branding.
- `repository-banner.svg` — wide repository/README banner source.
- `../../site/logo.svg` — production website/app wordmark symbol.

## Repository identity

GitHub does not provide a separate avatar for each repository. A repository normally inherits the owner or organization avatar. For a project-specific identity, use the repository social preview, README/banner artwork and a consistent release/documentation visual system. If PermissionScope later moves to a dedicated GitHub organization, `project-avatar.svg` is suitable as the organization avatar after raster export.

## Social preview specification

For GitHub repository social preview, export the wide banner to a **1280×640 PNG** under 1 MB with a solid background. Keep the product name and mark inside a central safe area so previews remain legible after platform cropping.

## Visual quality bar

A new visual element should answer at least one of these questions:

1. Does it help users understand the product?
2. Does it expose evidence or trust information?
3. Does it improve navigation or hierarchy?
4. Does it strengthen a consistent PermissionScope identity?

If not, it should usually be removed. Restraint is part of the brand.
