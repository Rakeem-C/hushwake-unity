# Fidelity texture sources

The grass, rock, bark and cobblestone PBR maps in this folder are Poly Haven assets, distributed under CC0-1.0. Original download URLs, resolutions and verified MD5 hashes are in `provenance.json`.

- https://polyhaven.com/a/grass_ground
- https://polyhaven.com/a/rock_ground_02
- https://polyhaven.com/a/bark_brown_02
- https://polyhaven.com/a/cobblestone_floor_02
- https://polyhaven.com/a/denim_fabric
- https://polyhaven.com/a/white_plaster_02
- https://polyhaven.com/a/tree_small_02 (tree geometry, branch, trunk and leaf maps)
- https://polyhaven.com/a/leafy_grass
- License: https://polyhaven.com/license

Diffuse maps are 2048×2048; normal and roughness maps are 1024×1024. Unity imports mipmaps and compressed GPU formats with repeat wrapping and anisotropic filtering. The original Hushwake models remain separate from these surface enhancements.

The forest model was normalized and reduced into two LODs. The original verified FBX is editor-only and excluded from Git and player builds. forest-model-provenance.json and forest-textures-provenance.jsonl record its downloads. Editor reduction uses MIT-licensed UnityMeshSimplifier, upstream commit 53fdb3122645bcd3ad2c258235200dff3dfbaa9b; source and notices are under Assets/Editor/ThirdParty/UnityMeshSimplifier.
