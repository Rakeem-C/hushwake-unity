# Forest and motion pass, October 11, 2026

Compiled source: `67766cae10808a2cb3c6bef5881ff72fc2828bfb`, version 0.4.0, Unity 6000.3.26f1. Built 2026-10-11 21:20:08 UTC; successful Web export: 78,005,592 bytes. The 547 gameplay assertions, 477 art assertions and source-content validation passed.

## Changes

- Photographic CC0 woodland tree geometry, fine branches, trunk surfaces and alpha-masked foliage replace the sparse procedural trees. Near trees use 47,507 triangles; distant trees use 13,787. The verified raw FBX has over two million triangles and remains ignored/editor-only. Complete leaf components are retained, with compensated coverage and natural close-up scale.
- Dense curved grass uses 8m tiles, five blades per tuft and bounded distance culling. The ground uses 2K photographed leaf litter/grass and normal/roughness maps; fern pinnae are smaller, raised and folded. Tree/building ground contact remains visible in balanced lighting and clears on indoor transitions.
- Character batching retains shoulders, elbows, knees and ankles. The procedural gait bends joints and feet through each stride while preserving every authored vertex.
- Forest cutaway shares one full-tree bound between all LOD pieces and handles cameras inside crowns. Finishing hits now clamp health to zero; the HUD and defeat state use the same bounded damage transition.

## Retained failures and correction

1. `829af3431b08d62ae0151e04e9dff7ad08fe1ae7`: safe mesh budgets but an excessively sparse crown. Withheld. Local proof: `.qa/forest-pass1-sparse.png`.
2. `7939e894303347dcd86002bf608007397f1539b9`: fuller ground cover and canopy, but oversized foreground foliage obscured the Tidemark approach and slowed it to about 19 fps. Withheld. Proof: `.qa/forest-pass2-cutaway-failure.png`.
3. `64a1d78f2aa3b220ccd16bacfd1db55925c3d0aa`: fresh normal-input replay verified the corrected approach and natural leaf scale, but combat exposed an existing negative-health HUD boundary. Withheld. Proof: `.qa/forest-pass3-negative-health.png`.
4. Final source above: defeat replay showed 0 health, Rest and try again restored 100, and the complete selected route was replayed again. Prevention rules and regression checks are in `graphics-regressions.md`, ArtAudit and Audit.

## Fresh browser proof

Ordinary keyboard/mouse input confirmed: the saved opening route; camera clearance and tree reappearance at Tidemark in Wake and Hush; defeat/recovery; a new journey, Mara's reply, all three bell stations and Waterkeeper Note; Tidemark entry; West Float; rejection of premature East Float; Low Gate then East Float unlocking Silt Gallery; visible moving enemies, red attack telegraphs, attack/Pulse/dodge; inventory, journal, map, pause, wheel zoom; save/title/full reload/Continue restoring Silt Gallery, Hush, quest stage and 100 health. Browser error scans were empty. Outdoor shade did not leak into the dungeon.

Verified browser device: Intel HD Graphics 530 via ANGLE D3D11, 1280x720. Selected settled outdoor samples were approximately 26-34 fps and indoor samples 56-60 fps, with lighter lighting used in the replay. Loading/world reconstruction and shader warmup were substantially slower. These are scene samples, not a controlled whole-campaign benchmark or a universal frame-rate guarantee.

Local evidence includes `.qa/forest-pass4-defeat-zero.png`, `.qa/forest-pass4-combat.png`, `.qa/forest-pass4-walking.png`, `.qa/forest-pass4-save-reload.png` and `.qa/forest-build-4.log`. QA captures, raw FBX and scratch imports stay outside the public repository. CC0 asset URLs/hashes and editor-only MIT simplifier notices remain tracked.

The foliage and motion are improved, but parity with the supplied Nintendo remake reference remains unachieved. Character rendering, environmental composition and cinematic lighting still need substantial art work. Full campaign, optional branches and all bosses remain outside this browser pass.
