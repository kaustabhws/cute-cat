# Companion extras and shortcut activation — 1.1.0

**Publication update, 13 September 2026:** the user subsequently authorized the source push and 1.1.1 release. Earlier local-only statements below are historical; see [release notes](releases/v1.1.1.md).

The user authorized all six next-version suggestions on 13 September 2026, then added desktop-shortcut activation of the existing instance. Browser URL detection remains deferred. This milestone is prepared and installed locally; publication of 1.1.0 is not part of the current delivery.

## Requirements and acceptance

| ID | Behavior | Acceptance |
| --- | --- | --- |
| EXTRA-01 | Small grace countdown beside the cat, with Allow 5 minutes | Displays the existing `GuardGate` decision, never creates a new delay. Zero grace retains immediate notice/run/paw. The button is bound to exact path, window identity and cancellation epoch. Foreground loss, menu, hiding, policy change or suppression removes it. |
| EXTRA-02 | Troubleshooting and shareable support report | Settings provides real menu opening, app-owned practice and real Windows-toast tests. Explains the distinction. Report is an explicit allowlist with no names, paths, titles, messages, settings contents, history or automatic upload. |
| EXTRA-03 | Saved outfits and procedural patterns | Solid, tabby, tuxedo and calico; editable base/marking colours. Up to 24 named looks retain all slots. Profiles may reference an outfit. Editing a linked look makes an everyday copy and unlinks that profile; deleting an outfit clears references. |
| EXTRA-04 | Expressive petting | Existing nonactivating click/drag detection remains. A click leans the head, closes happy eyes and relaxes ears with continuous rig interpolation. An independently optional, quiet synthesized purr defaults off. Meow remains available. |
| EXTRA-05 | Optional stretch/water reminders | Default off; 15–120 minutes of observed, attended focus time. No accrual while away or across long clock gaps. One due reminder waits for an idle opportunity. It fades after 18 seconds, offers Later or a five-minute break, and never interrupts a paw, menu or drag. |
| EXTRA-06 | Reviewed settings export/import | Versioned JSON, 512 KB limit, strict fields/depth and bounded profiles/rules/outfits. Import is reviewed before replacement. Every app rule starts unchecked; unavailable/system paths cannot be checked. Global guard, notifications, schedules and focused-control avoidance remain off after applying. |
| EXTRA-07 | A desktop/Start shortcut opens the running app | Second process signals the first and exits. Hidden panels show; minimized panels restore their previous normal/maximized state. A duplicate `--tray` startup remains silent. No duplicate companion or routine already-running error. |

## Data contract

State schema **5** adds `Preferences.Outfits`, `ShowGraceCountdown` (true), `Purrs` (false), `BreakCues` (false), `BreakCueMinutes` (45), `PetAppearance.Pattern`/`PatternColor`, and optional `FocusProfile.OutfitId`. Schema-4 input is copied to `.schema4.json` before migration. Existing flags and rules are preserved. Unknown future state stays read-only. Rolling back to 1.0.0 leaves schema-5 state untouched/read-only; the separate schema-4 backup retains the pre-upgrade configuration.

`SessionSnapshot.ReturnToFocus` can contain one normalized, paused focus snapshot. Taking a break preserves its earned time; extending a completed break retains it. Returning resumes only on an explicit action. Break time earns no focus history. Recovery pauses a running break and preserves the return snapshot; malformed/nested return data cannot create focus credit. Ending the session clears it.

The export envelope is `{ "Format": "CuteCat.Settings", "Version": 1, "Settings": ... }`. It contains configuration, including user-selected executable paths. It excludes history, session state, usage totals, temporary exceptions and machine-specific placement. Import preserves the current machine's startup/placement and current session/history/usage. It writes no registry keys. Export is an explicit save-file action; the support report is a different, privacy-minimized artifact.

## Native boundaries

The cue is a small WPF window using `WS_EX_NOACTIVATE` and `MA_NOACTIVATE`. It has no global input hook and does not take foreground ownership from the guarded app. Its mouse buttons are an optional convenience; keyboard users can use the existing app-rule exception controls in the main panel. Its presentation follows the cat within the selected work area; it has no independent intervention timer. The existing `PetMenu` activation/capture/dismissal implementation is preserved.

Shortcut activation uses the existing per-data-directory mutex and a message-only HWND. A registered message accepts only zero payload and requests showing the existing main window. The sender grants foreground permission to that process only. The receiver permits that single message across integrity levels for the previously approved UIAccess build; it exposes no arbitrary file, executable, preferences or process command. Retry is bounded to five seconds to cover simultaneous launches. Windows may still restrict foreground activation, especially in a disconnected RDP desktop; the panel is shown regardless.

Patterns clip to the deforming head/body paths and follow orientation. They never replace runtime vectors with raster frames or alter the paw endpoint. Accessory layer order and the shoulder bandana are unchanged. Diagnostic PNG/WebP exports are review artifacts only.

## Validation and remaining desktop checks

Run the deterministic suite plus `--extras-checks --app-fixture <test exe>`, the existing full native suite, `--menu-opening-checks`, `--menu-checks --menu-events-only`, and `--appearance-checks`. Extras uses a real separate app-window fixture with an explicitly injected foreground identity where needed; button events test application wiring, not real mouse capture. Shortcut checks launch real second processes against isolated state.

Review UI in both themes/minimum size, coat patterns across poses/yaws, original animation seams, and the nonactivating cue. Record executed counts and paths in build status/handoff. Active desktop tests still include physical outside clicks, click-through around cues, live Windows banner overlap, mixed-DPI monitors, keyboard/screen-reader behavior and audio volume. Never relabel RDP-simulated foreground tests as live desktop proof.
