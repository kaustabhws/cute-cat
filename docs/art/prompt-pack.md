# Original artwork prompt pack

Use with the art-direction and production documents. These prompts are inputs for a later generation/editing phase; none has been executed as part of this research task. Replace reference placeholders with actual approved files. Keep the user's chosen hand-painted medium throughout.

## 1. Concept exploration

> Create an original small studio cat character for a native Windows focus companion. Soft hand-painted 2D illustration, warm matte pigment, carefully simplified shapes, patient and slightly sleepy personality. Compact pear-shaped body, short paws, rounded triangular ears, broad soft muzzle, dark moss eyes, thick gently hooked tail. Cream and warm-gray fur, muted clay patch over the anatomical left ear and temple, tiny charcoal mark near the tail base. Clear silhouette when reduced to 128 pixels tall. Soft neutral upper-left lighting. One complete seated character, three-quarter view, centered with generous transparent padding. Transparent background if supported. No text, no logo, no watermark, no props, no room, no floor plane, no cast shadow. Avoid glossy 3D, photoreal fur, thin noisy details, neon outlines, and recognizable existing characters.

Explore three silhouette/proportion variants separately. Keep a labeled contact sheet outside the model output; do not request accurate generated typography.

## 2. Reference views

> Use [APPROVED_REFERENCE_V001] as the exact character identity. Paint one [FRONT / REAR / LEFT SIDE / RIGHT SIDE / THREE-QUARTER] view. Preserve proportions, head shape, eye spacing, ear shape, muzzle, tail thickness and length, palette, matte brush texture, lighting, and anatomical markings. The clay patch remains on the cat's anatomical left, not merely the viewer's left. Neutral standing pose, full body visible, same scale and ground-contact line as the reference, transparent background, no props or text. Do not redesign or mirror the markings.

Generate each view separately and compare side by side. Reject views that cannot plausibly belong to the same model.

## 3. Key pose

> Edit [APPROVED_REFERENCE_V001] into [POSE_DESCRIPTION]. Preserve the exact same cat anatomy, scale, camera, marking map, palette, lighting, and brush texture. Keep the feet/ground pivot at [PIVOT_GUIDE] within the same [CANVAS_GUIDE]. Full silhouette and tail inside frame. Transparent background; no baked shadow. Change only what the pose requires. This is a single animation key pose, not a sprite sheet.

Useful pose descriptions:

- Idle: relaxed seated body, eyelids soft, tail resting with a gentle hook.
- Notice: head lifts slightly, ears orient forward, attentive and kind, no alarm or anger.
- Paw guard: one forepaw extends as if gently setting a small sheet of paper down, balanced body, no aggressive swatting.
- Pet: lean the head a little into an imagined gentle touch, content eyes, stable body proportions.
- Sleep: comfortable curled pose, face still readable, tail wraps naturally around body.
- Walk contact: side view with near forepaw and far rear paw making believable ground contact; define anatomical side explicitly.

## 4. Cleanup edit

> Correct only [SPECIFIC_DEFECT] in [FRAME_FILE]. Match [REFERENCE_FILE] and neighboring frames [NEIGHBOR_FILES]. Preserve all other anatomy, proportions, markings, silhouette registration, lighting, texture, and transparent padding. Do not add detail or reinterpret the character.

Fix one defect at a time. Geometry registration and alpha cleanup may be better handled in an editor or export script than through another generation.

## 5. Welcome illustration

> Paint the exact cat from [APPROVED_REFERENCE_V001] quietly resting beside a small closed notebook and a plain ceramic cup, a sparse warm studio-desk vignette. Same soft hand-painted 2D treatment, matte cream/gray/clay palette, friendly mature tone, ample empty space, no text or UI controls, no trademarks, no busy room. Composition must crop gracefully inside a native application card and remain distinct from the transparent desktop sprite.

## 6. Small icon concept

> Simplify the exact approved cat's head and hooked tail into a bold original app-icon concept, readable at tiny sizes. Keep its defining ear shape and clay marking. Restrained two-to-four-color design with a clear silhouette and balanced negative space. No letters, no watermark, no complex fur. This is a concept to be redrawn and checked as a Windows icon, not a finished multi-resolution icon file.

## Acceptance reminder

Before accepting any output: compare with the reference, inspect actual alpha, downscale to runtime size, verify anatomical side, and play it next to neighboring frames. Do not describe a generated pose as production-ready until it passes native playback QA.
