# PermissionScope decision records

This directory stores durable records for architectural, security-boundary and long-lived engineering decisions.

Decision records exist so a future maintainer can understand **why** a choice was made, what alternatives were rejected and what evidence would justify revisiting it.

They are not release notes, marketing pages or meeting transcripts.

## When to write one

Create a decision record when a change materially affects one or more of these areas:

- trust boundaries;
- effective-access or identity semantics;
- the meaning of Unknown;
- filesystem mutation or rollback;
- persistent data formats with compatibility cost;
- release integrity or provenance;
- a long-lived platform/dependency constraint;
- a deliberate tradeoff that will otherwise look accidental later.

Routine bug fixes, editorial corrections and reversible implementation details usually do not need a record.

## Naming

Use a four-digit sequence followed by a short lowercase slug:

```text
0001-preserve-unknown-for-incomplete-context.md
0002-package-provenance-model.md
```

`0000-template.md` is the canonical template and is not itself a project decision.

## Status

Use one of these states:

- **Proposed** — under review;
- **Accepted** — current project decision;
- **Superseded** — replaced by a later record;
- **Rejected** — considered but intentionally not adopted;
- **Deprecated** — still historically relevant but no longer recommended.

Do not silently rewrite an accepted record to make history look cleaner. For a material reversal, create a new record and link the superseded one.

## Quality standard

A useful record answers:

1. What problem existed?
2. What constraints mattered?
3. What options were considered?
4. What was chosen and why?
5. What are the consequences and failure modes?
6. What evidence would justify revisiting the decision?

See [`GOVERNANCE.md`](../../GOVERNANCE.md) for decision authority and risk classes.
