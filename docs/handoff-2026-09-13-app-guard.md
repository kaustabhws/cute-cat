# Handoff — App guard repair 1.1.2

**Publication update, 14 September 2026:** the user authorized the source push and installer release. The local-only statements below record the earlier delivery status. See [release notes](releases/v1.1.2.md).

## Delivered locally

The user reported selected apps not closing, named Android Studio and clarified that other apps were affected. Version 1.1.2 is installed locally; GitHub remains at 1.1.1. No push/tag/release was performed for this repair.

Changes:

- Notification discovery no longer holds the app-guard scan loop. One pending shell query is retained; cancelled-epoch results are discarded. A regression with a deliberately blocked query failed both standard/custom app closing before the fix and passed afterward.
- Custom frames with empty legacy caption rectangles can resolve through bounded Windows HTCLOSE queries. Read-only probes found the actual Android Studio and Codex close regions through this path. No app-name-specific coordinates or input injection were added.
- Unowned, minimizable, sole-main-window dialogs can be selected; the blanket dialog-class rejection no longer excludes those desktop applications. Owned and ordinary message/save-style dialogs remain excluded.
- Status text distinguishes no selected rules, no active foreground window and a busy notification-control query.

App identity/path, foreground, enabled rule, cancellation epoch, geometry and painted paw contact remain required. Closing is a normal SC_CLOSE/accessibility request; save/exit prompts and vetoes remain under user control.

## Files

Runtime: `DesktopApps.cs`, `CompanionHost.cs`, `SupportDiagnostics.cs`, core `NativeCaptionLocator.cs`. Tests: `QualityChecks.AppGuard.cs`, `CaptionChecks.cs`, `AppControlProbe.cs`, custom-frame `AppWindowFixture`, native `DialogWindowFixture`, entry points and the CI regression step. Contracts/defaults/decisions/source references are updated in docs, with details in [the repair contract](25-app-guard-repair.md).

## Checks actually performed

| Check | Result / evidence |
| --- | --- |
| Release build and core | Zero warnings/errors; 226 passed (`artifacts/v112-build-final.log`) |
| UI-based standard/custom frame flow with real foreground and blocked notification query | Passed without foreground injection (`artifacts/app-guard-blocked-notifications-after`) |
| Expanded standard/custom/dialog matrix, refusal, disabled close, standalone prompt, stale result | 26 passed in explicit controlled mode (`artifacts/v112-guard-matrix-controlled`) |
| Installed signed App guard matrix | 26 passed (`artifacts/v112-installed-app-guard`) |
| Installed full native regression | 85 passed (`artifacts/v112-installed-full`) |
| Installed cat/tray opening | 10 passed (`artifacts/v112-installed-opening`) |
| Installed UI responsiveness/resize | 13 passed (`artifacts/v112-installed-responsiveness`) |
| Extras / menu lifecycle on source build | 25 / 19 passed (`artifacts/v112-extras`, `artifacts/v112-lifecycle`) |
| Installer/signature/recovery/preservation | 14 passed (`artifacts/install-v112-local`) |

The first full source run missed two timing thresholds while all functional checks passed; the installed full rerun passed including cadence. A later real-foreground matrix run had no foreground window in the RDP session. The expanded run therefore explicitly uses `--guard-events-only`, reports the override, and must not be called a physical-input test. The earlier successful real-foreground standard/custom test is recorded separately. Read-only user-app detection is not a claim of closing the user's live Android Studio project.

## Installer

`dist/installer/CuteCat-1.1.2-Setup.exe`, 58,879,040 bytes. SHA-256:

`0240106ADB920618B811DA449403AF26E69E7B46CFCCF347C186933A8338FB7B`

Installed at `C:\Program Files\Cute Cat`. The existing protected preview signer is reused; no new signing identity or private-key export. Existing settings/history and startup configuration are preserved.

## Next acceptance

The user should retry their selected apps with App guard enabled in the active profile. Real save/exit dialogs must stay untouched. The native foreground check may be unavailable while the RDP desktop is disconnected/inactive. Unsupported frames and off-monitor close targets remain limited by the documented scope. Do not publish this fix until asked.
