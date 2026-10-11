# Graphics regression guardrails

## Articulated rigid characters

Symptom: straight stick-like walking despite detailed authored knee and elbow meshes.
Cause: material batching merged descendant geometry under the root or hip instead of preserving the actual shoulder, elbow, knee and ankle pivots.
Prevention: mark the authored joints before caching batches; retain their hierarchy paths and all authored vertices. ArtAudit requires two visible knees, elbows and shoulders for Nera and Mara, plus unchanged vertex totals.
Proof: run Hushwake.Editor.ArtAudit.Run, then observe walking and combat in a freshly compiled browser player. A static audit alone cannot verify animation quality.
Files: Assets/Scripts/ModelBatching.cs, Assets/Scripts/ArtMotion.cs, Assets/Editor/ArtAudit.cs.

## Bounded photographic foliage

Do not ship raw multi-million-triangle source models. Import complete leaf components into bounded near/distant meshes; retain UV silhouettes, verify decreasing LOD budgets, and exclude LOD children from world static batching. Dense grass is tiled and distance culled. Keep tree/building contact shading valid in balanced lighting and clear the outdoor shade field on indoor transitions.
Proof: ArtAudit bounds both LOD budgets and contact shading; fresh normal-input outdoor/indoor camera replay and measured frame-rate samples are required before release. CC0/MIT provenance remains tracked; raw import files remain editor-only and ignored.

A foreground crown can contain the camera, making the generic slab entry distance zero. Forest cutaway must treat camera containment as blocking and share a single full-tree bound across every LOD renderer; otherwise leaves hide the avatar or individual tree parts flicker. ArtAudit covers camera-inside-crown and distant-tree cases. Replay the normal Tidemark approach and zoom after every cutaway change.
