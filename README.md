# Cute Cat

A free native Windows companion, drawn and animated continuously in C#. No sprite sheets, WebView, game engine or cloud service.

**v0.7.0 preview** — Windows 11 x64.

## Install

Download **CuteCat-0.7.0-Setup.exe** from [GitHub Releases](https://github.com/kaustabhws/cute-cat/releases). The wizard adds Start menu/desktop shortcuts and registers the app in **Settings → Apps → Installed apps**. Uninstall removes the program and owned certificate trust while preserving your settings and focus history.

**Preview signing:** Setup is unsigned. The app uses a local development certificate expiring **12 October 2026**. To render above Windows notifications, the wizard asks to trust that certificate and enable UIAccess, which grants broader access to other apps’ controls. Read the [access details](docs/16-uiaccess-review.md) before installing. This is a preview, not a production-trusted release.

## What it does

- Walks, runs, grooms, meows, plays, and turns with continuous code-driven animation.
- Naps when you are idle, with a breathing nose bubble and floating z symbols; stretches awake when you return.
- Offers a bandana, bow tie, bell collar or ear flower, colours, and activity preferences.
- Matches desktop apps you explicitly select. Choose a reminder or an angry paw that requests a normal window close, always or during focus sessions.
- Leaves save prompts and close refusals to you. Never force-kills a process.
- Dismisses supported Windows notifications after paw contact.
- Provides focus/break timers, local settings, and modern themed controls and pet/tray menus.

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

The preparation step creates a local signing certificate and deletes its private key after signing; it does not add trust. A durable production signing identity is future work. Rebuilding a preview produces a different signing identity.

The core, native adapters and renderer live in `src`; deterministic and native fixture checks live in `tests`. Diagnostic images are outputs, never runtime animation inputs. Build caches, personal configuration, user data, captures and signing keys are excluded from Git.

- [Getting started](docs/getting-started.md)
- [Build status and verification](docs/build-status.md)
- [Focus companion/customization contract](docs/18-focus-companion.md)
- [Installer and uninstall contract](docs/17-windows-installer.md)
- [Agent documentation map](docs/README.md)
- [Research and dated sources](docs/research/03-sources.md)
- [Third-party notices](docs/third-party-notices.md)
