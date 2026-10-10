# Rendering guardrails

## Detailed art must remain playable and visible

The second art candidate (`9d244b73f92935335f8d077e72972e5b20800cf8`) preserved the original models but ran at 7–10 fps in the active browser. Large foreground trees and the first dungeon's central masonry also hid Nera. Asset existence and successful compilation did not detect either failure.

Keep rigid mesh parts grouped by material and preserve separate moving limb pivots. Share the merged geometry between repeated models, then batch stationary world pieces. Do not merge a whole room into opaque material bounds: its facade pieces must remain individually eligible for camera cutaway. Exclude animated characters and generated terrain/water/meadow surfaces from that cutaway.

`ArtAudit` verifies that batching preserves the authored vertex count and a visible animated Nera leg, and that foreground bounds obstructing the camera are cut away while background bounds remain visible. Shorelines use fine original water samples and coarse coplanar interior tiles rather than a uniformly expensive dense mesh.

Required release proof: fresh ordinary-input browser replay of the settlement, movement toward the Tidemark entrance, and The Dry Lip. Nera and the controls must remain visible, and active-tab frame rate must be recorded. Run the full Web build and inspect the browser error log. The original failing screenshot is `.qa/graphics-pass2-occlusion.png`; local QA images are excluded from Git.

Relevant files: `ModelBatching.cs`, `CameraVisibility.cs`, `WorldArt.cs`, `WorldDetail.cs`, `Game.cs`, `ArtAudit.cs`, and `Tools/ExportArt.ts`.
