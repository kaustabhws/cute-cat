# Windows setup and uninstall contract

**0.7 update:** the same stable AppId and canonical install directory are retained. The installer now packages app rules, idle behavior, accessories and modern menus. Upgrade from the installed 0.6.1 build and 0.7 removal/reinstall were exercised. Certificate rotation only retires identities explicitly listed in `installer/CertificateHistory.json` when the previous installation's protected metadata proves ownership. Current artifact: `CuteCat-0.7.0-Setup.exe`; current behavior is documented in `18-focus-companion.md`. The older section below records the installer foundation.

Application version: 0.6.1. Setup packaging revision: 0.6.1.1. The user accepted the notification fix and requested a proper installation wizard, a Windows Installed apps entry, and uninstall from that entry. This work packages the existing signed app without changing its cat engine or notification code.

## User flow

`dist/installer/CuteCat-0.6.1-Setup.exe` opens a native Inno Setup wizard with the original code-rendered cat, Windows light/dark styling, welcome, local-preview information, notification access, desktop-shortcut choice, confirmation, progress and completion pages. It adds a Start menu shortcut and, if selected, a desktop shortcut. Startup is not enabled by Setup.

The wizard installs for this PC under Program Files. New installations use `C:\Program Files\Cute Cat`. An existing, verified 0.6.1 manual installation is adopted in place; the stable Inno AppId preserves its location on reinstall.

Users remove the app through **Settings → Apps → Installed apps → Cute Cat → Uninstall**. The registered uninstaller closes Cute Cat, removes its installed files, shortcuts, registration, and certificate trust owned by Cute Cat. Settings and focus history under `%LOCALAPPDATA%\CuteCat` are preserved. Empty known program directories are removed even when inherited from the preceding manual installation. Unexpected files are not recursively deleted.

## Windows integration

Stable AppId: `{1597EF40-EF65-4A51-A78C-A82FBAA17344}`. Never regenerate this ID between releases.

The 64-bit HKLM uninstall entry is:

`Software\Microsoft\Windows\CurrentVersion\Uninstall\{1597EF40-EF65-4A51-A78C-A82FBAA17344}_is1`

Inno owns DisplayName, DisplayVersion, DisplayIcon, Publisher, InstallLocation, InstallDate, EstimatedSize, UninstallString and QuietUninstallString. Modify/Repair are disabled; rerunning Setup reinstalls the app. Do not add a second hand-written registration beside Inno's entry.

The setup helper uses Windows Restart Manager with the exact installed executable resource. Other processes returned by Restart Manager are excluded from shutdown. No force-termination flag, global process-name kill, input injection or service is used. The native uninstall log owns installed file removal. The helper never deletes the user's app-data directory.

`setup-state.ini` records certificate ownership inside the protected install directory. A valid old manual receipt can transfer ownership into this record. Reinstall preserves it. Missing/corrupt optional ownership metadata does not block program removal; certificate trust is retained when ownership cannot be proved.

## Existing UIAccess authorization

This local setup includes the already-reviewed signed UIAccess app and the same public certificate; no private signing key is stored or regenerated. The certificate expires **12 October 2026**. See [UIAccess context and limits](16-uiaccess-review.md). This is not a publicly signed production installer or a claim of accessibility-product eligibility.

On a PC where that certificate is not already trusted, the notification-access checkbox starts unchecked and explains the broader UI access and machine-wide certificate trust change. Cancelling before installation adds no trust. Failed installation rolls back trust newly added by that attempt. An unattended install without prior trust must include the explicit `/ACCEPTUIACCESS=1` acknowledgment; otherwise it exits without installing or adding trust. Ordinary silent mode is not consent.

The application executable is signed; the setup wrapper and generated uninstaller are currently unsigned development executables. Public distribution requires a durable signing identity and a signing/upgrade strategy. Do not replace the app's signed executable with a rebuilt unsigned one.

## Build

Run `scripts/build-installer.ps1`. It verifies every reviewed payload hash, generates the sidebar and icon images using the shared CatPainter/CatRig code, compiles the small setup helper, and runs Inno Setup. The app runtime is bundled; end users need no .NET SDK. The setup helper targets the .NET Framework included with Windows 11.

Compiler: Inno Setup **7.1.0 x64**, obtained from the [official release](https://github.com/jrsoftware/issrc/releases/tag/is-7_1_0). Download SHA-256: `0362A383ED217D4C4239B5933866DD96D3EB2102737DA92F80F6057A4B40DF2F`. Its Authenticode signature was Valid, publisher Pyrsys B.V. The official installer was extracted with `/PORTABLE=1 /CURRENTUSER /VERYSILENT /NOICONS` into `.tools/inno-7.1.0`; it is a local build dependency. The supplied compiler identifies its current mode as non-commercial. Review current licensing before commercial release work.

Sources:

- `installer/CuteCat.iss`: wizard, Windows registration, install/uninstall hooks.
- `installer/SetupSupport.cs`: bounded Restart Manager, certificate ownership, secure directory ACL and owned-startup/shortcut cleanup.
- `installer/Preview.txt`, `Getting-started.txt`: user-facing local-preview guidance.
- `scripts/build-installer.ps1`: payload verification and reproducible packaging steps.
- `tools/CuteCat.SetupArtwork`: exports static wizard art from the production drawing code; no runtime animation sprites.
- `tests/CuteCat.SetupProbe`: native wizard navigation/control checks and PrintWindow renders of only this installer. No computer plugin or desktop screenshot capture.

Generated constants live in ignored `installer/generated`; compiler binaries live in ignored `.tools`. Never commit signing keys. The public certificate and reviewed application payload are the same 0.6.1 inputs documented in the prior handoff.

## Validation and future changes

Current results belong in [build status](build-status.md) and `artifacts/installer-qa`. Required checks are: wizard navigation/cancel, real registration/shortcuts, in-place reinstall with one app entry, closing a running cat, invoking the exact registered uninstall command, removal of owned files/registration/trust, preserved user state, explicit consent for fresh trust, and a final installed/running app.

This milestone verifies the current version's reinstall and the transition from its manual install. A future app/certificate version needs an explicit upgrade test, including compatibility with an adopted legacy directory and certificate ownership rotation. An expired preview must remain uninstallable through its ordinary native uninstaller; it does not require UIAccess to remove the app.

The measured final runtime has UIAccess and an administrator token when launched from the administrative automation environment on this account. That is not evidence of a standard-user launch. The installer uses original-user shell launch on its completion page; verify additional account configurations before making broader privilege claims.

Primary references checked 12 September 2026: [Inno Windows registration](https://jrsoftware.org/ishelp/topic_setup_createuninstallregkey.htm), [directory cleanup semantics](https://jrsoftware.org/ishelp/topic_dirssection.htm), [wizard styling](https://jrsoftware.org/ishelp/topic_setup_wizardstyle.htm), [setup/uninstall events](https://jrsoftware.org/ishelp/topic_scriptevents.htm), and [Restart Manager filter actions](https://learn.microsoft.com/en-us/windows/win32/api/restartmanager/ne-restartmanager-rm_filter_action).
