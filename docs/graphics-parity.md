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

The next candidate adds rigid material batching, static world batching, camera cutaway, greener meadow detail and finer shorelines. Its art regression audit passed 300 checks; a new Web build and replay are required before parity can be claimed. Full campaign gameplay verification remains a separate requirement.
