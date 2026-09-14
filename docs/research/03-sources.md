# Source register

App guard API check, 13 September 2026: [WM_NCHITTEST](https://learn.microsoft.com/en-us/windows/win32/inputdev/wm-nchittest) defines HTCLOSE (20); [DWM window attributes](https://learn.microsoft.com/en-us/windows/win32/api/dwmapi/ne-dwmapi-dwmwindowattribute) provide caption geometry; [WM_SYSCOMMAND](https://learn.microsoft.com/en-us/windows/win32/menurc/wm-syscommand) defines normal SC_CLOSE requests; [GetGUIThreadInfo](https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-getguithreadinfo) was consulted for foreground diagnostics. No cached or guessed foreground fallback was introduced.

## 0.6.0 notification investigation — 12 September 2026

- [Microsoft: invoke a control using UI Automation](https://learn.microsoft.com/en-us/dotnet/framework/ui-automation/invoke-a-control-using-ui-automation): locate a scoped control, obtain InvokePattern, and invoke its action. The sample cautions against traversing the entire desktop subtree; our fallback examines only direct roots filtered to verified shell PIDs.
- [Microsoft: EnumWindows](https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-enumwindows): on Windows 8+, enumeration covers top-level desktop-app windows; Win32 enumeration alone is not treated as complete immersive-shell discovery.
- [Microsoft: WPF app notifications](https://learn.microsoft.com/en-us/windows/apps/develop/notifications/app-notifications/app-notifications-dotnet?pivots=wpf): documents real local notifications and explicitly states that elevated apps are unsupported. The current recommended examples use Windows App SDK; this build uses the pinned WinRT-compatible Community Toolkit helper for the same OS toast platform.
- [Microsoft: app notification overview](https://learn.microsoft.com/en-us/windows/apps/develop/notifications/app-notifications/): differentiates notifications, content and listener access. The listener is not used here.
- [Community Toolkit Notifications 7.1.2](https://www.nuget.org/packages/CommunityToolkit.WinUI.Notifications/7.1.2): package metadata/license inspected; Windows desktop/WPF support, MIT copyright .NET Foundation and contributors. Pinned dependency and license included.

The Microsoft pages were read during the notification investigation before the user stopped computer use. The resumed fix used only code, command-line builds and native test programs, without the computer plugin. No source claims that Windows shell AutomationIds are a stable public API. Shell-layout compatibility remains bounded and fail-closed.

## 12 September 2026 current recheck

Opened and read the primary [Workcat homepage](https://workcat.app/en/) and [changelog](https://workcat.app/en/changelog/), [DesktopCat](https://desktopcat.com/), [BongoCat](https://bongocat.pet/), [Catime](https://cati.me/), [Cold Turkey](https://getcoldturkey.com/), and [Microsoft's notification listener guide](https://learn.microsoft.com/en-us/windows/apps/develop/notifications/app-notifications/notification-listener) in the browser during this task.

| Source | Current observed statement | Build implication |
| --- | --- | --- |
| Workcat | Mac positioning; standalone file still unavailable; drifty Mac path; Windows waitlist; pricing undecided | Inspiration is the companion/gesture loop, not a shipped Windows implementation or a license to copy artwork |
| DesktopCat | Free base cat for macOS 13+ and Windows 10/11, 28 states, paid custom character option at US$49 | A Windows desktop pet is not itself novel; prioritize original coherent motion and manners |
| BongoCat | Native C11/OpenGL, input-reactive companion; source/model licensing described separately | Native motion can be lightweight; no code/models or global input-monitoring behavior were imported |
| Catime | Native C/Win32 timer, free-download positioning, vendor size/CPU claims | Keep the utility compact; vendor resource claims are not independent measurements |
| Cold Turkey | Windows/macOS desktop app plus browser extensions, block scheduling/locking, one-time-purchase positioning | Browser protection needs a separate reliable adapter; do not infer browser URLs from window titles |
| Microsoft | Notification listener requires capability plus explicit access; remove one ID; no close-button screen rectangle | The physical paw gesture needs a narrowly scoped geometry adapter; no broad history permission is silently requested |

No competitor binary was installed, benchmarked or reverse engineered. No competitor art, code, model, font or sound was copied. Earlier dated wider research below is retained as historical evidence, not rechecked pricing or current support guarantees. The new native app's shell test yielded NoBanner; that is a compatibility limitation, not evidence that Windows notifications cannot be dismissed in every environment.

## 11 September 2026 recheck

Workcat's [English homepage](https://workcat.app/en/), [changelog](https://workcat.app/en/changelog/), [DesktopCat](https://desktopcat.com/), and [Cold Turkey](https://getcoldturkey.com/) were opened again in this session. Workcat still stated that the standalone app has not shipped, the cat is available in drifty for Mac, and the Windows link is a waitlist. DesktopCat still advertised Windows 10/11 support, a free base cat, and 28 animation states. Cold Turkey still described desktop plus extension setup and paid one-time licensing. These are vendor descriptions; no competitor was installed. The original broader 10 September source register below is retained as its own dated record.

[Microsoft notification listener documentation](https://learn.microsoft.com/en-us/windows/apps/develop/notifications/app-notifications/notification-listener) was opened on 11 September. It establishes the manifest capability, explicit user permission, revocation checks, and removal by notification ID. See [Windows notification feasibility](04-windows-notifications.md) for how this informed the new feature. A guessed older documentation route returned 404 and was not used as evidence.

## Original 10 September register

All sources below were opened and read on **10 September 2026 (UTC)**. Product sources are official vendor pages or official Steam listings. Technical sources are Microsoft or Google documentation. Product behavior is vendor-described unless explicitly identified as a browser-page observation. No competitor binary was installed, benchmarked, reverse engineered, or used to test intervention.

The Workcat and competitor research was completed before the build recommendation was written. Google search was unavailable behind an unusual-traffic page; Bing was used for discovery, and candidate claims were checked on the official sites. Search snippets are not the evidence for the comparison table.

## Workcat and its parent product

| ID | Source | What it establishes | Qualification |
| --- | --- | --- | --- |
| W1 | [Workcat English homepage](https://workcat.app/en/) | Product promise, six coats, planned account-free setup, Mac Accessibility access, drifty path, Windows waitlist | Page and visual layout inspected; desktop application not tested |
| W2 | [Workcat changelog](https://workcat.app/en/changelog/) | Standalone app not shipped; August milestones; release date, price, non-Mac support undecided | Latest visible entries: 28 August 2026 |
| W3 | [Workcat privacy policy](https://workcat.app/en/privacy/) | Local detection design; no screen transmission; site/email/server/font disclosures | Updated 28 August 2026; explicitly a pre-release app design position |
| W4 | [drifty homepage](https://drifty.so/) | Activity classification, local/cloud/BYOK options, optional cat, Extreme Nudge Beta, Mac requirements | Feature descriptions and displayed pricing are vendor claims, not audits |

## Comparable products

| ID | Source | Evidence used |
| --- | --- | --- |
| C1 | [DesktopCat](https://desktopcat.com/) | Windows/macOS availability, free Mochi, 28 states, interactions, US$49 custom character pack, local-input claims |
| C2 | [BongoCat by vladelaina](https://bongocat.pet/) | Native C11/OpenGL claim, input responses, model library, free/open-source positioning, separately described source/model licenses |
| C3 | [Catime](https://cati.me/) | Native Win32 timer, Pomodoro branding, transparent overlay, free download, size and idle-CPU claims |
| C4 | [Spirit City: Lofi Sessions on Steam](https://store.steampowered.com/app/2113850/Spirit_City_Lofi_Sessions/) | Focus tools, spirits, room/avatar customization, XP, music, DLC, system requirements, India-region price |
| C5 | [Virtual Cottage on Steam](https://store.steampowered.com/app/1369320/Virtual_Cottage/) | Activity commitment, timer behavior, lofi/rain, free base and music DLC |
| C6 | [Cold Turkey](https://getcoldturkey.com/) | Desktop app plus extension setup, blocking/schedules/locks, local-state and one-time-purchase positioning |
| C7 | [Cold Turkey system requirements](https://getcoldturkey.com/support/system-requirements/) | Explicit Windows and supported-browser lists |
| C8 | [Freedom](https://freedom.to/) | Windows availability, app/site/internet blocking, schedules, locked mode, cross-device positioning |
| C9 | [Freedom Premium plans](https://freedom.to/premium) | Monthly, yearly, and Forever plan structure; no stable global numeric comparison inferred |
| C10 | [Forest](https://www.forestapp.cc/) | Focus/tree loop, loss mechanic, mobile downloads, browser-extension links, free-start positioning |

Do not treat a competitor's testimonial, download count, stars, resource budget, or productivity claim as an independent study. We intentionally make no market-size or clinical-effectiveness claim. Software and art license terms must be read at the actual package/version before reuse; a homepage summary is insufficient.

## Engineering references

| ID | Source | Design implication |
| --- | --- | --- |
| T1 | [WPF overview](https://learn.microsoft.com/en-us/dotnet/desktop/wpf/overview/) | WPF is a Windows-only .NET UI framework with XAML, styles, controls, graphics, and animation; it can support original UI styling |
| T2 | [Choose a Windows development path](https://learn.microsoft.com/en-us/windows/apps/get-started/) | Microsoft recommends WinUI 3 for new native apps. Our WPF choice is a project-specific tradeoff, not Microsoft's default recommendation |
| T3 | [.NET support policy](https://dotnet.microsoft.com/en-us/platform/support/policy/dotnet-core) | .NET 10 is LTS; observed end of support 14 November 2028. Pin the current supported SDK/patch at implementation time |
| T4 | [Win32 window features: layered windows](https://learn.microsoft.com/en-us/windows/win32/winmsg/window-features) | `UpdateLayeredWindow` supports per-pixel alpha; zero-alpha areas pass mouse messages; layered `WS_EX_TRANSPARENT` passes mouse events through; do not mix opacity APIs casually |
| T5 | [Extended window styles](https://learn.microsoft.com/en-us/windows/win32/winmsg/extended-window-styles) | `WS_EX_NOACTIVATE`, `WS_EX_TOOLWINDOW`, and topmost behavior; ordinary `WS_EX_TRANSPARENT` description concerns painting order |
| T6 | [WM_NCHITTEST](https://learn.microsoft.com/en-us/windows/win32/inputdev/wm-nchittest) | `HTTRANSPARENT` forwards hit tests within the same thread, not a general cross-process solution; signed coordinates matter on multiple monitors |
| T7 | [High-DPI desktop development](https://learn.microsoft.com/en-us/windows/win32/hidpi/high-dpi-desktop-application-development-on-windows) | Per-monitor V2 awareness, coordinate spaces, `WM_DPICHANGED`, mixed-DPI testing. Older framework support tables are historical, not current WPF benchmarks |
| T8 | [SetWinEventHook](https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-setwineventhook) | Out-of-context events avoid injected hook DLLs; owning thread needs a message loop; callback lifetime and reentrancy must be handled |
| T9 | [SendInput](https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-sendinput) | Input injection has integrity/UIPI limits and keyboard-state hazards; generic shortcut injection is inappropriate for targeting browser content |
| T10 | [Chrome native messaging](https://developer.chrome.com/docs/extensions/develop/concepts/native-messaging) | Native-host registration, explicit allowed extension origins, length-prefixed UTF-8 JSON over stdio, process lifetime, service-worker parent handle can be zero |
| T11 | [Chrome tabs API](https://developer.chrome.com/docs/extensions/reference/api/tabs) | Sensitive URL/title access requires appropriate permissions; host permissions can cover selected sites; `activeTab` only provides temporary access after a user action |
| T12 | [Chrome webNavigation API](https://developer.chrome.com/docs/extensions/reference/api/webNavigation) | SPA history updates, main-frame/document identity, navigation lifecycle; `webNavigation` is a separately declared permission |
| T13 | [Extension service-worker lifecycle](https://developer.chrome.com/docs/extensions/develop/concepts/service-workers/lifecycle) | Global state can disappear; native messaging keeps workers alive while connected but disconnect/crash recovery is necessary |
| T14 | [Edge native messaging](https://learn.microsoft.com/en-us/microsoft-edge/extensions/developer-guide/native-messaging?tabs=v3%2Cwindows) | Edge host/extension setup is separate from Chrome; catalog extension IDs and allowed origins must be configured correctly |

## Evidence that still needs collection

Installer sources checked 12 September 2026:

- **T17:** [Inno Setup 7.1.0 official release](https://github.com/jrsoftware/issrc/releases/tag/is-7_1_0), [license](https://github.com/jrsoftware/issrc/blob/is-7_1_0/license.txt), [wizard styles](https://jrsoftware.org/ishelp/topic_setup_wizardstyle.htm), [Windows uninstall registration](https://jrsoftware.org/ishelp/topic_setup_createuninstallregkey.htm), [directory cleanup](https://jrsoftware.org/ishelp/topic_dirssection.htm), [setup/uninstall events](https://jrsoftware.org/ishelp/topic_scriptevents.htm). The exact compiler hash and verified signer are recorded in the installer contract; downloaded compiler identifies non-commercial mode.
- **T18:** [Microsoft Restart Manager filters](https://learn.microsoft.com/en-us/windows/win32/api/restartmanager/ne-restartmanager-rm_filter_action) and [RmShutdown](https://learn.microsoft.com/en-us/windows/win32/api/restartmanager/nf-restartmanager-rmshutdown) — exclude other processes and request a graceful shutdown without the force flag when updating/removing the owned app.

Additional primary sources checked 12 September 2026:

- **T15:** [UI Automation security overview](https://learn.microsoft.com/en-us/windows/win32/winauto/uiauto-securityoverview) — UIAccess requires signing/trust and secure installation, permits interaction across ordinary UI integrity restrictions, and is intended for assistive technologies. Microsoft explicitly discourages ordinary apps from using it merely to render above Windows UI.
- **T16:** [Application manifests](https://learn.microsoft.com/en-us/windows/win32/sbscs/application-manifests) — requestedExecutionLevel and uiAccess declarations; a manifest request alone is not a runtime permission grant.

- Actual Windows prototype recordings: click-through to other processes, uninterrupted typing, monitor removal, mixed scaling, Explorer restart.
- Browser fixture results for Shorts, full-site rules, and later Reels/TikTok adapters, including logged-in/out and SPA navigation variants.
- No historical Workcat release binary or private roadmap was obtained. Its unpublished implementation remains unknown.
- Art production trials: reference consistency, alpha edges, frame-to-frame stability, actual sprite-memory use.
- User interviews, willingness-to-pay research, one-week retention/annoyance feedback.
- Current installer signing costs, extension store distribution requirements, name/trademark availability, and exact package licenses before release.
