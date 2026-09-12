# Cute Cat

A free native Windows companion, drawn and animated continuously in C#. No sprite sheets, WebView, game engine or cloud service.

**v0.8.0 preview** — Windows 11 x64.

## Install

Download **CuteCat-0.8.0-Setup.exe** from [GitHub Releases](https://github.com/kaustabhws/cute-cat/releases). The wizard adds Start menu/desktop shortcuts and registers the app in **Settings → Apps → Installed apps**. Uninstall removes the program and owned certificate trust while preserving your settings and focus history.

**Preview signing:** The app, installer and uninstaller are signed and timestamped with a reusable protected preview identity. This is not public publisher verification. To render above Windows notifications, the wizard asks to trust its certificate and enable UIAccess, which grants broader access to other apps’ controls. Read the [access details](docs/16-uiaccess-review.md) and [signing/update contract](docs/19-profiles-and-reliability.md) before installing.

## What it does

- Walks, runs, grooms, meows, plays, and turns with continuous code-driven animation.
- Naps when you are idle, with a breathing nose bubble and floating z symbols; stretches awake when you return.
- Offers a bandana, bow tie, bell collar or ear flower, colours, and activity preferences.
- Matches desktop apps you explicitly select. Choose a reminder or an angry paw that requests a normal window close, always or during focus sessions.
- Leaves save prompts and close refusals to you. Never force-kills a process.
- Dismisses supported Windows notifications after paw contact.
- Provides focus/break timers, local settings, and modern themed controls and pet/tray menus.
- Switches between Work, Study and Break profiles, with independent rules, activity and weekly schedules.
- Offers per-app temporary exceptions, close grace periods and daily allowances.
- Remembers a resting spot per monitor, settles while you work, and optionally moves away from a focused control.
- Adds ear twitches, a look before running, waking yawns and little completion celebrations.
- Verifies updates from GitHub and retains compatible signed installers for repair/rollback. This release establishes the recovery baseline.

App guard starts off with no apps selected. Open **App guard → Add an app**, configure the rule, then enable it. Browser URL/tab detection is deferred. Unknown/system windows and unsupported close layouts are left alone.

![Accessory placement across poses](docs/images/accessory-placement.png)

## Develop

Requires Windows, PowerShell 7 and the .NET SDK in `global.json`.

```powershell
./scripts/build.ps1 -Publish
./scripts/get-inno-compiler.ps1
./scripts/prepare-uiaccess-review.ps1
./scripts/build-installer.ps1
```

The preparation step reuses a nonexportable signing key in the Windows user's CNG key store; it does not add trust or export the key. Packaging also requires Windows SDK SignTool and network access for timestamping. Publicly verified signing remains future work. GitHub Actions checks Windows Server 2022/2025; live Windows 11/RDP/multi-monitor tests are tracked separately.

The core, native adapters and renderer live in `src`; deterministic and native fixture checks live in `tests`. Diagnostic images are outputs, never runtime animation inputs. Build caches, personal configuration, user data, captures and signing keys are excluded from Git.

- [Getting started](docs/getting-started.md)
- [Build status and verification](docs/build-status.md)
- [Focus companion/customization contract](docs/18-focus-companion.md)
- [Profiles, privacy, updates and recovery](docs/19-profiles-and-reliability.md)
- [Installer and uninstall contract](docs/17-windows-installer.md)
- [Agent documentation map](docs/README.md)
- [Research and dated sources](docs/research/03-sources.md)
- [Third-party notices](docs/third-party-notices.md)
