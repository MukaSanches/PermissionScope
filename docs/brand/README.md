# PermissionScope brand system

PermissionScope should look precise, calm and verifiable. The visual system is intentionally closer to a security/product engineering brand than to a generic “cybersecurity” aesthetic.

## Core idea

**See who has access. Understand why.**

The mark combines two ideas: an authorization boundary and a scope/path for inspection. The product should never imply certainty where the Windows context is incomplete; the brand should communicate the same restraint.

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

## Product presentation

The website product scene is an **original PermissionScope workstation illustration**, not a rendering of a third-party product. Its physical language takes cues from real premium professional-display engineering: thin metal enclosure, optically dark screen edge, central camera/sensor detail, integrated audio/microphone cues, articulated support, controlled cable/port detail and a stable metal base.

The goal is believable industrial design, not brand imitation. Do not add third-party logos, model names, proprietary UI, certification marks or styling that could imply endorsement. The PermissionScope application screenshot inside the display must remain a genuine product capture.

The workstation scene should feel physically plausible while remaining subordinate to the product evidence. Environmental props — desk, keyboard, mouse, local hardware node and cable — exist only to create scale and context. They should never become decorative noise.

## Material and depth rules

- Metal uses restrained silver/graphite value changes rather than mirror-like chrome.
- Glass uses subtle reflections; avoid opaque “frosted card” effects over primary content.
- Shadows should describe contact, height and material separation, not create a floating-interface aesthetic.
- Camera, microphone, audio, hinge and port details are small enough to reward inspection without distracting at normal reading distance.
- Pointer depth is optional enhancement. The scene must remain convincing and fully readable with motion disabled.
- Mobile presentation simplifies physical context rather than shrinking desktop detail until it becomes visual noise.

## Motion

Motion is functional: short hover elevation, restrained spatial depth on the product capture, and gentle ambient geometry. The website respects `prefers-reduced-motion` and should remain fully understandable with all animation disabled.

## Project assets

- `project-avatar.svg` / `project-avatar.png` — square project mark for profiles and documentation.
- `repository-banner.svg` — editable source for the premium repository hero.
- `repository-banner.png` — optimized raster used by generated localized READMEs.
- `../../site/logo.svg` — production website/app symbol.
- `../../site/atelier.css` — final web art-direction layer.
- `../../site/atelier.js` — progressive workstation and navigation enhancements.

## Repository identity

GitHub does not provide a separate avatar for each repository. A repository normally inherits the owner or organization avatar. For a project-specific identity, use the repository social preview, README/banner artwork and a consistent release/documentation visual system. If PermissionScope later moves to a dedicated GitHub organization, `project-avatar.svg` is suitable as the organization avatar after raster export.

Every localized README deliberately points to the same shared banner asset. Updating `repository-banner.png` refreshes the first visual surface across all README languages without creating localized artwork drift.

## Social preview specification

For GitHub repository social preview, export the wide banner to a **1280×640 PNG** under 1 MB with a solid background. Keep the product name and mark inside a central safe area so previews remain legible after platform cropping. The README banner may use a wider editorial crop; the social preview should be exported separately rather than stretched.

## Visual quality bar

A new visual element should answer at least one of these questions:

1. Does it help users understand the product?
2. Does it expose evidence or trust information?
3. Does it improve navigation or hierarchy?
4. Does it strengthen a consistent PermissionScope identity?
5. Would it still make sense if all animation and visual effects were removed?

If not, it should usually be removed. Restraint is part of the brand.
