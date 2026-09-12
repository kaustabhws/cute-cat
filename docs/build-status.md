# Current build status

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
