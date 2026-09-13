# From GitHub 1.0.0 to production distribution

Checked against primary sources on 13 September 2026. The user explicitly requested version **1.0.0**, a new GitHub release and an explanation of the remaining production work. The requested GitHub release is a normal release, not marked pre-release. Its status/number does not certify the Windows publisher or replace desktop acceptance testing.

## What 1.0.0 currently provides

The current companion includes the code-drawn cat, wardrobe, themes, focus/app rules, supported notification paws and corrected cat/tray menu window. The app, setup helper, installer and uninstaller are SHA-256 signed and timestamped with the existing **self-signed development identity**. The installer still asks new PCs to trust that certificate. This is disclosed in setup and release notes. It is not a public-CA publisher identity and does not establish SmartScreen reputation.

Version 1.0.0 checks regular GitHub releases only; draft and pre-release entries are excluded from its update feed. Downloads retain SHA-256, size/URL and same-publisher-public-key verification. State schema remains 4. Browser integration is deferred, so it is not a release requirement for this companion-only scope.

## 1. Resolve signing and UIAccess together

Choose a legal publisher identity. Obtain either a Microsoft Artifact Signing **Public Trust** profile or a publicly trusted code-signing certificate/service from an appropriate CA. Use an HSM/token or managed signing service as required by that provider; never commit/export a private key into the repository or installer. Keep SHA-256 signatures and RFC 3161 timestamps for the app, helper, installer and uninstaller.

