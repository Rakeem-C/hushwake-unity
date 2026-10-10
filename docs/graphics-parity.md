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

Current status: first browser candidate building; visual parity is pending comparison and further iteration. Full campaign gameplay verification remains a separate requirement.
