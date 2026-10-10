using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

namespace Hushwake
{
    // Share merged rigid pieces between instances, while retaining every animated pivot.
    public static class ModelBatching
    {
        sealed class Piece { public Mesh mesh; public Material material; public int[] pivot; }
        static readonly Dictionary<string, Piece[]> cache = new Dictionary<string, Piece[]>();
        public static bool AnimatedPivot(Transform t) => t.name.Contains(":hip:") || t.name == "leftLeg" || t.name == "rightLeg" || t.name.Contains("upper-arm") || t.name.Contains(":arm:");
        static int[] Path(Transform t, Transform root) { var path = new List<int>(); while (t != root) { path.Add(t.GetSiblingIndex()); t = t.parent; } path.Reverse(); return path.ToArray(); }
        static Transform Resolve(Transform root, int[] path) { foreach (int i in path) root = root.GetChild(i); return root; }
        public static void Optimize(string key, Transform root)
        {
            // Room walls stay individually cullable for the camera cutaway.
            if (key.StartsWith("room-")) return;
            var filters = root.GetComponentsInChildren<MeshFilter>();
            if (!cache.TryGetValue(key, out var pieces))
            {
                var groups = new Dictionary<(Transform, Material), List<MeshFilter>>();
                foreach (var filter in filters)
                {
                    var renderer = filter.GetComponent<MeshRenderer>(); if (renderer == null || filter.sharedMesh == null) continue;
                    var pivot = filter.transform; while (pivot != root && !AnimatedPivot(pivot)) pivot = pivot.parent;
                    var group = (pivot, renderer.sharedMaterial);
                    if (!groups.ContainsKey(group)) groups[group] = new List<MeshFilter>(); groups[group].Add(filter);
                }
                var result = new List<Piece>();
                foreach (var group in groups)
                {
                    var mesh = new Mesh { name = key + " rigid material group", indexFormat = IndexFormat.UInt32 };
                    mesh.CombineMeshes(group.Value.Select(f => new CombineInstance { mesh = f.sharedMesh, transform = group.Key.Item1.worldToLocalMatrix * f.transform.localToWorldMatrix }).ToArray(), true, true, false);
                    mesh.RecalculateBounds();
                    result.Add(new Piece { mesh = mesh, material = group.Key.Item2, pivot = Path(group.Key.Item1, root) });
                }
                pieces = result.ToArray(); cache[key] = pieces;
            }
            foreach (var filter in filters) { var renderer = filter.GetComponent<MeshRenderer>(); if (renderer != null) renderer.enabled = false; }
            foreach (var piece in pieces)
            {
                var obj = new GameObject("Batched original detail", typeof(MeshFilter), typeof(MeshRenderer)); obj.transform.SetParent(Resolve(root, piece.pivot), false);
                obj.GetComponent<MeshFilter>().sharedMesh = piece.mesh; obj.GetComponent<MeshRenderer>().sharedMaterial = piece.material;
            }
        }
        public static void StaticWorld(Transform world)
        {
            var objects = world.GetComponentsInChildren<MeshRenderer>().Where(r => r.enabled && r.GetComponent<MeshFilter>() != null && r.GetComponentInParent<ArtMotion>() == null && r.GetComponent<GeneratedMeshLifetime>() == null).Select(r => r.gameObject).ToArray();
            if (objects.Length > 0) StaticBatchingUtility.Combine(objects, world.gameObject);
        }
    }
}
