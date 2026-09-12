# 0.8 handoff — profiles and reliability

Requirements: PROF-01, APP-03, DESK-01, CAT-11, REL-02 and QA-08. User-authorized scope excludes browser integration, permits deferred live testing over RDP, selects protected preview signing, and requests GitHub source plus release publication.

Implemented behavior and precedence are in [19-profiles-and-reliability.md](19-profiles-and-reliability.md). Start there before changing rules or the updater. State schema is 3 and the procedural rig contract is 5. Work inherits existing rules; fresh installs keep app guard off. Manual profile selection disables schedules. No process termination, input injection, screen capture, URLs, titles, or typed content were added.

## Source map

- `FocusProfiles.cs`, `StateStore.cs`: schedules, exceptions, grace, usage, schema migration and normalization.
- `DesktopPlacement.cs`, `DesktopAttention.cs`: normalized spots and optional transient caret/control geometry.
- `CompanionHost.cs`, `DesktopApps.cs`: precedence, current foreground sampling, cancellation and normal-close routing.
- `CatRig.cs`, `Companion.cs`, `TurnTransition.cs`, `CatPainter.cs`: notice/ear/yawn/celebration and continuous accessory anchors.
- `MainWindow.Profiles.cs`, `MainWindow.Updates.cs`, ordinary main window and pet menu/XAML: native configuration and working profile submenu.
- `UpdatePolicy.cs`, `ReleaseUpdates.cs`: bounded GitHub metadata/downloads, SHA-256, same-public-key Windows Authenticode verification and recovery.
- `sign-artifact.ps1`, `prepare-uiaccess-review.ps1`, installer files: reusable protected CNG signing, RFC 3161 timestamps, signed uninstaller and pre-install recovery validation.
- `test-installer.ps1`, core/native policy checks and `.github/workflows/windows-checks.yml`: repeatable verification and declared CI matrix.

## Executed evidence

Release build: no warnings/errors. Core: 160 passed. Native: 79 passed in `artifacts/qa-v080-release`. Native close/refusal fixtures used the actual foreground identity; no user app was selected. The policy cancellation fixture supplies its test-owned foreground identity deliberately. Walk/run frame samples averaged about 16.66 ms, p95 about 17.03 ms; this is a short local RDP sample, not a hardware performance guarantee.

Package: 19 checks passed in `artifacts/package-v080-final`, including signatures/timestamps, registry, protected recovery identity, tampering/different-publisher/version rejection, data-preserving uninstall and reinstall. An earlier package test found Inno's padded version metadata and a post-install exception that returned zero; both are corrected. The shipped setup SHA-256 is recorded in build status and the release checksum. Raw diagnostics and state backups stay ignored.

Repair directly from the cached installer also passed. The first hosted CI run exposed the fixture's fixed window placement on a smaller monitor; it now uses the available work area. The production app correctly rejected the off-monitor button.

Visuals: code-generated personality and light/dark native UI exports were reviewed at desktop size. Curated files are `docs/images/personality.png`, `profiles.png`, and `desktop-settings.png`. Exported frames/images are never runtime assets.

## Signing and operational limits

The approved reusable preview certificate is `93ECC442E6EC72D1238D6BCA2E8F80D9B01D88EE`, expiring 12 September 2029. Its nonexportable private key remains in the current user's CNG store (export policy None); never export or commit it. Preparation reuses it and does not add trust. The installer is signed and timestamped but still needs explicit preview trust; no public publisher/SmartScreen claim is made.

The installed path and AppId remain stable. The updater never silently imports trust or installs software. 0.8 is the first compatible recovery baseline; 0.7's unsigned setup is not offered as verified rollback. Future-version rollback, full live Windows 11 notification smoke, editor avoidance, mixed DPI/hotplug, RDP reconnect and standard-user UIAccess are still desktop test gates. The user explicitly allowed these to be tested later. Continue without a computer plugin where native fixtures and code-rendered visuals suffice.

Next dependency-ready work is that desktop acceptance matrix and a two-version upgrade/rollback exercise after the next signed build exists. Browser URL work requires a new explicit request.
