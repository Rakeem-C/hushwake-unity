# Higher fidelity visual target

The user selected Nintendo's official 2026 Ocarina of Time remake, scheduled November 5, 2026, rather than the CryZENx fan remake. Reference: https://www.nintendo.com/us/store/products/the-legend-of-zelda-ocarina-of-time-125697/ . The official environment gallery was inspected live; a local reference capture stays outside the public repository.

The target is natural, dense environments, smooth character/landscape forms, clear detailed materials, atmospheric lighting and clean image edges. Restoring Hushwake's old artwork alone does not establish parity with this new reference. Do not claim matching or exceeding it without a direct visual comparison.

Candidate changes: globally continuous height-derived terrain normals, shared vertices and independently frustum-cullable terrain sections; CC0 Poly Haven 2K albedo and 1K normal/roughness maps; enhanced bark, ground, masonry and irregular rock surfaces; smoother organic normals without altering character hierarchy or vertex counts; curved grass and deterministic fern/flower placement that excludes interaction points and submerged terrain; softer shadow strength, restrained water ripples, and edge-adaptive antialiasing without blurring menus.

Regression proof: the art audit checks adjoining terrain positions/normals, high resolution mipmapped resources, source geometry preservation and existing visibility/material profiles. The importer requires at least 100 enhanced organic meshes and 8 enhanced materials so Unity's renamed material assets cannot silently bypass upgrades. Fresh browser proof is required for Wake/Hush, settlement/river views, dungeon cutaway, moving combat actors, menus, save/reload and performance before publication.

Texture source/license/hash records are included under `Assets/Resources/Fidelity`. Original source art remains attributable to the separate Hushwake export. Nintendo's images and game assets are not included in this game.
