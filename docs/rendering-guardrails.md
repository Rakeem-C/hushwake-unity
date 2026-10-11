# Rendering guardrails

## Detailed art must remain playable and visible

The second art candidate (`9d244b73f92935335f8d077e72972e5b20800cf8`) preserved the original models but ran at 7–10 fps in the active browser. Large foreground trees and the first dungeon's central masonry also hid Nera. Asset existence and successful compilation did not detect either failure.

Keep rigid mesh parts grouped by material and preserve separate moving limb pivots. Share the merged geometry between repeated models, then batch stationary world pieces. Do not merge a whole room into opaque material bounds: its facade pieces must remain individually eligible for camera cutaway. Exclude animated characters and generated terrain/water/meadow surfaces from that cutaway.

`ArtAudit` verifies that batching preserves the authored vertex count and a visible animated Nera leg, and that foreground bounds obstructing the camera are cut away while background bounds remain visible. Shorelines use fine original water samples and coarse coplanar interior tiles rather than a uniformly expensive dense mesh.

Required release proof: fresh ordinary-input browser replay of the settlement, movement toward the Tidemark entrance, and The Dry Lip. Nera and the controls must remain visible, and active-tab frame rate must be recorded. Run the full Web build and inspect the browser error log. The original failing screenshot is `.qa/graphics-pass2-occlusion.png`; local QA images are excluded from Git.

Relevant files: `ModelBatching.cs`, `CameraVisibility.cs`, `WorldArt.cs`, `WorldDetail.cs`, `Game.cs`, `ArtAudit.cs`, and `Tools/ExportArt.ts`.

The third candidate fixed the obstruction and passed the gauge sequence and gallery combat replay with no console errors, reaching 25–27 fps indoors but only 13 fps outside. Matching the original's 960×540 drawing resolution alone did not fix the outdoor workload, so the release keeps the 1280×720 native image. The next pass bounds terrain geometry, prevents terrain from casting expensive atlas-wide shadows, supplies the runtime ambient probe, and follows the original's software-renderer shadow policy. A lighter lighting option is available in Pause, with automatic fallback only after sustained slow active play; it keeps all authored art. Fresh browser evidence is still required.

The fourth candidate still showed 11–13 fps outdoors after the lighting fallback. Camera visibility must cache fixed scenery bounds, perform managed segment/bounds tests, and write `Renderer.enabled` only on an actual visibility transition. Hidden distant labels must not be rotated each frame. Bound distant detail ranges while retaining the full terrain/water atlas and detailed nearby models; animated actors are distance culled without applying a facade cutaway to them. Preserve all 65 original unlit surfaces and both translucent material profiles, with a portable profile manifest checked by `ArtAudit` (372 checks).
