using System.Collections.Generic;
using UnityEngine;

namespace Hushwake
{
    public static class WorldArt
    {
        static readonly Dictionary<Color, Material> Materials = new Dictionary<Color, Material>();
        public static Material Material(Color color)
        {
            if (Materials.TryGetValue(color, out var found)) return found;
            var material = new Material(Shader.Find("Standard"));
            material.color = color;
            material.SetFloat("_Glossiness", .08f);
            Materials[color] = material;
            return material;
        }
        public static GameObject Shape(Transform parent, PrimitiveType type, Vector3 position, Vector3 scale, Color color, string name = "detail")
        {
            var obj = GameObject.CreatePrimitive(type); obj.name = name;
            obj.transform.SetParent(parent, false); obj.transform.localPosition = position; obj.transform.localScale = scale;
            obj.GetComponent<Renderer>().sharedMaterial = Material(color);
            var collider = obj.GetComponent<Collider>(); if (collider != null) Object.Destroy(collider);
            return obj;
        }
        public static float Height(float x, float z) => Mathf.Sin(x * .063f) * .6f + Mathf.Cos(z * .083f) * .45f + Mathf.Sin((x + z) * .13f) * .18f;
        public static Vector3 Ground(float x, float z) => new Vector3(x, Height(x, z), z);
        public static Mesh TerrainMesh()
        {
            const int size = 100; const float cell = 2.5f;
            var vertices = new List<Vector3>(); var colors = new List<Color>(); var triangles = new List<int>();
            for (var z = 0; z < size; z++) for (var x = 0; x < size; x++)
            {
                var p = new Vector3((x - size / 2) * cell, 0, (z - size / 2) * cell);
                var corners = new[] { p, p + new Vector3(cell, 0, 0), p + new Vector3(0, 0, cell), p + new Vector3(cell, 0, cell) };
                foreach (var i in new[] { 0, 2, 1, 1, 2, 3 })
                {
                    var v = corners[i]; v.y = Height(v.x, v.z); vertices.Add(v);
                    var hue = Mathf.PerlinNoise(v.x * .035f + 100, v.z * .035f + 100);
                    colors.Add(Color.Lerp(new Color(.14f, .29f, .27f), new Color(.36f, .48f, .34f), hue));
                    triangles.Add(triangles.Count);
                }
            }
            var mesh = new Mesh { name = "Hushwake low-poly landscape", indexFormat = UnityEngine.Rendering.IndexFormat.UInt32 };
            mesh.SetVertices(vertices); mesh.SetColors(colors); mesh.SetTriangles(triangles, 0); mesh.RecalculateNormals(); return mesh;
        }
        public static GameObject Landscape(Transform parent)
        {
            var obj = new GameObject("Landscape", typeof(MeshFilter), typeof(MeshRenderer)); obj.transform.SetParent(parent, false);
            obj.GetComponent<MeshFilter>().sharedMesh = TerrainMesh();
            obj.GetComponent<MeshRenderer>().sharedMaterial = new Material(Shader.Find("Hushwake/Landscape"));
            return obj;
        }
        public static void Tree(Transform parent, Vector3 position, float height, Color leaves)
        {
            Shape(parent, PrimitiveType.Cylinder, position + Vector3.up * height * .35f, new Vector3(.28f, height * .35f, .28f), new Color(.22f, .18f, .16f), "trunk");
            var crown = Shape(parent, PrimitiveType.Sphere, position + Vector3.up * height * .8f, new Vector3(height * .6f, height * .7f, height * .6f), leaves, "canopy");
            crown.transform.rotation = Quaternion.Euler(13, position.x * 19, 7);
        }
        public static void House(Transform parent, Vector3 position, Color roof)
        {
            Shape(parent, PrimitiveType.Cube, position + Vector3.up * 1.5f, new Vector3(4, 3, 3), new Color(.69f, .69f, .53f), "limestone house");
            var top = Shape(parent, PrimitiveType.Cube, position + Vector3.up * 3.4f, new Vector3(4.8f, .55f, 3.8f), roof, "copper roof"); top.transform.rotation = Quaternion.Euler(0, 0, -7);
            Shape(parent, PrimitiveType.Cube, position + new Vector3(0, 1.25f, -1.52f), new Vector3(.9f, 2.5f, .1f), new Color(.13f, .19f, .22f), "door");
            Shape(parent, PrimitiveType.Cube, position + new Vector3(-1.25f, 1.7f, -1.56f), new Vector3(.55f, .8f, .15f), new Color(.95f, .66f, .28f), "warm window");
        }
        public static Transform Character(Transform parent, Vector3 position, Color coat, string name)
        {
            var root = new GameObject(name).transform; root.SetParent(parent, false); root.localPosition = position;
            Shape(root, PrimitiveType.Capsule, new Vector3(0, .95f, 0), new Vector3(.58f, .62f, .45f), coat, "coat");
            Shape(root, PrimitiveType.Sphere, new Vector3(0, 1.68f, 0), new Vector3(.44f, .48f, .44f), new Color(.74f, .52f, .36f), "face");
            Shape(root, PrimitiveType.Sphere, new Vector3(0, 1.86f, -.05f), new Vector3(.49f, .22f, .49f), new Color(.15f, .14f, .22f), "hair");
            Shape(root, PrimitiveType.Capsule, new Vector3(-.17f, .32f, 0), new Vector3(.19f, .33f, .21f), new Color(.13f, .19f, .22f), "left boot");
            Shape(root, PrimitiveType.Capsule, new Vector3(.17f, .32f, 0), new Vector3(.19f, .33f, .21f), new Color(.13f, .19f, .22f), "right boot");
            Shape(root, PrimitiveType.Cube, new Vector3(.44f, .84f, .12f), new Vector3(.07f, .9f, .12f), new Color(.6f, .89f, .86f), "threadblade");
            return root;
        }
        public static Transform Label(Transform parent, Vector3 position, string text, Color color)
        {
            var obj = new GameObject("label", typeof(TextMesh)); obj.transform.SetParent(parent, false); obj.transform.localPosition = position;
            var label = obj.GetComponent<TextMesh>(); label.text = text; label.fontSize = 36; label.characterSize = .055f; label.anchor = TextAnchor.MiddleCenter; label.alignment = TextAlignment.Center; label.color = color;
            return obj.transform;
        }
        public static void Ring(Transform parent, Vector3 position, float radius, Color color)
        {
            for (var i = 0; i < 24; i++)
            {
                var angle = i * Mathf.PI * 2 / 24;
                Shape(parent, PrimitiveType.Cube, position + new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * radius, new Vector3(.18f, .07f, .18f), color, "ring");
            }
        }
    }
}
