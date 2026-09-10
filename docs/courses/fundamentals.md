# PermissionScope Academy — Windows permission fundamentals

> Goal: understand the concepts PermissionScope evaluates before touching a real environment.

## 1. Mental model

Windows access is not a single “yes/no” flag. A resource has a security descriptor, a DACL contains access-control entries (ACEs), an identity carries group memberships, and Windows evaluates those rules in context.

PermissionScope does not replace Windows. It records the evidence it can read and uses Windows Authz for the evaluated discretionary decision.

## 2. Vocabulary

- **Identity** — user or security principal being evaluated.
- **SID** — stable security identifier used by Windows.
- **ACE** — one allow/deny rule in an ACL.
- **DACL** — ordered collection of discretionary access rules.
- **Inherited ACE** — rule inherited from a parent container; the inherited flag does not by itself prove the exact ancestor of origin.
- **Effective access** — result of evaluating the available permission context.
- **Unknown** — PermissionScope cannot safely prove Granted or Denied from the available context.

## 3. Safe lab

1. Open PermissionScope.
2. Choose **Explore the demo**.
3. Select the fictional identity `LAB\Alex`.
4. Observe the summary result.
5. Open **Access Path**.
6. Match every human-readable explanation to the technical evidence below it.

## 4. What to look for

Ask four questions in order:

1. Which identity is being evaluated?
2. Which groups/memberships are part of the recorded evidence?
3. Which ACEs contribute to the result?
4. Is any important context explicitly marked Unknown?

## 5. Important boundary

A DACL result is not a guarantee that opening a file will succeed. Integrity policy, encryption, locks, privileges and other Windows mechanisms can still matter.

## 6. Exercise

Before checking the answer, predict whether the demo identity should have read, modify or full-control rights. Then compare your reasoning with Access Path.

## 7. Completion check

You are ready for the next course when you can explain the difference between **Denied** and **Unknown** without using them interchangeably.

Next: [Reading Access Path](access-path.md)
