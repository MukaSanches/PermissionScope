# Maintainer triage runbook

This runbook keeps PermissionScope issues and pull requests actionable without turning repository activity into an informal backlog.

## First-pass triage

For every new public issue or proposal, determine in this order:

1. **Is it safe to discuss publicly?** If it may disclose an unpatched vulnerability, move the reporter toward the private route in `SECURITY.md` and avoid eliciting exploit details in public.
2. **Is it reproducible or concrete?** Ask for the smallest privacy-safe example when evidence is missing.
3. **What project surface is affected?** Permission decision, identity, mutation, UI, export, packaging, documentation, repository operations or support.
4. **What risk class applies?** Use the Class 1/2/3 model in `GOVERNANCE.md`.
5. **What is the next action?** Reproduce, request information, accept as proposal, convert to discussion, close as duplicate/not planned, or schedule implementation.

## Priority model

Priority is driven by impact and evidence, not by comment volume.

| Priority | Typical condition | Maintainer action |
|---|---|---|
| P0 | Credible package/release compromise or actively dangerous state-changing defect | Contain first; coordinate privately; suspend affected distribution if necessary |
| P1 | Incorrect effective-access conclusion, data-safety defect, rollback failure, severe privacy exposure | Reproduce promptly and prioritize a regression-backed fix |
| P2 | Reproducible functional regression, installer failure, important accessibility break | Schedule ahead of ordinary enhancements |
| P3 | Normal product improvement, documentation issue, quality/performance work | Prioritize by evidence, reach and maintenance cost |
| P4 | Idea without a concrete problem or evidence | Keep in discussion/proposal state until actionable |

This table is an internal prioritization aid, not a contractual response-time SLA.

## Issue outcomes

A well-triaged issue should end the first useful maintainer pass in one of these states:

- **actionable** — enough evidence exists to implement or investigate;
- **needs information** — reporter knows exactly what evidence is missing;
- **discussion first** — valuable idea, but not concrete enough for implementation tracking;
- **duplicate** — canonical issue is identified;
- **not planned** — outside product direction, unsafe, disproportionately costly or contradicted by project principles;
- **security/private** — public thread contains no sensitive exploit detail and private handling is used.

When closing something as not planned, prefer a short reason tied to a documented project boundary rather than a bare close action.

## Pull request triage

Before reviewing implementation detail, verify:

- the PR states a real outcome rather than only listing files changed;
- scope is focused enough to review as one decision;
- risk class is plausible;
- verification is appropriate to the affected surface;
- known limitations are explicit;
- generated content was changed through its source of truth;
- security-sensitive changes include failure-mode evidence;
- major dependency upgrades are isolated rather than hidden in a broad batch.

A green CI run does not resolve architectural or security-review questions.

## Dependency updates

Minor and patch dependency updates may be grouped when they remain independently understandable. Major updates should normally be isolated so compatibility impact and rollback are clear.

Do not merge a dependency update solely because Dependabot opened it. Review upstream release notes, runtime requirements and security implications, then rely on the repository verification gates.

## Stale work

Do not keep obsolete pull requests open merely as historical bookmarks. If later work has superseded a divergent branch, close it with an explanatory comment; GitHub already preserves commits and discussion history.

Avoid automated stale-closing policies for legitimate security, architecture or accessibility discussions unless the project later has enough volume to justify them.

## Labels

If repository labels are standardized in GitHub Settings, prefer a small vocabulary that describes **type, priority and state** rather than every subsystem. Suggested concepts:

- type: bug, enhancement, documentation, security, dependencies;
- priority: P0–P4 when prioritization is actually needed;
- state: needs-info, blocked, ready, discussion;
- platform: x64, ARM64 only when architecture matters.

Do not use labels as a substitute for a clear issue title and maintainer comment.

## Periodic hygiene

At reasonable intervals, review:

- open PRs that are no longer mergeable or have been superseded;
- dependency PRs that combine breaking changes;
- Actions workflows that target obsolete versions;
- documentation that still claims an old release boundary;
- issue forms and contact links that route people incorrectly;
- repository protection/settings against `REPOSITORY-SETTINGS.md`.

Repository hygiene should reduce ambiguity, not erase useful history.
