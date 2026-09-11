# PermissionScope brand system

PermissionScope should look precise, calm and verifiable. The visual system is intentionally closer to a security/product-engineering brand than to a generic “cybersecurity” aesthetic.

## Core idea

**See who has access. Understand why.**

The mark combines two ideas: an authorization boundary and a scope/path for inspection. The product should never imply certainty where the Windows context is incomplete; the brand should communicate the same restraint.

## Brand principles

1. **Evidence over spectacle.** Visual polish should help people understand trust, product behavior or navigation.
2. **Calm over alarm.** Permission analysis is serious without needing fear-based visual language.
3. **Precision over decoration.** Every badge, screenshot, table and claim should have a reason to exist.
4. **Originality over imitation.** Never borrow third-party logos, proprietary UI or certification language to manufacture credibility.
5. **Restraint is part of the identity.** PermissionScope should feel engineered, not over-produced.

## Primary palette

| Token | Value | Role |
|---|---|---|
| Permission Blue | `#2764E7` | Primary action, evidence links, focus accents |
| Signal Blue | `#6D96F0` | Depth, gradients, secondary accents |
| Scope Cyan | `#72D5FF` | Sparse highlights and active signal |
| Deep Navy | `#17315C` | Trust surfaces and institutional sections |
| Night | `#111722` | Dark presentation surfaces |
| Paper | `#F7F8FA` | Light background |

Use blue as evidence/action, not decoration. Avoid neon-heavy “hacker” styling, aggressive red/green palettes, fake certification seals, glossy stock imagery, excessive glassmorphism and decorative animation that competes with content.

## Typography

Primary UI and documentation typography: `Segoe UI Variable`, then `Segoe UI`, then the system sans-serif fallback. Technical evidence may use `Cascadia Mono` or `Consolas`.

Typography should use a small number of strong sizes, tight display letter spacing, generous line height for body copy and restrained all-caps only for short section kickers.

## GitHub presentation system

The repository is a product surface in its own right. It should communicate engineering maturity before a visitor reads source code.

### Repository hero

The first viewport should establish, in order:

1. PermissionScope identity;
2. the one-sentence product promise;
3. build/security/release evidence;
4. the most important next actions;
5. a genuine product capture.

Do not crowd the hero with vanity counters, large emoji collections, donation prompts, fake awards or badges that do not lead to evidence.

### Badges

Use badges only when they represent a meaningful state such as build, CodeQL, license or current release. Prefer a small, stable badge set over a wall of status images.

Badges are evidence pointers, not decoration.

### Screenshots

Repository screenshots must be genuine PermissionScope captures or clearly disclosed synthetic fixtures. Do not alter results to make a screenshot more attractive. If identities or ACLs are synthetic, say so in the surrounding text or alt text.

### Tables and information density

Use tables when they improve scanning across parallel concepts such as trust properties, platform support or navigation. Avoid decorative tables whose only function is visual centering.

A README section should still make sense in plain text and on narrow GitHub mobile layouts.

### Editorial voice

Repository writing should be:

- direct but not aggressive;
- technical without unnecessary jargon;
- explicit about limits;
- accessible to a non-specialist when possible;
- free of unverifiable superlatives such as “unbreakable”, “perfect security”, “military-grade” or “100% accurate”.

Prefer:

> Evaluated with Windows Authz. Unsupported context remains Unknown.

Avoid:

> The ultimate permission engine with perfect enterprise-grade accuracy.

### Trust language

Never use visual or textual language that implies Microsoft, GitHub or another vendor has certified or endorsed PermissionScope unless a documented official program actually provides that status.

A green workflow means that workflow passed. It is not a security certification.

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

Motion is functional: short hover elevation, restrained spatial depth on the product capture and gentle ambient geometry. Motion-enhanced surfaces should respect `prefers-reduced-motion` and remain understandable with animation disabled.

## Project assets

- `project-avatar.svg` / `project-avatar.png` — square project mark for profiles and documentation.
- `repository-banner.svg` — editable source for the repository hero.
- `repository-banner.png` — optimized raster used by generated localized READMEs.
- `README-header.html` — compact reusable header fragment.

The site has its own implementation files; brand governance should not require repository-documentation changes to alter the deployed site.

## Repository identity

GitHub does not provide a separate avatar for each repository. A repository normally inherits the owner or organization avatar. For a project-specific identity, use the repository social preview, README/banner artwork and a consistent release/documentation visual system.

If PermissionScope later moves to a dedicated GitHub organization, `project-avatar.svg` is suitable as the organization avatar after raster export.

Every localized README deliberately points to the same shared banner asset. Updating `repository-banner.png` refreshes the first visual surface across README languages without creating localized artwork drift.

## Social preview specification

For GitHub repository social preview, export a **1280×640 PNG** under 1 MB with a solid background. Keep the project name, mark and promise inside a central safe area so platform cropping does not remove them.

The social preview should be exported separately rather than stretching the README banner.

## Accessibility

- Maintain readable contrast between text and backgrounds.
- Never encode meaning only by color.
- Write useful alt text for screenshots and brand assets.
- Avoid tiny text baked into raster graphics.
- Do not require hover, motion or color perception to understand repository navigation.

## Visual quality bar

A new visual element should answer at least one of these questions:

1. Does it help users understand the product?
2. Does it expose evidence or trust information?
3. Does it improve navigation or hierarchy?
4. Does it strengthen a consistent PermissionScope identity?
5. Would it still make sense if all animation and visual effects were removed?

If not, it should usually be removed. Restraint is part of the brand.
