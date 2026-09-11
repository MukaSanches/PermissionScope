# Repository administration baseline

This document defines the intended GitHub-level settings for the official PermissionScope repository. It exists because several important controls live in GitHub settings rather than versioned files.

## Default branch

- Default branch: `main`
- Direct pushes to `main` should be exceptional.
- Normal product and repository changes should arrive through pull requests.

## Recommended branch protection / ruleset

For `main`, enable a repository ruleset or branch protection with the following baseline:

- require a pull request before merging;
- require status checks relevant to the affected change;
- require the branch to be up to date before merge when GitHub can do so safely;
- block force pushes;
- block branch deletion;
- dismiss stale approvals when the reviewed diff changes materially if multiple maintainers are active;
- require conversation resolution before merge;
- allow repository administrators to bypass only for documented emergency/security reasons.

For a single-maintainer project, requiring an external approval can make maintenance impossible. The important current controls are PR traceability, status checks and protected history. If additional maintainers join, required approving reviews should be enabled.

## Merge strategy

Preferred merge method for focused repository work: **squash merge**.

Use merge commits only when preserving a meaningful branch topology is valuable. Avoid mixing merge styles without a reason.

Commit titles should describe the shipped outcome, not the mechanics of editing files.

## Actions

- Keep third-party Actions pinned to immutable commit SHAs when practical.
- Dependabot should group minor/patch updates but isolate major updates for review.
- Workflows should use the minimum permissions required for their job.
- Release workflows should separate build/verification from publication.
- Historical version-specific workflows should not remain active after a version-independent replacement exists.

## Security

- Keep private vulnerability reporting enabled when available.
- Keep CodeQL enabled for supported languages.
- Treat a green CodeQL run as evidence, not certification.
- Do not place secrets in workflow files, issue templates or repository documentation.

## Issues and Discussions

- Blank issues are disabled.
- Bugs, features, documentation issues and high-impact proposals use structured forms.
- Usage questions and early ideas belong in Discussions when they are not yet actionable issues.
- Unpatched vulnerabilities never belong in public issues or discussions.

## Repository metadata

Keep these fields current:

- concise description focused on Windows permission analysis;
- project website/homepage;
- Apache-2.0 license detection;
- relevant topics such as `windows`, `permissions`, `ntfs`, `access-control`, `local-first`, `winui3`;
- social preview artwork consistent with `docs/brand/README.md`.

Avoid keyword stuffing or claims that are broader than the current product boundary.

## Releases

- Publish from verified source state.
- Provide SHA-256 checksums.
- Preserve SBOM/provenance evidence where the release workflow supports it.
- Never overwrite an existing release to silently change its contents.
- Keep known limitations visible in release notes.

## Current-settings audit note

Repository-file governance can be enforced in code review. GitHub-hosted controls such as branch protection/rulesets and social-preview configuration must be verified in the repository Settings UI because they are not fully represented by versioned files.
