# Artwork production pipeline

Status: v002 original reference, eight reference-conditioned six-key action sheets, and a registered motion pack generated on 11 September 2026. The user's Azure `gpt-image-2` endpoint was used successfully; see [configuration](../art/azure-image-generation.md). The medium is soft hand-painted 2D. The former v001 code/assets did not exist in this checkout.

## Production stages

| Stage | Deliverable | Exit gate |
| --- | --- | --- |
| A0: exploration | Three clearly different original silhouettes, same general medium | Choose one character direction; no sprite production yet |
| A1: reference lock | Turnaround, marking map, expressions, palette, proportions | Character reads at desktop size and is consistent from both sides |
| A2: motion blocking | Few key poses for idle/walk/notice/paw/pet | Gesture, pivot, timing, and silhouette work in a native playback preview |
| A3: clean animation | Registered individual frames, authored holds/in-betweens | No anatomy drift, jitter, foot slide, popping texture, or lighting changes |
| A4: exports | Alpha PNGs/atlases, JSON manifest, previews | Contract validation, resource-budget check, alpha and size QA pass |
| A5: product art | Icon, tray marks, welcome vignette, portrait | Same identity across native UI and desktop; icon legible at small sizes |

Character-direction review is a creative checkpoint for choosing the product's identity, not a requirement to ask permission for every generation or reversible edit. Apply the user's existing approvals and continue routine cleanup within the approved direction.

## Using generation well

Use the [prompt pack](../art/prompt-pack.md). Generate concepts first, then condition subsequent work on the exact approved reference image and version. A repeated textual prompt or seed alone does not ensure the same character. Preserve prompts, model/tool identifiers, generation date, source/reference relationships, and edit history in an art provenance log.

Prefer editing an approved image for a new pose when it preserves anatomy better. Generate individual clean key poses, not a promise of a perfect 8×8 motion grid. A grid can be an exploration contact sheet but must be inspected and sliced/registered manually or through a reviewed script.

If transparent output is supported, request it. If not, obtain a clean separable background and perform controlled matting. Never deliver a checkerboard painted into RGB as “transparency.” Reject images with uncertain rights, recognizable borrowed characters, signatures, or unexplained watermarks.

## Source organization to create during implementation

```text
art-source/
  cat_01/reference/v001/
  cat_01/concepts/
  cat_01/keyposes/
  cat_01/animation-source/
  provenance.jsonl
assets/
  cat_01/v001/atlas-256.png
  cat_01/v001/atlas-384.png
  cat_01/v001/manifest.json
  ui/app.ico
  ui/tray-light.ico
  ui/tray-dark.ico
  ui/welcome.png
art-previews/
  cat_01/contact-sheet.png
  cat_01/playback.webm
  cat_01/alpha-backgrounds.png
```

These are planned paths, not existing files. Large editable masters stay out of release bundles. Decide Git LFS or an external asset archive when the first large source set exists; record location and immutable hashes so other agents can reproduce exports.

## Export contract

- Editable masters at least 1024 px tall for painting/cleanup. Runtime art is exported deliberately rather than decoding master-size images on startup.
- Current exports use 384×384 untrimmed RGBA frames, ground pivot (192,354), a measured contact point for the paw frame, and a v2 clip manifest with per-frame durations, looping and loop entry. The source consists of one reference plus eight six-key sheets. Gaits have reviewed offline optical-flow in-betweens; idle/sleep use small deformations of one painted pose to avoid fur drift. No claim of hand-drawn in-betweens is made.
- sRGB, 8-bit RGBA PNG with straight alpha on disk. Convert once to premultiplied BGRA for native compositing; do not double-premultiply.
- The current [schema](../contracts/asset-manifest.schema.json) maps clips to hashed PNG files, frame durations, loop entry, pivot, and the paw-contact anchor. The loader validates version, paths, durations, loop bounds, and checksums. GDI caches only the current clip; the WPF preview cache is bounded.

## Reproducible local export

