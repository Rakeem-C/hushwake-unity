using System.IO;
using System.Linq;
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
   foreach(var path in Directory.GetFiles("Assets/Resources/Fidelity").Where(p=>p.EndsWith(".jpg")||p.EndsWith(".png"))){
    var importer=(TextureImporter)AssetImporter.GetAtPath(path);importer.sRGBTexture=path.Contains("Diffuse")||path.Contains("_diff.");
    // Preserve RGB OpenGL normal encoding for the custom surface shaders.
    importer.textureType=TextureImporterType.Default;importer.mipmapEnabled=true;importer.wrapMode=TextureWrapMode.Repeat;importer.filterMode=FilterMode.Trilinear;importer.anisoLevel=8;importer.maxTextureSize=importer.sRGBTexture?2048:1024;importer.textureCompression=TextureImporterCompression.CompressedHQ;importer.SaveAndReimport();
   }
   foreach(var guid in AssetDatabase.FindAssets("t:Material",new[]{"Assets/Resources/OriginalArt"})){
    var material=AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(guid));if(material.shader.name!="Hushwake/OriginalArt")continue;
    var name=AuthoredName(material);
    if(name.Contains("bark")){Apply(material,"bark_brown_02",1.7f);material.color=new Color(.8f,.75f,.68f);enhanced++;}
    else if(name=="hushwake-authored:leaf"){
     material.SetTexture("_MainTex",Resources.Load<Texture2D>("Fidelity/tree_small_02_leaves_diff"));material.SetTexture("_AlphaMap",Resources.Load<Texture2D>("Fidelity/tree_small_02_leaves_alpha"));material.SetTexture("_NormalMap",Resources.Load<Texture2D>("Fidelity/tree_small_02_leaves_nor_gl"));material.SetTexture("_RoughnessMap",Resources.Load<Texture2D>("Fidelity/tree_small_02_leaves_rough"));material.SetFloat("_HasAlpha",1);material.SetFloat("_HasNormal",1);material.SetFloat("_HasRoughness",1);material.SetFloat("_Wind",1);material.SetFloat("_Cull",0);material.color=new Color(.85f,.96f,.78f);material.SetColor("_EmissionColor",new Color(.015f,.025f,.008f));material.mainTextureScale=new Vector2(.195f,.14f);material.mainTextureOffset=new Vector2(.02f,.705f);enhanced++;
    }
    else if(name=="hushwake:settlement-finish:plaster"){Apply(material,"white_plaster_02",2);enhanced++;}
    else if(name.Contains("character:")&&name.Contains("cloth")){
     // Preserve each resident's costume palette; add fine weave relief without dyeing everyone blue.
     material.SetTexture("_NormalMap",Resources.Load<Texture2D>("Fidelity/denim_fabric_nor_gl"));material.SetTexture("_RoughnessMap",Resources.Load<Texture2D>("Fidelity/denim_fabric_Rough"));material.SetFloat("_HasNormal",1);material.SetFloat("_HasRoughness",1);enhanced++;
    }
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
