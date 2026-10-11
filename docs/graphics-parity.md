# Graphics parity target

Reference: original Hushwake Sites v8, source `ac071c727210da99b34d654be92b38311698c58d`, inspected through normal browser input on October 10, 2026. Local benchmark screenshot: `.qa/original-benchmark.png`.

The target is the original game's detailed stylized presentation, rather than the initial Unity primitive prototype. Compare at 1280×720 using ordinary controls. Asset import alone is not proof of final rendering.

| Area | Required visual evidence |
| --- | --- |
| Characters | Original Nera/resident faces, clothing, hair, boots and equipment; readable at the gameplay camera; animated walking |
| Settlements | Original regional facade, roof, window and construction meshes; PBR maps; paving, lamps and courtyard props |
| Landscape | Original sampled topography, detailed ground substrate, dense varied grass and branched leaf canopies; distant landforms and water |
| Atmosphere | Soft directional shadows, balanced sky/fill light, haze, water movement and weather detail; distinguish Wake/Hush |
| Dungeons | Regional original architecture and station models, textured floors and readable portals; original distinct bosses |
| Presentation | Close gameplay framing, restrained readable labels/HUD, styled title/menu typography; no default gray prototype presentation |
| Reliability | No browser runtime/shader errors; frame-rate evidence in active tab; normal-input opening route, dungeon puzzle and save replay |

Art conversion preserves mesh positions, normals, UVs, vertex colors, shared PBR textures, hierarchy and handedness. Source assets stay separate from the original checkout. Google Fonts Cormorant Garamond is distributed under the bundled OFL license.

Pass 1 restored the original characters, houses and trees, but its light terrain, small trees and incomplete dungeon art did not meet the target. Pass 2 (`9d244b73f92935335f8d077e72972e5b20800cf8`) restored 240 original models, water, larger canopies and regional dungeon art. A fresh browser replay confirmed the opening bell sequence, realm change and dungeon entry, but found 7–10 fps and foreground occlusion. That candidate is withheld from publication.

Accepted graphics candidate: `6f606e860833720962883c2fc4f2c6c960d1422b`, Unity 6000.3.26f1, Web export 43,817,265 bytes, built 2026-10-11 00:45 UTC. Both the 543-check progression audit and 372-check art audit passed. It uses rigid material batching with retained limb pivots, stationary world batching, foreground camera cutaway, original height/water samples, smooth shoreline coverage, textured meadow/cobble surfaces, original unlit and translucent profiles, restrained post-processing and a styled story-focused title.

Fresh normal-input browser replay confirmed the bell reward, readable Wake/Hush terrain after the seventh candidate's lighting failure, unobstructed Tidemark entry and The Dry Lip, West Float, rejected East Float before Low Gate, successful gauge sequence, Silt Gallery entry, visible moving rat/eel, attack/instrument effects, red attack telegraph and dodge. Save/title/full reload/Continue restored Silt Gallery, Hush, quest stage and 91 health. Map, journal, inventory, pause and wheel zoom passed. Browser console errors: none.

The comparison restores the original artwork and its level of modeled detail; it does not claim identical composition, physical simulation or every frame of the original renderer. The title/settlement, riverbank and first dungeon provide the visual sample. All imported regional architecture and bosses pass asset checks, but unvisited regions/bosses still need live visual playthroughs.

On this browser's Intel HD Graphics 530, the original read-only diagnostics measured 12.7–14.5 fps at adaptive 832×468 / 960×540. The sixth detailed candidate measured 16–18 fps at 1280×720; the final candidate reached 17 fps on the Hush bell bank and 30–32 fps through the dungeon with lighter lighting. These are scene samples, not a controlled identical-camera benchmark or a universal performance guarantee. Detailed shadows can cost substantially more; the sustained-slow-play fallback preserves art and resolution while disabling real-time shadows and MSAA. F3 exposes optional frame timings.

Local evidence: `.qa/original-benchmark.png`, `.qa/graphics-pass6-timing.png`, `.qa/graphics-pass7-lighting-regression.png`, `.qa/graphics-pass8-save-reload.png`. Fresh public deployment evidence is recorded in `docs/verification.md`. Full campaign gameplay verification remains a separate requirement.
