# Current build status

## 0.9.1 preview — publication approved

Removed the pointer-click outline from the top navigation while keeping its selected pill and a keyboard-only underline. All text inputs now have rounded corners, consistent padding and themed focus/caret/selection. The small minutes field was visually checked with `180` to avoid clipped text. Build passed without warnings; the existing 12 appearance checks passed and light/dark focused-navigation/input exports were inspected in `artifacts/controls-v091-final`. Core, state and cat behavior are unchanged from 0.9.0. The user explicitly approved the GitHub push and installer release on 13 September.

Installed locally as 0.9.1; all 14 installer/signature/preservation checks passed in `artifacts/install-v091-local`. Installer size: 58,824,680 bytes; SHA-256 `C397C2FFFC1CE74F69E8453CE0E6591F37C53F35B7567739B121815A1277364E`. Existing settings and history were preserved. This same tested installer is the approved release artifact; signing remains the protected preview identity, not a publicly verified publisher.

## 0.9.0 local appearance preview — not published

During the 0.9.0 stage, the user paused publication pending PC testing. Implemented real coat colours/custom hex, independent hat/neckwear/collar slots in a tabbed wardrobe, the charcoal/periwinkle UI palette, and configurable native Windows title-bar colours. See [current contract](20-appearance-and-theme.md). The later 0.9.1 publication approval is recorded above.

Verified locally: zero-warning build, **167 core checks**, **79 native regression checks**, and **12 appearance/caption checks**, all passed. Native caption pixels were inspected in light and dark mode, as were wardrobe controls and layered accessories across poses. Evidence: `artifacts/appearance-v090-final` and `artifacts/qa-v090-local`. State schema is 4; schema-3 state gets a preserved backup. No new permission or monitoring scope was added. User visual acceptance is pending.

Installed locally through `CuteCat-0.9.0-Setup.exe`: **14 installation/signature/preservation checks passed** in `artifacts/install-v090-local`. The existing protected preview signing identity was reused; no new trust acknowledgment was needed. The 0.8 and 0.9 installers are retained in Recovery. Local installer: 58,833,968 bytes, SHA-256 `E1C23733B95DF25C4D697C84199E3AEDD7A0304231141F74F70D1F01BBAAE5B1`. No GitHub push, tag or release was performed for this work.

The sections below describe previously published 0.8/0.7 versions and their historical release checks.

## 0.8.0 preview — profiles and reliability

Implemented Work/Study/Break profiles and weekly schedules, per-app exceptions/grace/daily allowances, local bounded usage totals, per-monitor resting spots, settling during work, optional focused-control avoidance, procedural ear twitches/notice/yawn/celebration, and profile switching from the themed pet/tray submenu. Browser URL/tab work remains deferred by user instruction.

The app now offers optional GitHub update checking, same-publisher Authenticode plus SHA-256 verification, user-initiated setup, and protected cached installers for repair/rollback. The app/helper/installer/uninstaller use the same timestamped, nonexportable preview signing identity. This release establishes the recovery baseline; the prior unsigned installer is excluded.

Executed against the 0.8 implementation:

| Check | Result |
| --- | --- |
| Release build | Zero warnings/errors |
| Core checks | 160 passed, 0 failed |
| Native checks | 79 passed, 0 failed (`qa-v080-release`) |
| App policies | Allowance, fresh grace, exception during approach, Break cancellation, resumption and independent profile rules passed |
| Controlled native app close/refusal | Real foreground identity in both fixtures; normal close and one-request refusal passed |
| Contact | ~2.7 seconds including notice/turn; ~0.405 physical pixel error |
| Cadence sample | Walk/run mean ~16.66 ms; p95 ~17.03 ms in this RDP session |
| UI/art | Light/dark native control exports and procedural personality/accessory artwork inspected; profile submenu opens in both themes |
| Protected signing identity | CNG export policy `None`; SHA-256/RFC 3161 timestamps required |
| Installer / update trust | 19 package checks passed: registered installation, valid app/helper/setup/uninstaller signatures, identical protected recovery copy; altered, wrong-version, unsigned and other-publisher installers rejected |
| Uninstall / reinstall | Both completed; program directory and registration removed, exact user state retained, reinstall restored a verified recovery copy |
| Cached installer repair | Same-version repair launched directly from protected Recovery completed successfully |

An early installer check caught padded Inno metadata and a misleading zero exit after a post-install exception. The comparison now trims resource padding, recovery verification precedes file replacement, and unfinished setup returns a nonzero custom exit. Package verification also checks the actual installed state and logs, not only the exit code.

