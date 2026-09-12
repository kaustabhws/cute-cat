# Code-drawn companion — current direction

12 September 2026. The latest user request supersedes hand-painted generated sprites: one original rounded, block-like 2D cat, entirely drawn and animated in code. The six supplied pictures guide simplicity and expression; their contours, characters and artwork are not copied. The Azure authorization remains valid but is unnecessary for this implementation.

## Build sequence

1. Inspect the actual checkout and refresh primary sources when research claims change. The initial 0.4 rebuild started from documentation only; current native source exists. Historical 0.2/0.3 claims cannot be reused as current evidence.
2. Implement an original cubic-path character and deterministic, continuous rig: walk, run, meow, groom, play, sleep, pickup, landing, reaching paw. Smooth pose blending, distance-linked stance, tail follow-through, blinking and breathing.
3. Render that rig directly into a reusable premultiplied Win32 layered surface. WPF provides ordinary native controls; neither WebView nor a game runtime is required.
4. Implement focus timing, tray, local settings, pointer recovery, display bounds and explicit quiet/reduced-motion controls.
5. Implement opt-in, scoped shell notification detection, measured paw contact, identity revalidation and cancellation. Supply an explicitly labeled practice card and a real Windows test banner.
6. Run deterministic and native integration checks, inspect actual renderer exports in both themes and at desktop sizes, measure cadence/resources, package a self-contained executable, update the evidence and handoff.

## Rig contract

The unit canvas is 320 × 256, bottom-center pivot (160,228); default visible height is 128 DIP. Paths and colors are source code, not a sprite manifest. `CatRig.Evaluate` accepts monotonic time, action age and displacement phase. The same `CatPose` drives desktop and UI preview. Pose changes blend over 320 ms with a quintic curve; geometry never changes to another generated image. `CatRig.Contact` is a defined forepaw endpoint during the contact plateau. Dismissal is requested only after this exact pose is successfully painted and actual HWND rounding is accounted for.

Revision 0.5.0 adds a separate continuous orientation channel and 860 ms grounded turns. Head/body/tail use staggered curves; the front-facing cross-section retains width. The user requested a cuter original character, implemented through a shorter plumper body, larger soft cheeks and a plush tail. Turn replay and both boundary directions have deterministic/native checks. Existing pointer recovery now explicitly permits the needed turn before translation.

The implementation owns the contour, palette, face and gait. No competitor code, model, raster art, font or sound is imported. Diagnostic PNGs and animation previews are outputs of the code renderer and are never runtime animation inputs.

## Acceptance

- Walking/running in both directions, grooming, sleep/breath/wake, meow, playful hop, drag/land and notification reach have distinct continuous motion.
- Calm autonomous scheduling, explicit replay, quiet focus and reduced motion remain controllable.
- Hidden/suspended rendering stops. One reusable surface replaces the historical frame cache. Active frame cadence and memory are measured, not inferred from a 60 Hz setting.
- Tests cover stale/moved notification identity, expiry, hide/drag/disable, rendered contact, session recovery and corrupt/future state. Physical click-through and shell compatibility must be reported separately from simulated inputs.
- Browser protection remains a subsequent dependency-ready task under the original implementation plan; this deliverable is the requested companion/notification milestone.

See [current build status](build-status.md) for results; older art manifests are historical specifications and are not used by this renderer.