For the current 0.3.0 pack, run `scripts/prepare_motion_v003.py`, then `scripts/validate_project.py`. This consumes the preserved base pack and the four revised twelve-key gait sheets plus the original drag pose. It writes `assets/cat-v003` (packaged as `assets/cat`), 234 frame entries in twelve clips, and light/dark review sheets and animated WebP previews under `art-previews/v003`. The revised warp uses an inverse displacement map, blends paint only on opaque overlap, smooths the displacement field, and removes detached interpolation islands. Walks have 48 frames per cycle; runs have 36. The 384px pivot/contact coordinates remain unchanged.

The following original commands reproduce the preserved 0.2.0 baseline, not the current locomotion revision.

Run `scripts/prepare_art.py`, then `scripts/tween_motion.py`. Preparation uses the bundled chroma helper followed by a magenta-hue mask and connected-component cleanup. This fixes uneven key backgrounds and generated grid-line residue. Scale is fixed per clip, body registration is preserved, and foot baselines are aligned. The fast gallop uses one warped surface per in-between; an earlier alpha blend created ghost paws and was rejected during review. Raw sheets and the approved reference stay unchanged.

Runtime files are in `assets/cat`; raw requests in `output/imagegen`; cleaned masters in `art-source/alpha`; prompts and provenance in `art-source`; actual-size light/dark contact sheets and motion GIFs in `art-previews`. Build tooling uses Pillow, NumPy, SciPy, and OpenCV; these are not runtime app dependencies.
- Trimmed atlases require offsets to preserve the original canvas/pivot. The initial contract deliberately uses untrimmed equal-size cells to reduce integration mistakes.
- Leave at least two transparent pixels between packed cells to avoid sampling bleed. Sprite export tooling owns the atlas; a model does not lay out the production atlas.
- No automatic horizontal flip for the asymmetrical cat. Left/right views need approved consistent anatomical markings.
- Cache only the active clips/resolution set. Measure decoded memory, not just compressed PNG size. For example, a 384×384 RGBA frame is 589,824 bytes before overhead.
- Record content hashes and reference version. The example manifest contains placeholder image paths and is not a production pack.

If 384 px frames are insufficient for a large cat at 250% scale, either add a budgeted higher-resolution tier or cap the maximum size with a documented quality decision. Do not silently upscale low-quality sprites while claiming all sizes are polished.

## Art QA checklist

At actual desktop scale, inspect:

- Silhouette and expression on white, black, warm paper, colored wallpaper, and dense text.
- Anatomy: count paws/ears; compare head/body ratio, eye spacing, tail length, and marking position against the reference.
- Edge alpha: no halos, clipped fur, colored matte, checkerboard, stray pixels, or huge invisible input rectangle.
- Registration: static body mass does not wobble due to inconsistent framing; pivots remain aligned.
- Motion: believable acceleration, paw contact, walk cycle seam, idle holds, start/stop transitions, and cancellation.
- Timing: character remains calm over a 20-minute session; notice animation communicates without becoming the distraction.
- UI consistency: icon, preview, welcome vignette, and all clips still depict the same cat.
- Reduced motion: attractive still states exist; functionality survives without motion.

Have a contact sheet for anatomy and a playback preview for timing. Inspect on the actual native renderer before accepting the export. A beautiful master illustration or generated sheet is not enough.

## Provenance and rights

Each accepted asset records creator/tool, date, reference IDs, prompt/edit notes, source file, export hash, intended usage, and applicable license/terms. Do not copy Workcat's artwork, gait, screenshots, icons, or marketing assets into the shipped product.

If using an external font, sound, brush texture, icon set, or open-source pet asset, record its own license separately from the application code license. Commissioned manual cleanup should include the needed commercial usage rights. Do not promise that generated output is legally unique or automatically protected in every jurisdiction.

## Initial effort and cost model

Budget roughly 1–2 focused days for concepts/reference review, 4–8 days for key poses and animation cleanup, and 2–4 days for export/integration/QA, with revisions likely. These are planning ranges for one character, not a guaranteed generation turnaround. A polished animation set may need an illustrator/animator even with image-generation access.

Track generation spend and manual time per accepted asset; the useful metric is approved coherent frames, not images generated. Do not place a generation API key in the desktop app: art is produced at build/design time and bundled as ordinary assets.
