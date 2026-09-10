# PermissionScope Access Test Corpus

Small, synthetic, redistributable permission fixtures under the repository's Apache-2.0 license. No production descriptor or real domain account is included.

Version 1 covers discretionary rights for an isolated synthetic SID. The harness passes each descriptor to Windows Authz after file generic-right mapping and checks the recorded hexadecimal mask. These expectations are verified by the harness; they are not a replacement for Windows access checks in the product.

Important counterexamples: a NULL DACL is not an empty DACL; a deny for one right does not deny every right; and noncanonical ordering can make an earlier allow relevant. Do not simplify these to a rule that every deny always wins.

The synthetic context does not establish an interactive or network logon, complete group membership, conditional claims, share access or successful file opening. Add cases with a clear context and reproducible Windows evidence. Bump the corpus schema when interpretation changes.