The first hosted CI run passed build/core/allowance tests but correctly declined a fixture close button placed beyond its smaller screen. The fixture now fits the current working area instead of assuming a wide desktop. This was a fixture placement defect; off-monitor targeting remains refused by the app.

Release installer: `CuteCat-0.8.0-Setup.exe`, **58,825,800 bytes**. SHA-256: `FDA435BC101B666BCE8E6923D796145DCD4D671DD988F321AF3B017214BB8B51`.

Preview certificate: `93ECC442E6EC72D1238D6BCA2E8F80D9B01D88EE`, expires **12 September 2029 at 22:45:49 UTC**. Its private key stays in this Windows user's CNG store and is not shipped or committed. This is preview trust, not public publisher verification or SmartScreen reputation.

Current contracts: [profiles/reliability](19-profiles-and-reliability.md), rig contract version 5, state schema 3. Live Windows 11 notification smoke, real editor avoidance, mixed-DPI/hotplug/RDP reconnect, standard-user UIAccess and future-version rollback remain desktop matrix checks. Existing 0.6/0.7 toast evidence is historical, not a new 0.8 live-toast test. GitHub Actions covers controlled Windows Server 2022/2025 scenarios; it does not prove the Windows 11 desktop matrix.

## Historical 0.7 evidence

## 0.7.0 preview — focus companion and customization

Implemented app-specific normal-close/reminder rules, angry paw contact, elapsed-idle naps/wake, accessories and colour/activity preferences, modern switches/scrollbars, and one themed menu shared by the pet and tray icon. The user specifically corrected the bandana placement; neckwear now sits behind the chin with a small shoulder fold, and placements were reviewed across standing, front/left, grooming and sleep poses.

The native installer upgrades the previous version, preserves user state, rotates the known owned preview certificate, remains registered in Windows Installed apps, and supports uninstall/reinstall. Source and installer publication to `kaustabhws/cute-cat` was explicitly requested.

## Executed checks

| Check | Result |
| --- | --- |
| Release compilation | Passed without warnings/errors |
| Core checks | 113 passed, 0 failed |
| Native suite | 65 passed, 0 failed in `qa-v070-release` |
| Live foreground app fixture | Normal close, angry pose and precise caption contact passed without injected foreground identity |
| Live refusal/save fixture | Window remained open; exactly one close request |
| Live contact timing/error | About 2.0 s from approach start; ~0.405 physical pixels |
| Idle/wake and appearance | Native transitions and appearance propagation passed |
| Visual review | Light/dark control exports, themed menu, accessories in five poses; bandana corrected after user feedback |
| Upgrade from 0.6.1 | Exit 0; valid new app signature, old owned certificate retired, nickname/progress/history preserved |
| 0.7 uninstall | Exit 0; registration, program directory and owned certificate removed; schema-2 data preserved |
| Safe defaults | App guard remains off and rule list empty after migration |
| Final reinstall | Performed after the removal check; release artifact remains the tested 0.7 installer |

An earlier native run missed two timing-sensitive recovery assertions; the subsequent complete suite passed. An initial app fixture ran with a supplied foreground identity while Windows reported no foreground window. That is separate from the later live foreground run, which used the actual Windows foreground window. No user apps were selected or closed for testing.

Evidence remains local under `artifacts/qa-v070-release`, `artifacts/qa-v070-live`, `artifacts/accessory-placement-v070` and `artifacts/package-v070`. These raw diagnostics and private configuration are excluded from Git. Curated code-generated artwork is in `docs/images`. See [handoff](handoff-2026-09-12-customization.md).

## Delivery and limits

Installer: `CuteCat-0.7.0-Setup.exe`, 58,788,264 bytes.

SHA-256: `6FE3A970D60711F50930B4AF242CAB8E9BB2C6F82ED58CFC528302281847B75D`.

Installation remains `C:/Program Files/Cute Cat`. The signed app uses preview certificate `2255541E29F4C5912F979C89551B30D994BAC9D3`, expiring **12 October 2026**. Its private key was deleted. The setup/uninstaller wrappers remain unsigned; public distribution is a clearly labeled prerelease, not a production signing claim. See [UIAccess context](16-uiaccess-review.md).

Scope is supported desktop windows and close controls on the cat's selected monitor. System windows are excluded. Save prompts/refusals remain open. Browser URLs/tabs, broad multi-monitor/account coverage, durable signing and an updater remain future work. The app guard is voluntary and can be paused; it is not an unbreakable security boundary.

The signed UIAccess app can inherit administrator context when launched from this administrative environment. The installer uses original-user shell launch; a broader standard-user account matrix is not claimed tested.
