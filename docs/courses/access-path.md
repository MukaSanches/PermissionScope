# PermissionScope Academy — Reading Access Path

> Goal: move from the summary result to the evidence behind it.

## 1. Start with the question

Never read Access Path without first stating the exact question: **which identity, which resource and which action are being evaluated?**

## 2. Read top to bottom

Use this sequence:

1. Confirm the selected identity.
2. Confirm the resource path.
3. Confirm the requested action/mask.
4. Review recorded memberships.
5. Review contributing permission entries.
6. Read every limitation or Unknown marker.
7. Compare the technical mask with the plain-language summary.

## 3. Separate evidence from interpretation

PermissionScope should help you distinguish:

- what Windows returned;
- what the application could verify;
- what remains outside the model;
- what the human-readable explanation means.

Do not treat a friendly summary as stronger evidence than the technical record beneath it.

## 4. Inheritance

An inherited flag tells you a rule was inherited. It does **not** prove which ancestor originally created that rule. Avoid overclaiming the source unless you have separate evidence.

## 5. Unknown is a safety feature

Unknown means the available context is insufficient for a reliable conclusion. Examples can include remote-token equivalence, conditional rules or unverified reparse targets.

Never rewrite Unknown mentally as “probably allowed” or “probably denied”.

## 6. Investigation exercise

Using the synthetic demo:

1. write your conclusion in one sentence;
2. list the evidence supporting it;
3. list every caveat shown by PermissionScope;
4. explain what additional evidence would be required to remove each caveat.

## 7. Diagnostic note template

Use this compact structure when sharing a finding:

- **Resource:** synthetic or redacted path
- **Identity:** synthetic or redacted identity
- **Observed result:** Granted / Partial / Denied / Unknown
- **Evidence:** contributing entries and memberships
- **Limitations:** every unresolved context item
- **Next verification:** the next safe action

Next: [Safe troubleshooting](troubleshooting.md)