Artifact Signing requires an Azure account/subscription, identity validation, an Artifact Signing account and a Public Trust certificate profile. Identity validation is completed through the Azure portal. At the time checked, individual Public Trust enrollment is limited to the US and Canada; organization availability covers a broader listed set of countries. Check eligibility for the actual publisher before buying or configuring anything. Private Trust profiles are not a substitute for public distribution trust. Current setup: [Artifact Signing quickstart](https://learn.microsoft.com/en-us/azure/artifact-signing/quickstart), [overview](https://learn.microsoft.com/en-us/azure/artifact-signing/overview), [signing integrations](https://learn.microsoft.com/en-us/azure/artifact-signing/how-to-signing-integrations).

**A new certificate alone is not sufficient for this installer.** `SetupSupport.cs` currently implements a local-preview trust workflow: it imports the bundled leaf certificate into LocalMachine Root. A production build must validate its existing public trust chain and skip that import/preview-consent branch. Preserve the ordinary notification-permission explanation. On upgrade, remove only old preview trust proven to belong to Cute Cat; do not remove unrelated certificates. Rerun clean-install/upgrade/uninstall tests on a machine where the preview certificate has never been installed.

The current always-above-notification effect uses `uiAccess=true`. Microsoft states that UIAccess should not be used by non-assistive applications or merely to appear above other Windows UI. Public signing and Program Files installation meet technical trust/location requirements, but do not change that intended-use guidance. Review this before presenting the current mechanism as production-ready. The conservative public path is a signed **standard-permission build** and adjusted notification animation/layering. Retain UIAccess only with a supported accessibility purpose and the relevant distribution/certification requirements confirmed; do not invent an assistive-technology claim. Reference: [Microsoft UIAccess guidance](https://learn.microsoft.com/en-us/windows/win32/winauto/uiauto-securityoverview).

## 2. Plan the signing-key transition

The updater pins the running app's signing **public key** in `InstallerSignature.SamePublisher`. A new CA/Artifact Signing identity normally has a different key, so existing installations will reject it as an automatic update. That protection must not simply be disabled.

Use a documented manual reinstall through an independently verified public-signed installer, or implement a separately reviewed transition release that explicitly trusts the new identity. Test migration and recovery across that boundary. Microsoft says Artifact Signing certificates renew daily and last 72 hours; it explicitly warns against durable pinning to an end-entity public key or thumbprint and provides a subscriber-identity EKU for durable identity verification. Integrate that documented identity mechanism with Windows signature validation before promising automatic updates through this service. The install helper's current direct `NotAfter` check also needs a production path that respects timestamped Authenticode validity rather than rejecting a still-valid installer after its leaf certificate expires. Review `ReleaseUpdates.cs`, `SetupSupport.cs`, `CertificateHistory.json` and both signing scripts together. Reference: [Artifact Signing certificate management](https://learn.microsoft.com/en-us/azure/artifact-signing/concept-certificate-management).

## 3. Finish the desktop acceptance matrix

Use fresh supported Windows 11 VMs/accounts with default security settings and no development certificate installed. Test both a standard user and an administrator account. Keep the app's normal runtime at the intended user privilege; installing under Program Files can request elevation, but daily use should not require an administrator service.

Exercise actual user clicks for cat and hidden-tray menu opening, outside-click dismissal, Escape, submenus and reopening. Then test real Windows notifications, selected-app close requests and save refusal, lock/sleep/RDP reconnect, multiple monitors/negative coordinates/mixed DPI, themes/high contrast/Narrator/keyboard navigation, startup, upgrades and recovery. Menu visibility checks and simulated activation tests remain useful regressions; they do not prove physical desktop click delivery. Record exact build/hash/OS and results. Complete the existing idle/active performance checks on ordinary laptops and verify cleanup after crashes.

## 4. Make delivery repeatable

1. Freeze the release code, resolve known regressions and bump the version consistently.
2. Build from the exact source commit in a clean environment; run core/native/packaging checks appropriate to the change.
3. Sign and timestamp through the protected publisher service. Verify public trust on a clean machine, not only on the developer PC.
4. Generate SHA-256 checksums and updater metadata from the final signed installer; preserve the prior verified version for recovery.
5. Draft release notes with supported Windows versions, upgrade behavior and known limits. Finish validation before publishing the final tag/release. Do not overwrite released assets/tags with different binaries.
6. Publish the installer, checksum and metadata; download them again and verify the checksum and signature. Use the normal release/Latest flags only for the chosen stable channel.

Current `scripts/test-installer.ps1` already covers registration, signatures, recovery integrity and tampered/different-publisher rejection. Its trust check runs in the machine's current trust configuration; it is not a substitute for the clean-machine public-trust test above.

GitHub's pre-release checkbox is release metadata, not Windows certification. Instructions: [GitHub release management](https://docs.github.com/en/repositories/releasing-projects-on-github/managing-releases-in-a-repository). SmartScreen also evaluates reputation; signing does not guarantee an immediate warning-free first download. Do not instruct users to disable SmartScreen. Reference: [Microsoft Defender SmartScreen](https://learn.microsoft.com/en-us/windows/security/operating-system-security/virus-and-threat-protection/microsoft-defender-smartscreen/).

## 5. Choose a distribution route and publish support basics

Direct distribution of a properly signed installer through GitHub/a website is a valid route; Microsoft Store is optional. For the Store, create the appropriate Partner Center account, reserve the app identity, prepare the supported package/submission type, supply listing/privacy/support information and pass certification. MSIX signing/distribution is different from a direct EXE release, and Store submission does not automatically waive UIAccess/restricted-capability rules. References: [Store publishing](https://learn.microsoft.com/en-us/windows/apps/publish/), [MSIX requirements](https://learn.microsoft.com/en-us/windows/apps/publish/publish-your-app/msix/app-package-requirements).

Finalize the product name and publisher details. Decide whether source reuse is permitted and add the chosen project license; the current repository has third-party notices but no root project LICENSE, and publishing source alone is not a general reuse license. Do not choose a license on the user's behalf. Provide plain-language privacy information, supported configurations, a support/bug-report route, security-reporting instructions and a changelog. Preserve dependency/runtime notices and review the applicable Inno Setup distribution terms ([official site](https://jrsoftware.org/isinfo.php)). The app can remain completely free while the publisher pays signing/distribution costs.

Recommended next order: decide UIAccess/standard-mode scope → obtain the publisher identity and signing provider → implement production installer/update trust migration → clean-machine desktop acceptance → publish a production-qualified maintenance release. No accounts, certificate purchase, root-trust removal, store submission or licensing decision was performed merely to write this guide.
