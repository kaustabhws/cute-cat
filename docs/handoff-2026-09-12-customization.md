# Handoff — 0.7.0 focus companion

The user requested app-specific closing/reminders, idle sleep and richer sleep animation, wake/activity behavior, pet accessories, modern settings/tray menus and an updated installer. They then corrected the bandana's beard-like placement, and explicitly requested committing required files to `https://github.com/kaustabhws/cute-cat.git` with the installer in Releases.

## Final implementation

- Core `CompanionBrain` and `AppRulePolicy`; schema 2 with schema-1 backup and safe defaults.
- `DesktopApps` matches selected executable paths and actual foreground window/process identity, resolves a native/accessible caption close control, and requests normal close after painted contact. Close refusals/save prompts are not clicked repeatedly. System/self/dialog windows are excluded.
- Global/per-rule controls, close or reminder, always/focus-only scope, temporary pause, and an open-app/executable picker. Browser detection stays deferred.
- Held sleep, elapsed-idle naps, wake stretch, procedural nose bubble/z symbols, varied activity and anger.
- Bandana, bow tie, bell collar and flower with five colours. Neckwear is below/behind the chin. The bandana's smaller fold is on the shoulder; it does not overlay the face. Checked both directions, front view, grooming and sleep.
- Animated switches, rounded themed scrollbars and a WPF context menu shared by the desktop cat and notification-area icon.
- Stable Inno AppId, 0.7 installer, known-certificate rotation and data-preserving upgrade/uninstall. New app-closing behavior is off with no rules after upgrade.

## Verification

113 core checks and the full 65-check native suite passed. The later live app fixture used the actual Windows foreground window, received normal close after ~2 seconds and ~0.405 px paw-contact error. The refusal fixture remained open with one close request. An earlier no-foreground run used a controlled foreground adapter and is not presented as live proof. An intermediate timing-sensitive recovery run had two failures; the subsequent full suite passed.

Actual WPF control exports and code-generated art were inspected on light/dark backgrounds. The initial bandana was rejected by the user and corrected before packaging. No computer plugin, desktop screenshots, runtime image generation, force kill, URL/title classification or hosted backend was added.

The 0.7 installer upgraded 0.6.1, retained nickname/progress/history, installed the new signed app and retired the old owned certificate. Uninstall removed registration/files/trust and retained schema-2 user data; reinstall restores the final installation. Raw test outputs are local and Git-ignored. See build status for the final installer hash and current release evidence.

## Publishing and security boundaries

The user authorized this repository and installer release. Commit sources, build scripts, tests, docs, licenses, icon and curated code-generated previews. Do not commit artifacts, caches, personal configuration, user state, captures, private signing keys or credentials. The optional Azure configuration document has been made generic; the original private endpoint configuration is retained only in ignored local artifacts.

The installer is a prerelease: unsigned wrapper, user-approved UIAccess and a local certificate expiring 12 October 2026. The new public certificate thumbprint is `2255541E29F4C5912F979C89551B30D994BAC9D3`; the private key was deleted. The old known owned certificate is retired during upgrade. The release notes must disclose this signing/access scope. No durable production signature or accessibility-product eligibility is claimed.

Source entry points: `CompanionBrain.cs`, `StateStore.cs`, `Companion.cs`, `CatRig.cs`, `CatPainter.cs`, `DesktopApps.cs`, `CompanionHost.cs`, `PetMenu.cs`, `MainWindow.xaml.cs`, `App.xaml`, installer scripts and `CertificateHistory.json`. Contract: `18-focus-companion.md`.
