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
            if(name=="weathered rock"){
                obj.GetComponent<Renderer>().sharedMaterial=Resources.Load<Material>("Fidelity/boulder");
                var filter=obj.GetComponent<MeshFilter>();var mesh=Object.Instantiate(filter.sharedMesh);var points=mesh.vertices;
                for(int i=0;i<points.Length;i++){float irregularity=.88f+Mathf.PerlinNoise(points[i].x*6+position.x,points[i].z*6+position.z)*.24f;points[i]*=irregularity;}
                mesh.vertices=points;mesh.RecalculateNormals();mesh.RecalculateTangents();mesh.RecalculateBounds();filter.sharedMesh=mesh;obj.AddComponent<GeneratedMeshLifetime>();
            }
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
        { return TerrainPatch(0,0,256); }
        public static Mesh TerrainPatch(int startX,int startZ,int size)
        {
            const float cell = 250f/256;
            var vertices = new List<Vector3>(); var colors = new List<Color>(); var triangles = new List<int>();
            var normals=new List<Vector3>();var uv=new List<Vector2>();
            for (var z = 0; z <= size; z++) for (var x = 0; x <= size; x++)
            {
                var v=Ground((startX+x)*cell-125,(startZ+z)*cell-125);vertices.Add(v);
                // Global derivatives keep normals continuous at every chunk boundary.
                normals.Add(new Vector3(Height(v.x-.25f,v.z)-Height(v.x+.25f,v.z),.5f,Height(v.x,v.z-.25f)-Height(v.x,v.z+.25f)).normalized);uv.Add(new Vector2(v.x,v.z));
                colors.Add(Color.Lerp(new Color(.19f,.29f,.15f),new Color(.35f,.43f,.23f),Mathf.PerlinNoise(v.x*.035f+100,v.z*.035f+100)));
                if(x<size&&z<size){int n=z*(size+1)+x;triangles.AddRange(new[]{n,n+size+1,n+1,n+1,n+size+1,n+size+2});}
            }
            var mesh = new Mesh { name = "Smooth authored landscape", indexFormat = UnityEngine.Rendering.IndexFormat.UInt32 };
            mesh.SetVertices(vertices);mesh.SetColors(colors);mesh.SetUVs(0,uv);mesh.SetTriangles(triangles,0);mesh.SetNormals(normals);mesh.RecalculateTangents();mesh.RecalculateBounds();return mesh;
        }
        public static GameObject Landscape(Transform parent)
        {
            var root=new GameObject("Smooth landscape atlas");root.transform.SetParent(parent,false);var mat=new Material(Shader.Find("Hushwake/Landscape"));
            foreach(var pair in new[]{("_Grass","grass_ground_Diffuse"),("_Rock","rock_ground_02_Diffuse"),("_GrassNormal","grass_ground_nor_gl"),("_RockNormal","rock_ground_02_nor_gl"),("_GrassRough","grass_ground_Rough"),("_RockRough","rock_ground_02_Rough")})mat.SetTexture(pair.Item1,Resources.Load<Texture2D>("Fidelity/"+pair.Item2));
            for(int z=0;z<256;z+=32)for(int x=0;x<256;x+=32){var obj=new GameObject("Landscape",typeof(MeshFilter),typeof(MeshRenderer));obj.transform.SetParent(root.transform,false);obj.GetComponent<MeshFilter>().sharedMesh=TerrainPatch(x,z,32);var renderer=obj.GetComponent<MeshRenderer>();renderer.sharedMaterial=mat;renderer.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;obj.AddComponent<GeneratedMeshLifetime>();}
            return root;
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
            var model=Original(string.IsNullOrEmpty(artKey)?"nera":artKey,parent,position);model.name=name;model.localScale=Vector3.one*.82f;model.gameObject.AddComponent<ArtMotion>();ContactShadow(model);return model;
        }
        static Mesh shadowMesh;static Material shadowMaterial;
        static void ContactShadow(Transform model){
            if(shadowMesh==null){shadowMesh=new Mesh{name="Soft character contact"};shadowMesh.vertices=new[]{new Vector3(-.85f,0,-.85f),new Vector3(.85f,0,-.85f),new Vector3(-.85f,0,.85f),new Vector3(.85f,0,.85f)};shadowMesh.uv=new[]{Vector2.zero,Vector2.right,Vector2.up,Vector2.one};shadowMesh.triangles=new[]{0,2,1,1,2,3};shadowMesh.RecalculateBounds();shadowMaterial=new Material(Shader.Find("Hushwake/ContactShadow"));}
            var obj=new GameObject("Soft foot contact",typeof(MeshFilter),typeof(MeshRenderer));obj.transform.SetParent(model,false);obj.transform.localPosition=Vector3.up*.025f;obj.GetComponent<MeshFilter>().sharedMesh=shadowMesh;var renderer=obj.GetComponent<MeshRenderer>();renderer.sharedMaterial=shadowMaterial;renderer.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;renderer.receiveShadows=false;
        }
        public static Transform Original(string key,Transform parent,Vector3 position){var prefab=Resources.Load<GameObject>("OriginalArt/"+key);if(prefab==null)throw new System.InvalidOperationException("Missing original art: "+key);var result=Object.Instantiate(prefab,parent).transform;result.localPosition=position;ModelBatching.Optimize(key,result);return result;}
        public static Transform Label(Transform parent, Vector3 position, string text, Color color)
        {
            var obj = new GameObject("label", typeof(TextMesh)); obj.transform.SetParent(parent, false); obj.transform.localPosition = position;
            var label = obj.GetComponent<TextMesh>(); label.text = text; label.fontSize = 36; label.characterSize = .055f; label.anchor = TextAnchor.MiddleCenter; label.alignment = TextAlignment.Center; label.color = color;
            return obj.transform;
        }
        public static void Ring(Transform parent, Vector3 position, float radius, Color color)
        {
            var obj=new GameObject("ring",typeof(MeshFilter),typeof(MeshRenderer));obj.transform.SetParent(parent,false);obj.transform.localPosition=position;var p=new Vector3[130];var tris=new int[384];for(int i=0;i<=64;i++){float a=i*Mathf.PI*2/64;p[i*2]=new Vector3(Mathf.Cos(a),0,Mathf.Sin(a))*(radius-.035f);p[i*2+1]=new Vector3(Mathf.Cos(a),0,Mathf.Sin(a))*(radius+.035f);if(i<64){int n=i*2,j=i*6;tris[j]=n;tris[j+1]=n+2;tris[j+2]=n+1;tris[j+3]=n+1;tris[j+4]=n+2;tris[j+5]=n+3;}}var mesh=new Mesh{vertices=p,triangles=tris};mesh.RecalculateNormals();obj.GetComponent<MeshFilter>().sharedMesh=mesh;obj.GetComponent<MeshRenderer>().sharedMaterial=Material(color);obj.AddComponent<GeneratedMeshLifetime>();
        }
    }
}
