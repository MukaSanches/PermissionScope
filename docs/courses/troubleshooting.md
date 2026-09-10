# PermissionScope Academy — Safe troubleshooting

> Goal: investigate access problems without turning diagnosis into uncontrolled permission changes.

## 1. Preserve the original state

Before changing anything, record what exists. Save an observation or snapshot and note the exact identity, resource and action being investigated.

## 2. Reproduce narrowly

Prefer the smallest reproducible case:

- one resource;
- one identity;
- one action;
- one known Windows host;
- synthetic data whenever possible.

Avoid testing broad trees when a single object can answer the question.

## 3. Read before changing

Use PermissionScope in this order:

1. Analyze.
2. Open Access Path.
3. Identify contributing entries.
4. Read Unknown/limitations.
5. Compare with the expected policy.
6. Only then consider a change.

## 4. Treat apply as a separate operation

Analysis and simulation are read-only. If you use an apply workflow, keep it explicit, scoped and reversible. Re-check the target immediately before applying and verify the result immediately afterwards.

## 5. High-risk situations

Stop and obtain additional evidence when the target involves:

- links/reparse points;
- remote resources;
- conditional access rules;
- protected operating-system resources;
- concurrent changes by another process or administrator;
- a result marked Unknown.

## 6. Rollback discipline

A safe change process has four artifacts:

1. pre-change observation;
2. intended change;
3. post-change verification;
4. rollback path.

If any of those is missing, the process is not ready for production use.

## 7. Support-quality report

When opening an issue, include:

- PermissionScope version;
- Windows version/build;
- minimal synthetic SDDL when possible;
- expected result;
- actual result;
- exact reproducible steps;
- safe diagnostic output.

Never post credentials, production snapshots, tokens, private account inventories or unredacted sensitive paths.

## 8. Completion check

You have completed the Academy starter path when you can diagnose a synthetic permission issue, explain the evidence, state the limitations and propose a reversible next step.
