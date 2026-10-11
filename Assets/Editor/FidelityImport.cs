using System.IO;
using UnityEditor;
using UnityEngine;
namespace Hushwake.Editor {
 public static class FidelityImport {
  [System.Serializable]public class Names{public string[] names;}
  static string[] names;
  static string AuthoredName(Material material){if(material.name.StartsWith("material-")&&int.TryParse(material.name.Substring(9),out int id)&&id>=0&&id<names.Length)return names[id];return material.name;}
  public static void Run(){
   AssetDatabase.Refresh();
   names=JsonUtility.FromJson<Names>(Resources.Load<TextAsset>("Fidelity/material-names").text).names;
   int enhanced=0;
   foreach(var path in Directory.GetFiles("Assets/Resources/Fidelity","*.jpg")){
    var importer=(TextureImporter)AssetImporter.GetAtPath(path);importer.sRGBTexture=path.Contains("Diffuse");
    // Preserve RGB OpenGL normal encoding for the custom surface shaders.
    importer.textureType=TextureImporterType.Default;importer.mipmapEnabled=true;importer.wrapMode=TextureWrapMode.Repeat;importer.filterMode=FilterMode.Trilinear;importer.anisoLevel=8;importer.maxTextureSize=path.Contains("Diffuse")?2048:1024;importer.textureCompression=TextureImporterCompression.CompressedHQ;importer.SaveAndReimport();
   }
   foreach(var guid in AssetDatabase.FindAssets("t:Material",new[]{"Assets/Resources/OriginalArt"})){
    var material=AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(guid));if(material.shader.name!="Hushwake/OriginalArt")continue;
    var name=AuthoredName(material);
    if(name.Contains("bark")){Apply(material,"bark_brown_02",1.7f);enhanced++;}
    else if(name=="cobble"||name=="courtstone"){Apply(material,"cobblestone_floor_02",1);enhanced++;}
    else if(name.Contains("fieldstone")||name=="limestone"){Apply(material,"rock_ground_02",1.2f);enhanced++;}
    EditorUtility.SetDirty(material);
   }
   var boulder=AssetDatabase.LoadAssetAtPath<Material>("Assets/Resources/Fidelity/boulder.mat");if(boulder==null){boulder=new Material(Shader.Find("Hushwake/OriginalArt"));AssetDatabase.CreateAsset(boulder,"Assets/Resources/Fidelity/boulder.mat");}Apply(boulder,"rock_ground_02",2);boulder.color=new Color(.88f,.90f,.86f);EditorUtility.SetDirty(boulder);
   var organic=new System.Collections.Generic.HashSet<Mesh>();
   foreach(var guid in AssetDatabase.FindAssets("t:Prefab",new[]{"Assets/Resources/OriginalArt"}))foreach(var filter in AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(guid)).GetComponentsInChildren<MeshFilter>()){
    var renderer=filter.GetComponent<MeshRenderer>();if(renderer==null)continue;var name=AuthoredName(renderer.sharedMaterial);
    if(name.Contains("character:")&&(name.Contains("skin")||name.Contains("cloth")||name.Contains("hair")||name.Contains("leather")))organic.Add(filter.sharedMesh);
   }
   foreach(var mesh in organic){var positions=mesh.vertices;var normals=mesh.normals;var sums=new System.Collections.Generic.Dictionary<Vector3Int,Vector3>();
    for(int i=0;i<positions.Length;i++){var key=Vector3Int.RoundToInt(positions[i]*10000);if(!sums.ContainsKey(key))sums[key]=Vector3.zero;sums[key]+=normals[i];}
    for(int i=0;i<normals.Length;i++){var average=sums[Vector3Int.RoundToInt(positions[i]*10000)].normalized;if(Vector3.Dot(normals[i],average)>.45f)normals[i]=average;}
    mesh.normals=normals;mesh.RecalculateTangents();EditorUtility.SetDirty(mesh);
   }
   if(organic.Count<100||enhanced<8)throw new System.Exception("Fidelity changes were not applied to the authored materials and organic geometry");
   AssetDatabase.SaveAssets();ArtAudit.Run();Debug.Log("HUSHWAKE_FIDELITY_IMPORT_SUCCEEDED organicMeshes="+organic.Count+" materials="+enhanced);
  }
  static void Apply(Material m,string asset,float scale){m.SetTexture("_MainTex",Resources.Load<Texture2D>("Fidelity/"+asset+"_Diffuse"));m.SetTexture("_NormalMap",Resources.Load<Texture2D>("Fidelity/"+asset+"_nor_gl"));m.SetTexture("_RoughnessMap",Resources.Load<Texture2D>("Fidelity/"+asset+"_Rough"));m.SetFloat("_HasNormal",1);m.SetFloat("_HasRoughness",1);m.mainTextureScale=Vector2.one*scale;}
 }
}
