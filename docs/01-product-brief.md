# Product brief

**Current 0.9.1 companion scope:** profiles and weekly schedules, app-specific normal-close/reminder rules with exceptions/allowances, desktop manners, procedural personality, a coat/hat/neckwear/collar wardrobe, native theme controls and signed preview update/recovery. The user explicitly deferred browser integration. Current implementation and measured coverage are in [build status](build-status.md); the browser-focused loop below remains future product direction.

Status: product direction, updated 12 September 2026. Latest user choices: a simple rounded cat drawn and smoothly animated entirely in code, a completely free app, and a small native companion. Working name: **Cute Cat**; public name remains open. The implementation milestone is companion movement and notification paw dismissal; see build status for measured coverage. The earlier hand-painted direction is superseded.

## Product promise

An expressive little Windows cat that helps you leave distracting feeds and get back to the work you chose.

The app should feel like a considerate studio companion. A cat appears quickly, rests near the desktop edge, and occasionally responds to you. During a focus session, it notices sites you chose to interrupt and helps pause the feed. The experience is calm, specific, and recoverable.

## Intended users and jobs

Primary users are Windows-based students, developers, writers, and designers who drift into short-form feeds while doing self-directed work. They want light external structure and like the idea of a pet nearby. This is a hypothesis to test; it is not a claim about every knowledge worker.

| Situation | Job to be done | Successful experience |
| --- | --- | --- |
| Starting a difficult task | Help me begin without configuring another system | A named session can start in a few clicks |
| Reflexively opening Shorts | Interrupt the habit before I spend half an hour there | Chosen feed pauses; reason and next action are obvious |
| Watching a useful tutorial | Preserve legitimate research | Normal YouTube videos remain available under the Shorts rule |
| Feeling alone while working | Give me a small companion, without more obligations | The cat is present, expressive, quiet, and never demands care |
| Presenting, gaming, or taking a break | Let me control the experience | Cat/protection states are explicit and easy to pause or hide |

The initial app is not workplace surveillance, compulsory parental control, addiction treatment, an AI agent that judges work quality, or a full life-management system.

## Core loop

1. Choose the work and optionally start a short session.
2. The cat settles near a chosen screen edge; ordinary work proceeds.
3. A selected feed becomes active in a supported, connected browser.
4. After a short dwell, the cat notices. The feed is paused and covered by a small branded focus screen within the webpage.
5. Choose to return, allow five minutes, or pause protection. Nothing is closed or discarded.
6. Finish or end the session. The cat acknowledges the effort, then rests.

The desktop cat adds emotional meaning; the browser extension supplies reliable page context. The cat is not pretending to see the screen or understand the user's intentions.

## Principles

- **Companion first:** its silhouette, motion, personality, and desktop manners are essential product work.
- **Chosen boundaries:** the user selects sites/apps and knows when protection applies.
- **Specific protection:** protect Shorts without treating every browser visit or YouTube tutorial as a failure.
- **Recoverable actions:** pause and cover content; never terminate an app or close a document by default.
- **Quiet by design:** no repeated nagging, notification bursts, constant walking, or random desktop obstruction.
- **Local core:** no account, network model, screenshot capture, or backend is needed for sessions and rules.
- **Honest status:** companion-only, paused, active, and disconnected are different states.
- **No guilt:** breaks and unfinished work do not hurt the cat or remove earned items.

## What makes it distinct

The competitive research shows that Windows pets, focus games, and blockers already exist. Our proposed distinction is the care with which they meet: an original code-drawn cat with continuous motion, a small polished native control surface, clear feed-level rules, and dependable Windows integration.

We should be able to demonstrate the entire promise in 20 seconds: start a session, open Shorts, see the cat react and feed pause, allow it briefly, return to the task. A feature that does not strengthen that story is a candidate for later.

## Scope boundaries

The first release includes one cat, basic interactions, focus/break timing, Chrome/Edge connection, Shorts and user-selected whole-site protection, application nudges, a simple local session summary, and essential settings.

Later candidates include Reels/TikTok route-specific protection, optional cosmetics, ARM64, schedules, Firefox, ambient sound, and richer local statistics. No store, cloud sync, task-manager integration, generative chat, room builder, or anti-uninstall mechanism is needed to validate the product.

## Success hypotheses and instrumentation

Private-beta targets are design gates, not measured outcomes:

- At least 6 of 8 observed onboarding participants can start a protected session without coaching and explain its browser limits.
- Zero wrong-target interventions in the scripted release corpus; all observed false positives are reviewed before expanding coverage.
- At least 6 of 8 pilot users choose to keep the companion available after a week and describe interruptions as understandable. This is a qualitative signal, not a population estimate.
- At least 6 of 8 can find Pause protection within ten seconds.
- Runtime meets the resource and responsiveness budgets in the architecture document on the declared reference laptop.

Record pilot feedback with participant consent. The app's default local summary counts session time and confirmed interventions, not “time saved” or “productive hours.” No analytics service is needed for a small beta.

## Cost and presentation

The user explicitly decided on 11 September 2026 that the app is completely free of cost. No subscriptions, paid features, paid coats, accounts, upgrade prompts, or payment infrastructure. Previous monetization proposals are superseded. Use a quiet native utility, warm character art, ordinary Windows window controls, and a compact timer. Avoid KPI-card dashboards, sales language, and onboarding funnels.
