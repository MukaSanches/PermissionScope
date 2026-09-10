# Publication state and checkpoints

Preparation is not publication. Do not list a service as live until its own response confirms that status.

1. Authorize GitHub CLI with the official `gh auth login --hostname github.com --git-protocol https --web` flow. No password, token, browser cookie or recovery code belongs in this repository or a chat message.
2. Review source, run the clean build, tests, UI regression, resource checks and website tests. Freeze code and rebuild the affected packages once.
3. Review and commit source, then run `build/Publish-GitHub.ps1`. It stops on an unexpected account, remote, dirty source tree or existing release. It does not overwrite released artifacts.
4. Verify GitHub build/security/Pages workflow outcomes. The script creates a draft release; publish it only after the remaining desktop and release checks pass. Verify public site/download URLs after publication. Configure main-branch protection after the real check names are established. Existing public releases are immutable.
5. Run `build/Prepare-Channels.ps1 -VerifyPublished`. This downloads the published installers and checks their hashes before generating channel drafts. Without that switch it only creates local drafts from local artifacts.
6. Validate the WinGet manifests with `winget validate`, test in a clean Windows environment, then submit one PR to microsoft/winget-pkgs. Draft manifests do not establish availability of `winget install PermissionScope`.
7. Package/test the Chocolatey draft with its official tools and submit through an authorized maintainer account. Wait for platform verification/moderation; do not represent an untested draft as approved.
8. In Partner Center, Samuel must complete any personal identity verification, legal attestations or security challenge. Use the assigned package identity after the product name is actually reserved. The supplied Store listings and unsigned MSIX are preparation only.
9. Publish launch listings only after working public downloads, through eligible authenticated accounts and in accordance with community rules.

No email account or invented identity is required for the local product. The project does not collect or transmit account secrets.
