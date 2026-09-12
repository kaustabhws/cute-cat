# Agent playbook

## Task entry points

| Task | Read |
| --- | --- |
| Native UI | Requirements, experience/UI, tokens, Windows architecture |
| Companion/rendering | Code-cat revision, art direction, production, code-cat-rig contract, architecture, display tests |
| Browser protection | Focus engine, data/privacy, browser protocol, test plan |
| Artwork | User-selected medium, character bible, prompt pack, Azure setup, provenance |
| Storage/recovery | Focus engine, data/privacy, deterministic tests |
| Release | Build status, test plan, decisions, current packaging scripts |

Before coding, inspect the current implementation and build status. The planning package includes future release requirements; do not assume every diagram or interface already exists. Preserve user data and existing edits.

## Bounded task prompts

**Native surface:** Implement or improve the small Win32 cat surface. Preserve nonactivation and per-pixel click-through. Prove interaction on actual Windows; report display configurations tested. Do not add global keyboard hooks.

**Browser adapter:** Implement one explicitly named feed adapter. Add positive and negative URL fixtures, SPA/visibility handling, stale-response cancellation, and disconnect cleanup. No arbitrary page-content classification or broad permission expansion.

**Character art:** The current cat is entirely code-drawn. Edit the rig and cubic paths; keep one original silhouette, smooth transitions, clear grooming and sleep poses, measured paw geometry and actual-size light/dark previews. Azure remains authorized for optional future art, but do not reintroduce generated animation frames contrary to the latest request.

**UI refinement:** Improve the selected screen using the token system and the actual cat art. Verify keyboard flow, long text, both themes, and small-window layout. Do not rebuild the native UI in a webview.

## Definition of done

The requested behavior works, required tests pass, the actual visual result has been inspected where relevant, recovery paths remain usable, and docs reflect changed scope/contracts. A test plan without executing it is not verification. A successful compile is not visual QA. A local alpha is not a signed public release.

## Handoff format

Record: task/requirement IDs; final behavior; files changed; commands/tests actually run; visual evidence; known limitations; next dependency-ready task; contract/decision changes. Keep credentials, raw browsing data, and personal task labels out of handoffs.

## Common mistakes to avoid

Do not use title matching or injected Ctrl+W to close a “distracting” tab. Do not fake bridge connectivity. Do not ship generated sprites with baked backgrounds. Do not mirror asymmetrical art without correcting markings. Do not let a delayed animation act on an old page. Do not silently enable all-site permissions, startup, or cloud calls. Do not overwrite actual build status with aspirational marketing copy.
