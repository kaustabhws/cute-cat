# Original code-drawn character

Current medium: **simple, rounded 2D vector art with a continuous procedural rig**, explicitly requested 12 September 2026. This supersedes D01's hand-painted direction and D17/D20's image-frame pipeline. The historical specification is retained in `history/04-hand-painted-art.md`.

## Character bible

Internal character `oat-cat-01`, revision 2 (0.5.0); default nickname Pip is editable. The user asked for a cuter, less realistic companion: a shorter plump body, larger round cheeks, little bean paws, a short plush hooked tail, low-set eyes with tiny catchlights and a small W-shaped smile. The warm cream coat, peach cheeks and three oat marks preserve the original identity. Orientation is projected continuously through a round front view; the renderer never mirrors the whole drawing.

The reference images establish the desired simplicity, friendliness and readable silhouette. No attached contour, Workcat art, competitor code, model, animation or logo is copied. The renderer's cubic paths, proportions and timing are original to this project.

| Part | Definition |
| --- | --- |
| Coat | `#FCF0D5` |
| Contour / eyes | `#4A463A` / `#403D34`, rounded strokes |
| Far paws | `#E4D5B5`, subtle depth |
| Ear / forehead marks | `#E2AB97` |
| Cheeks | `#ECB8A5` |
| Canvas / pivot | 320 × 256 units; ground pivot (160,228) |
| Size tiers | 96 / 128 / 160 logical pixels by the 180-unit design height |
| Notification contact | Forepaw endpoint (287,107), plateau in reaching pose |

`CatRig.cs` owns analytic motion parameters. `CatPainter.cs` owns shapes, colors and draw order. `CatSurface.cs` supplies a reusable premultiplied alpha surface. The UI preview draws the same pose through the same painter.

## Motion language

- Walk/run: four short articulated legs, stance tied to distance, airborne return arc, body bob, stretch and tail follow-through. Running has a larger stride and suspension; direction is independent of character markings.
- Turn: a grounded 0.86-second change of direction. The head looks first, alternating paws lift slightly, the torso turns through a full-width front view, and the tail follows. Both boundaries, explicit replay, notification approach and return use the same transition.
- Idle: slow breathing, occasional blinking and a quiet tail.
- Groom: lifted paw visibly passes in front of the cheek, with head dips and small licking motions.
- Sleep/wake: the same tail curls around the body, head lowers, eyes close and breathing slows; waking blends back to standing.
- Meow: a small mouth opens/closes and the head rises. Optional quiet sound is synthesized locally; off by default.
- Play: a small springy hop with all feet leaving the baseline.
- Pickup/drop: a friendly wide-eyed pose, comfortable dangling paws, then a short squash/settle.
- Paw: reach to a defined endpoint, hold for contact, return. The rig never directly dismisses a notification.

Pose changes blend over 320 ms using a quintic curve. No generated keys, optical flow, decoded image frames or video are used at runtime. Preview WebPs are diagnostic recordings of code output.

## Visual acceptance

Inspect actual size tiers on light and dark backgrounds, action silhouettes, groom draw order, sleep tail continuity, both directions and cycle seams. Review native WPF controls in both themes and at the minimum window size. Check measured active frame cadence separately: a smooth mathematical curve is not proof of timely presentation.

No public naming or licensing clearance is claimed. Source creation is documented in [the production note](05-art-production.md).
