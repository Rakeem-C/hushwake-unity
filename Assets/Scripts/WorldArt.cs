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
            var template = Resources.Load<Material>("HushwakeStandard");
            var material = template != null ? new Material(template) : new Material(Shader.Find("Standard"));
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
        static float[] heights;
        public static float Height(float x, float z)
        {
            if(heights==null){var bytes=Resources.Load<TextAsset>("OriginalHeight").bytes;heights=new float[257*257];System.Buffer.BlockCopy(bytes,0,heights,0,bytes.Length);}
            var gx=Mathf.Clamp((x+125)/250*256,0,255.999f);var gz=Mathf.Clamp((z+125)/250*256,0,255.999f);int ix=(int)gx,iz=(int)gz;
            return Mathf.Lerp(Mathf.Lerp(heights[iz*257+ix],heights[iz*257+ix+1],gx-ix),Mathf.Lerp(heights[(iz+1)*257+ix],heights[(iz+1)*257+ix+1],gx-ix),gz-iz);
        }
        public static Vector3 Ground(float x, float z) => new Vector3(x, Height(x, z), z);
        public static Mesh TerrainMesh()
        {
            const int size = 256; const float cell = 250f/256;
            var vertices = new List<Vector3>(); var colors = new List<Color>(); var triangles = new List<int>();
            for (var z = 0; z < size; z++) for (var x = 0; x < size; x++)
            {
                var p = new Vector3((x - size / 2) * cell, 0, (z - size / 2) * cell);
                var corners = new[] { p, p + new Vector3(cell, 0, 0), p + new Vector3(0, 0, cell), p + new Vector3(cell, 0, cell) };
                foreach (var i in new[] { 0, 2, 1, 1, 2, 3 })
                {
                    var v = corners[i]; v.y = Height(v.x, v.z); vertices.Add(v);
                    var hue = Mathf.PerlinNoise(v.x * .035f + 100, v.z * .035f + 100);
                    colors.Add(Color.Lerp(new Color(.37f, .43f, .25f), new Color(.60f, .62f, .39f), hue));
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
            var mat=new Material(Shader.Find("Hushwake/Landscape"));mat.SetTexture("_Substrate",Resources.Load<Texture2D>("OriginalArt/substrate"));obj.GetComponent<MeshRenderer>().sharedMaterial=mat;
            return obj;
        }
        public static void Tree(Transform parent, Vector3 position, float height, Color leaves)
        {
            var art=Original("tree-"+Mathf.Abs(Mathf.RoundToInt(position.x*3+position.z))%6,parent,position);art.localScale=Vector3.one*(height/11f);art.localRotation=Quaternion.Euler(0,position.x*23,0);return;
        }
        public static void House(Transform parent, Vector3 position, Color roof,string region="hearth",int variant=0)
        {
            var art=Original("house-"+region+"-"+variant%3,parent,position);art.localScale=Vector3.one*.66f;return;
        }
        public static Transform Character(Transform parent, Vector3 position, Color coat, string name,string artKey="")
        {
            var model=Original(string.IsNullOrEmpty(artKey)?"nera":artKey,parent,position);model.name=name;model.localScale=Vector3.one*.82f;model.gameObject.AddComponent<ArtMotion>();return model;
        }
        public static Transform Original(string key,Transform parent,Vector3 position){var prefab=Resources.Load<GameObject>("OriginalArt/"+key);if(prefab==null)throw new System.InvalidOperationException("Missing original art: "+key);var result=Object.Instantiate(prefab,parent).transform;result.localPosition=position;return result;}
        public static Transform Label(Transform parent, Vector3 position, string text, Color color)
        {
            var obj = new GameObject("label", typeof(TextMesh)); obj.transform.SetParent(parent, false); obj.transform.localPosition = position;
            var label = obj.GetComponent<TextMesh>(); label.text = text; label.fontSize = 36; label.characterSize = .085f; label.anchor = TextAnchor.MiddleCenter; label.alignment = TextAlignment.Center; label.color = color;
            return obj.transform;
        }
        public static void Ring(Transform parent, Vector3 position, float radius, Color color)
        {
            var obj=new GameObject("ring",typeof(MeshFilter),typeof(MeshRenderer));obj.transform.SetParent(parent,false);obj.transform.localPosition=position;var p=new Vector3[130];var tris=new int[384];for(int i=0;i<=64;i++){float a=i*Mathf.PI*2/64;p[i*2]=new Vector3(Mathf.Cos(a),0,Mathf.Sin(a))*(radius-.035f);p[i*2+1]=new Vector3(Mathf.Cos(a),0,Mathf.Sin(a))*(radius+.035f);if(i<64){int n=i*2,j=i*6;tris[j]=n;tris[j+1]=n+2;tris[j+2]=n+1;tris[j+3]=n+1;tris[j+4]=n+2;tris[j+5]=n+3;}}var mesh=new Mesh{vertices=p,triangles=tris};mesh.RecalculateNormals();obj.GetComponent<MeshFilter>().sharedMesh=mesh;obj.GetComponent<MeshRenderer>().sharedMaterial=Material(color);obj.AddComponent<GeneratedMeshLifetime>();
        }
    }
}
