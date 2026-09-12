# Art direction and character bible

User-approved medium: **soft, hand-painted 2D**. The earlier v001 assets were absent from this checkout. The original v002 reference was generated on 11 September 2026 through the authorized Azure endpoint. It has a cream coat, dark brown eyes, clay/taupe on the anatomical RIGHT ear/temple (viewer-left in the front view), a taupe back and tail, and a charcoal tail tip. Preserve the actual v002 reference. The left-facing views were authored separately, not mirrored.

The current set includes idle breathing, left/right walks and runs, play/pounce, paw washing and cheek grooming, settling to sleep, breathing while asleep, waking, and a reaching paw. `art-source/provenance.json` records source images and prompts. All artwork is original, neutral cat behavior; no competitor art, branded character, named-artist imitation, or mature content is used. The table below remains the broader original animation wish list; actual shipped clips live in the v2 manifest.

12 September revision: `assets/cat-v003` replaces locomotion with twelve source keys per direction/action and adds a friendly surprised pickup pose (wide eyes, small curious mouth, relaxed dangling paws) and soft landing. `art-source/provenance-v003.json` records the new Azure requests. It retains the same v002 character and asymmetric coat. Dragging must look cute and comfortably supported, never frightened or hurt.

## Character concept

Internal character ID: `cat_01`. The v002 character is a small cream-and-taupe studio kitten with a compact body, rounded triangular ears, a broad soft muzzle, dark brown eyes, and a thick gently hooked tail. A muted clay patch sits over the anatomical right ear and temple; the tail has a charcoal tip. These asymmetries distinguish the character and create a continuity test.

Personality: patient, curious, a little sleepy, quietly pleased to share a desk. It does not scold, cry, starve, or panic when focus breaks. Its notice pose expresses curiosity; its paw gesture looks like gently placing a paper screen in front of a feed.

This description is an exploration starting point. Lock exact proportions and markings only after concept review. Do not use Workcat, Mochi, BongoCat, commercial animation characters, or a named artist as image references to imitate.

## Shape, texture, and light

- Readable silhouette at 96, 128, and 160 logical pixels tall.
- Head roughly one third of seated total height; soft body mass and short paws; avoid a human-like torso.
- Large simple eye/muzzle masses; details must survive downscaling. Expression should read from pose before eyebrows or tiny mouth changes.
- Restrained painterly fur texture inside color masses. Avoid noisy individual fur strands, glossy plastic, photorealism, heavy outlines, or overly airbrushed shading.
- Soft upper-left studio lighting, neutral enough for arbitrary desktop wallpaper. No dramatic cast lighting baked into one side.
- Mostly opaque interior, clean antialiased contour, fully transparent exterior. Do not bake a white or colored halo into alpha.
- Shadow, if used, is a separate optional click-through layer. No floor plane, room background, text, watermark, or decorative sparkle in sprite frames.

## Palette

Character paint palette is separate from UI semantic colors. Initial swatches: warm cream `#EDE0C9`, taupe `#9A9185`, clay `#C28D73`, dark moss `#394C43`, soft nose `#B97870`. Adjust the approved reference for silhouette contrast; do not force every local value to these exact swatches at the cost of natural shading.

Build contrast from shape and value rather than a bright outer glow. Inspect on white, near-black, colored wallpaper, text editors, and busy browser pages. The darkest/lightest coats need their own contour QA if added later.

## Required reference package

1. Front, rear, left-side, right-side, and three-quarter views with consistent anatomy.
2. Seated, standing, curled, and stretched proportions; paw and tail construction notes.
3. Marking map explicitly labeled by anatomical side. Do not mirror asymmetrical finished art to fake the opposite direction.
4. Expression sheet: neutral, curious, content, sleepy, gentle notice. Avoid angry or distressed intervention faces.
5. Palette and lighting notes; close-up of intended brush texture.
6. Real-size 96/128/160 DIP silhouettes on both themes.

Keep the approved reference immutable under a version ID. Any anatomy change means a new reference version and a review of all dependent poses.

## Initial animation set

These are production targets to refine after blocking the motion. Frame durations, not a global FPS assumption, control the final playback.

| Clip ID | Approximate frames | Behavior | Loop / interrupt |
| --- | --- | --- | --- |
| `idle` | 6 | Breath, small ear/tail motion, long held frames | Loop; interrupt anytime |
| `blink` | 3 | Brief eyes close/open over idle | One shot; return to idle |
| `walk_left` | 8 | Small grounded edge walk, consistent foot contact | Loop; blend/settle on stop |
| `walk_right` | 8 | Matching opposite direction with correct markings | Loop; separately authored |
| `sit_down` | 5 | Standing-to-seated transition | One shot |
| `sleep` | 6 | Curled breathing, long holds | Loop; wake transition |
| `wake` | 5 | Lift head, uncoil/settle | One shot |
| `pet` | 6 | Lean into pet, eyes relax | One shot; rate-limited |
| `drag` | 2 | Composed held pose, no distress | Hold/short loop |
| `land` | 4 | Soft settle after drag, no violent bounce | One shot |
| `notice` | 4 | Look up, attentive posture | One shot; cancel safely |
| `paw_guard` | 6 | Gentle paw placing gesture | One shot; visual only |
| `celebrate` | 6 | Tiny pleased stretch/nod | One shot, no confetti |

Approximately 69 source frames before reuse or refinement; do not commission many coats before this set is coherent. Internal alpha only needs idle, walk, notice, paw, and pet. Public P0 requires all declared interactions or a deliberate requirement update.

## Animation production rules

Use hand-painted key poses with cleanup and controlled in-betweens. Image generation can help obtain poses and paint treatments but is not assumed to produce a temporally stable sprite sheet. Do not independently generate dozens of unrelated frames and call them an animation.

Lock the canvas, camera, palette, scale, pivot, tail length, and marking map. Anchor at a ground-contact pivot near the lower center. Frame registration prevents jitter; foot contact prevents skating. Timing needs holds, anticipation, and settle rather than identical frame durations everywhere.

Priorities: drag > user interaction > focus notice/flourish > session transition > idle. Cancelled interactions return through an appropriate settle or directly to a quiet pose. Animation state never commits a browser command; it can signal a visual milestone only.

## Surrounding art

P0 needs an app icon, tray icon variants, one welcome vignette, small rule/status illustrations only where useful, and an approved cat portrait. Use native vector controls for UI icons; image generation is for cat/illustration raster assets, not text, buttons, or entire UI screenshots.

The icon should read as this particular cat at 16/24/32 pixels with a simplified silhouette. Export appropriate Windows `.ico` sizes from reviewed masters. Artwork in the settings preview may be larger, but it must not have different eyes, markings, or texture from the cat on the desktop.
