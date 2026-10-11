using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
namespace Hushwake.Editor {
 public static class ImportOriginalArt {
  [Serializable] public class Manifest {public int version;public string sourceCommit;public MeshDef[] meshes;public MatDef[] materials;public TexDef[] textures;public ModelDef[] models;}
  [Serializable] public class MeshDef {public string file;public int vertices;}
  [Serializable] public class MatDef {public string name;public float[] color,emission;public float emissionIntensity,roughness,metallic,opacity,alphaCutoff;public bool doubleSided,unlit;public int map,normal,roughnessMap;}
  [Serializable] public class TexDef {public string file;public int width,height;public bool srgb;public float[] repeat,offset;}
  [Serializable] public class ModelDef {public string name;public Node node;}
  [Serializable] public class Node {public string name;public float[] position,rotation,scale;public int mesh,material;public Node[] children;}
  static Mesh[] meshes;static Material[] mats;
  static Vector3 V(float[] a)=>new Vector3(a[0],a[1],a[2]);
  static Color C(float[] a)=>new Color(a[0],a[1],a[2],1);
  [Serializable]public class ProfileIndex {public int[] unlit;public Opacity[] translucent;}
  [Serializable]public class Opacity {public int id;public float opacity;}
  static Shader ProfileShader(MatDef d)=>Shader.Find(d.unlit?"Hushwake/OriginalUnlit":d.opacity<.99f?"Hushwake/OriginalTransparent":"Hushwake/OriginalArt");
  static void ProfileManifest(Manifest data){var unlit=new System.Collections.Generic.List<int>();var translucent=new System.Collections.Generic.List<Opacity>();for(int i=0;i<data.materials.Length;i++){if(data.materials[i].unlit)unlit.Add(i);if(data.materials[i].opacity<.99f)translucent.Add(new Opacity{id=i,opacity=data.materials[i].opacity});}File.WriteAllText("Assets/Resources/OriginalArt/material-profiles.json",JsonUtility.ToJson(new ProfileIndex{unlit=unlit.ToArray(),translucent=translucent.ToArray()},true));}
  public static void UpdateMaterialProfiles(){var data=JsonUtility.FromJson<Manifest>(File.ReadAllText(".art-import/manifest.json"));if(data.sourceCommit!="ac071c727210da99b34d654be92b38311698c58d")throw new Exception("Unexpected material provenance");for(int i=0;i<data.materials.Length;i++){var d=data.materials[i];var m=AssetDatabase.LoadAssetAtPath<Material>($"Assets/Resources/OriginalArt/material-{i}.mat");if(m==null||ProfileShader(d)==null)throw new Exception("Missing material profile");m.shader=ProfileShader(d);var tint=C(d.color).gamma;tint.a=d.opacity;m.color=tint;EditorUtility.SetDirty(m);}ProfileManifest(data);AssetDatabase.SaveAssets();AssetDatabase.Refresh();ArtAudit.Run();Debug.Log("HUSHWAKE_MATERIAL_PROFILES_SUCCEEDED");}
  public static void Run(){
   var input=Path.GetFullPath(".art-import");var data=JsonUtility.FromJson<Manifest>(File.ReadAllText(Path.Combine(input,"manifest.json")));
   if(data.version!=1||data.sourceCommit!="ac071c727210da99b34d654be92b38311698c58d")throw new Exception("Unexpected art provenance");
   const string folder="Assets/Resources/OriginalArt";Directory.CreateDirectory(folder);File.Copy(Path.Combine(input,"OriginalHeight.bytes"),"Assets/Resources/OriginalHeight.bytes",true);File.Copy(Path.Combine(input,"OriginalWater.bytes"),"Assets/Resources/OriginalWater.bytes",true);AssetDatabase.Refresh();
   var textures=new Texture2D[data.textures.Length];
   for(int i=0;i<textures.Length;i++){
    var d=data.textures[i];var tex=new Texture2D(d.width,d.height,TextureFormat.RGBA32,true,!d.srgb){name=d.file,wrapMode=TextureWrapMode.Repeat,filterMode=FilterMode.Trilinear,anisoLevel=4};
    var bytes=File.ReadAllBytes(Path.Combine(input,d.file));if(bytes.Length!=d.width*d.height*4)throw new Exception("Invalid texture size");tex.SetPixelData(bytes,0);tex.Apply(true,false);
    textures[i]=tex;AssetDatabase.CreateAsset(tex,$"{folder}/texture-{i}.asset");
    if(d.file=="substrate.rgba"||d.file=="macro.rgba"||d.file=="meadow.rgba")AssetDatabase.CreateAsset(UnityEngine.Object.Instantiate(tex),$"{folder}/{d.file.Replace(".rgba","")}.asset");
   }
   mats=new Material[data.materials.Length];var shader=Shader.Find("Hushwake/OriginalArt");if(shader==null)throw new Exception("Art shader absent");
   for(int i=0;i<mats.Length;i++){
    var d=data.materials[i];var tint=C(d.color).gamma;tint.a=d.opacity;var m=new Material(ProfileShader(d)){name=d.name,color=tint};m.SetFloat("_Glossiness",1-d.roughness);m.SetFloat("_Metallic",d.metallic);m.SetFloat("_Cull",d.doubleSided?0:2);m.SetFloat("_Cutoff",d.alphaCutoff);
    m.SetColor("_EmissionColor",C(d.emission)*d.emissionIntensity);m.SetFloat("_Wind",d.name.Contains("leaf")?1:0);
    if(d.map>=0){m.SetTexture("_MainTex",textures[d.map]);m.mainTextureScale=new Vector2(data.textures[d.map].repeat[0],data.textures[d.map].repeat[1]);}
    if(d.normal>=0){m.SetTexture("_NormalMap",textures[d.normal]);m.SetFloat("_HasNormal",1);}
    if(d.roughnessMap>=0){m.SetTexture("_RoughnessMap",textures[d.roughnessMap]);m.SetFloat("_HasRoughness",1);}
    mats[i]=m;AssetDatabase.CreateAsset(m,$"{folder}/material-{i}.mat");
   }
   meshes=new Mesh[data.meshes.Length];
   for(int i=0;i<meshes.Length;i++)using(var r=new BinaryReader(File.OpenRead(Path.Combine(input,data.meshes[i].file)))){
    int count=r.ReadInt32();if(count!=data.meshes[i].vertices)throw new Exception("Mesh mismatch");var p=new Vector3[count];var n=new Vector3[count];var uv=new Vector2[count];var c=new Color[count];
    for(int j=0;j<count;j++){p[j]=new Vector3(r.ReadSingle(),r.ReadSingle(),r.ReadSingle());n[j]=new Vector3(r.ReadSingle(),r.ReadSingle(),r.ReadSingle());uv[j]=new Vector2(r.ReadSingle(),r.ReadSingle());c[j]=new Color(r.ReadSingle(),r.ReadSingle(),r.ReadSingle(),r.ReadSingle());}
    int length=r.ReadInt32();var tris=new int[length];for(int j=0;j<length;j++)tris[j]=r.ReadInt32();if(r.BaseStream.Position!=r.BaseStream.Length)throw new Exception("Trailing mesh data");
    var mesh=new Mesh{name=$"original-{i}",indexFormat=IndexFormat.UInt32,vertices=p,normals=n,uv=uv,colors=c,triangles=tris};mesh.RecalculateBounds();mesh.RecalculateTangents();meshes[i]=mesh;AssetDatabase.CreateAsset(mesh,$"{folder}/mesh-{i}.asset");
   }
   foreach(var model in data.models){var root=Build(model.node,null);root.name=model.name;PrefabUtility.SaveAsPrefabAsset(root,$"{folder}/{model.name}.prefab");UnityEngine.Object.DestroyImmediate(root);}
   // Named surfaces support paving, retaining walls and detailed dungeon floors.
   for(int i=0;i<data.textures.Length;i++)if(data.textures[i].file.EndsWith("-albedo.rgba")){
    var name=data.textures[i].file.Replace("-albedo.rgba","");var m=new Material(shader){name=name};m.SetTexture("_MainTex",textures[i]);m.SetTexture("_NormalMap",textures[i+1]);m.SetFloat("_HasNormal",1);m.SetTexture("_RoughnessMap",textures[i+2]);m.SetFloat("_HasRoughness",1);AssetDatabase.CreateAsset(m,$"{folder}/{name}.mat");
   }
   var sky=new Cubemap(32,TextureFormat.RGBA32,true){name="Hushwake sky reflection"};foreach(CubemapFace face in new[]{CubemapFace.PositiveX,CubemapFace.NegativeX,CubemapFace.PositiveY,CubemapFace.NegativeY,CubemapFace.PositiveZ,CubemapFace.NegativeZ}){var colors=new Color[1024];for(int y=0;y<32;y++)for(int x=0;x<32;x++){float height=face==CubemapFace.PositiveY?1:face==CubemapFace.NegativeY?-1:(y/31f*2-1);colors[y*32+x]=Color.Lerp(new Color(.20f,.25f,.20f),new Color(.65f,.75f,.78f),(height+1)*.5f);}sky.SetPixels(colors,face);}sky.Apply(true);AssetDatabase.CreateAsset(sky,$"{folder}/SkyReflection.asset");
   ProfileManifest(data);AssetDatabase.SaveAssets();AssetDatabase.Refresh();Debug.Log($"HUSHWAKE_ART_IMPORT_SUCCEEDED models={data.models.Length} meshes={meshes.Length} textures={textures.Length}");
  }
  static GameObject Build(Node d,Transform parent){var obj=new GameObject(d.name);obj.transform.SetParent(parent,false);obj.transform.localPosition=V(d.position);obj.transform.localRotation=new Quaternion(d.rotation[0],d.rotation[1],d.rotation[2],d.rotation[3]);obj.transform.localScale=V(d.scale);
   if(d.mesh>=0){obj.AddComponent<MeshFilter>().sharedMesh=meshes[d.mesh];var renderer=obj.AddComponent<MeshRenderer>();renderer.sharedMaterial=mats[d.material];renderer.shadowCastingMode=ShadowCastingMode.On;renderer.receiveShadows=true;}
   foreach(var child in d.children)Build(child,obj.transform);return obj;}
 }
}
