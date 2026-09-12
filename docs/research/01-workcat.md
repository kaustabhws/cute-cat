# Workcat: primary-source research

Observed 10 September 2026; homepage and changelog rechecked 12 September with the same standalone/Mac/waitlist findings. Sources [W1–W4 and current recheck](03-sources.md). This is a public-site review. No Workcat/drifty binary was installed, and detection accuracy, memory use, permissions, and real operating-system intervention were not independently tested. The new app's art direction is now original code-drawn geometry; older hand-painted recommendations below are superseded.

## What the product is

Workcat describes a Mac desktop cat that walks around the screen, recognizes predefined distracting content such as Shorts, Reels, and TikTok, then approaches and closes it with a paw. The site emphasizes one visible action instead of a large productivity dashboard. It says the cat stays near the bottom of the screen and avoids covering the working window. [W1]

The central product loop is **ambient companion → recognized distraction → physical-looking intervention → return to work**. The cat makes the intervention understandable and memorable. This is a product interpretation, not proof that the behavior improves focus.

## Actual availability

| Question | Finding | Evidence and qualification |
| --- | --- | --- |
| Standalone download? | Not yet | Homepage and changelog explicitly say there is no standalone Workcat file. [W1, W2] |
| Can someone use this cat now? | Through drifty on Mac | Install drifty and enable its desktop cat in settings. This does not mean drifty and planned Workcat have identical features. [W1, W4] |
| Windows app? | No released Workcat Windows app established | On the inspected Windows browser, the CTA leads to drifty's Windows waitlist. The Workcat changelog still lists non-Mac support as undecided. A waitlist is evidence of interest, not a release commitment. [W1, W2] |
| Price? | Undecided | The changelog explicitly leaves price and paid features open. Do not label Workcat free. [W2] |
| Account/model setup? | Planned standalone app requires neither | Homepage promises no login, account, or model download before the cat appears. This is a future standalone design position. [W1] |
| Permissions? | Mac Accessibility permission | The site says it is needed to recognize and close windows. No exact API or browser support matrix is disclosed. [W1] |
| Distribution? | Planned signed, notarized direct Mac download | Stated in the 17 August changelog. The standalone file is still absent. [W2] |

The latest visible changelog entries were dated 28 August 2026. They describe email handoff for mobile visitors and platform-specific download/waitlist CTAs. Earlier August entries establish the cat interaction, same-art browser demo, six coats, and availability through drifty. These are mostly website and positioning milestones, not standalone binary releases. [W2]

## Character and presentation

The inspected English site used a restrained dark background, cream display typography, blue accents, fine dividers, and generous spacing. The hierarchy puts the short product promise before installation details. Its interactive cat area offers six coats: Ivory, Charcoal, Grey, Apricot, Sage, and Plum. The text identifies the drawing and gait as the same ones used in the app and invites dragging and petting. [W1]

The useful design principle is continuity: the character in the product demonstration is the character users expect to adopt. For our app, onboarding art, tiny desktop frames, settings portraits, and promotional illustrations should share one approved character model. Workcat's particular silhouette, gait, copy, palette, and logo are not assets to reuse.

The screenshots viewed during research establish website layout, not a complete native-app UI audit. Do not invent a Workcat dashboard, reward economy, Pomodoro workflow, or asset-production technology from this landing page.

## Detection and intervention: what is known

The Workcat page says detection evaluates predefined targets locally and screen contents are not sent away. It describes closing a window with the cat's paw and says there is no confirmation dialog or settings detour. It does not document URL matching, accessibility-tree inspection, image recognition, model use inside drifty, tab targeting, unsaved-work behavior, or false-positive rates. [W1, W3]

These missing details matter for Windows. A foreground executable name cannot identify Shorts within a browser, and a window title is an unreliable proxy. Our design therefore uses browser-authorized context and explicit rules, rather than assuming that Workcat's undisclosed implementation can be reproduced safely with generic Windows input commands.

## Relationship to drifty

drifty is a broader automatic activity tracker. Its site describes capturing apps, sites, and sessions; AI classification into Focus, Neutral, or Drift; timelines and reports; and local, cloud, or bring-your-own-key classification options. It advertises a Mac requirement of macOS 13+ on Apple Silicon and a separate Intel path. [W4]

Its optional cat is off by default and comes in six coats/four sizes. It joins an intervention after confirmed drift. The page describes catching the cat before its paw lands to prevent the tab or app from closing. A separate opt-in **Extreme Nudge**, labeled Beta, can close a browser tab or quit an app. [W4]

Do not merge this into the standalone Workcat specification. Workcat's minimal local predefined-target promise and drifty's contextual activity-classification system are distinct offerings. Our first release does not need drifty-style AI, activity surveillance, account infrastructure, or reports to reproduce the emotional value of a companion that interrupts a chosen feed.

## Privacy statements and limits

The Workcat privacy policy, updated 28 August 2026, labels the app section as a design position for an unshipped product. It promises local predefined-target detection, no account for the core behavior, and no transmission of screen contents. [W3]

The website says it has no visitor analytics, ad scripts, cookies, or visitor identifiers in browser storage. It also discloses Cloudflare server request records, Google Fonts connections, and optional download emails relayed to drifty/Resend. Therefore, “the website makes no outside connections” and “the entire ecosystem is offline” would both be inaccurate. [W3]

## Lessons for our product

**Keep:** immediate character presence, a single understandable focus action, local processing, low visual clutter, and the same character quality everywhere.

**Improve deliberately:** show exactly which browsers/sites are protected; provide a visible allow/pause escape; revalidate the target before intervening; handle multiple monitors and mixed scaling; distinguish companion-only mode from active protection.

**Differentiate:** an original hand-painted character; a soft reversible feed shield rather than default closing; exceptional Windows behavior; keyboard-accessible controls; simple sessions with no account or model download.

**Validate:** whether people welcome the cat after a week, whether the feed shield reduces unwanted sessions, whether users trust its decisions, and whether art can remain expressive at 128 logical pixels. Public marketing does not answer these questions.
