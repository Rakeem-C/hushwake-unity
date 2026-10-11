# Rendering guardrails

## Detailed art must remain playable and visible

The second art candidate (`9d244b73f92935335f8d077e72972e5b20800cf8`) preserved the original models but ran at 7–10 fps in the active browser. Large foreground trees and the first dungeon's central masonry also hid Nera. Asset existence and successful compilation did not detect either failure.

Keep rigid mesh parts grouped by material and preserve separate moving limb pivots. Share the merged geometry between repeated models, then batch stationary world pieces. Do not merge a whole room into opaque material bounds: its facade pieces must remain individually eligible for camera cutaway. Exclude animated characters and generated terrain/water/meadow surfaces from that cutaway.

`ArtAudit` verifies that batching preserves the authored vertex count and a visible animated Nera leg, and that foreground bounds obstructing the camera are cut away while background bounds remain visible. Shorelines use fine original water samples and coarse coplanar interior tiles rather than a uniformly expensive dense mesh.

Required release proof: fresh ordinary-input browser replay of the settlement, movement toward the Tidemark entrance, and The Dry Lip. Nera and the controls must remain visible, and active-tab frame rate must be recorded. Run the full Web build and inspect the browser error log. The original failing screenshot is `.qa/graphics-pass2-occlusion.png`; local QA images are excluded from Git.

Relevant files: `ModelBatching.cs`, `CameraVisibility.cs`, `WorldArt.cs`, `WorldDetail.cs`, `Game.cs`, `ArtAudit.cs`, and `Tools/ExportArt.ts`.

The third candidate fixed the obstruction and passed the gauge sequence and gallery combat replay with no console errors, reaching 25–27 fps indoors but only 13 fps outside. Matching the original's 960×540 drawing resolution alone did not fix the outdoor workload, so the release keeps the 1280×720 native image. The next pass bounds terrain geometry, prevents terrain from casting expensive atlas-wide shadows, supplies the runtime ambient probe, and follows the original's software-renderer shadow policy. A lighter lighting option is available in Pause, with automatic fallback only after sustained slow active play; it keeps all authored art. Fresh browser evidence is still required.

The fourth candidate still showed 11–13 fps outdoors after the lighting fallback. Camera visibility must cache fixed scenery bounds, perform managed segment/bounds tests, and write `Renderer.enabled` only on an actual visibility transition. Hidden distant labels must not be rotated each frame. Bound distant detail ranges while retaining the full terrain/water atlas and detailed nearby models; animated actors are distance culled without applying a facade cutaway to them. Preserve all 65 original unlit surfaces and both translucent material profiles, with a portable profile manifest checked by `ArtAudit` (372 checks).

The sixth candidate's F3 timing overlay measured game updates and visibility below 1 ms, versus 16–18 fps outdoors. Diagnose the graphics path rather than repeatedly rewriting cheap game logic. Actor visibility uses the actor's live position, sampled once per actor per visibility pass; do not cache its spawn position. Idle limb animation must avoid repeated transform writes.

The original was measured through its read-only diagnostics on this same Intel HD Graphics 530 browser: 12.7–14.5 fps, dynamically reduced to 832×468 / 960×540, roughly 300 draw calls and 314k–327k triangles. This is a device-specific comparison, not a 60 fps guarantee or evidence for other hardware.

The seventh candidate's Lambert terrain/meadow experiment lost the runtime sky fill, leaving nearly black terrain in Hush. Do not replace the Standard surfaces without proving both realms in a fresh browser pass. Landscape swatches need display-to-linear conversion; imported source mesh vertex colors already contain their original linear values and must not be converted again. Keep the retained failing case `.qa/graphics-pass7-lighting-regression.png` and replay the same bell-bank view after a shader change.

## Fidelity surfaces must be applied, continuous and bounded

The first fidelity import passed resource checks but enhanced zero organic meshes because Unity renamed materials to numeric asset names. Resolve authored names through the tracked `Fidelity/material-names.json`; assert the number of actually enhanced meshes and materials. Texture existence alone is insufficient proof that the rendered model uses it. The corrected import enhanced 400 organic meshes and ultimately 45 materials.

Terrain patches must share positions and height-derived normals along their boundaries. `ArtAudit` checks all 33 points on an adjacent patch seam and bounded vertex counts. Keep terrain pieces individually frustum cullable; the landscape atlas must remain exempt from facade cutaway. Detail placement must exclude interaction points, houses and submerged terrain. Do not replace the normal maps' RGB encoding with Unity's packed normal import unless the shader unpacking changes with it.

Photographed leaves use an explicit linear silhouette mask, matching cropped atlas coordinates for albedo/normal/roughness, opaque cutout depth, and double-sided rendering. Preserve character palettes when adding cloth relief. Fresh release proof must show Wake and Hush, readable UI, visible moving actors, native drawing resolution and warmed-up frame rate, then repeat save/reload and inspect console errors. Higher resolution textures do not establish visual parity with an AAA reference.
