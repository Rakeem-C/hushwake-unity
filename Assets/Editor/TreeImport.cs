using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityMeshSimplifier;
namespace Hushwake.Editor {
 public static class TreeImport {
  const string Folder="Assets/Resources/Fidelity/Forest";
  static Mesh Part(Mesh source,int submesh,Matrix4x4 transform,Bounds bounds){
   var map=new Dictionary<int,int>();var vertices=new List<Vector3>();var uv=new List<Vector2>();var normals=new List<Vector3>();var indices=source.GetTriangles(submesh);var positions=source.vertices;var tex=source.uv;var n=source.normals;float scale=10/bounds.size.y;
   for(int i=0;i<indices.Length;i++){int old=indices[i];if(!map.TryGetValue(old,out int id)){id=vertices.Count;map[old]=id;var p=transform.MultiplyPoint3x4(positions[old]);p-=new Vector3(bounds.center.x,bounds.min.y,bounds.center.z);vertices.Add(p*scale);uv.Add(tex[old]);normals.Add(transform.MultiplyVector(n[old]).normalized);}indices[i]=id;}
   var result=new Mesh{indexFormat=IndexFormat.UInt32};result.SetVertices(vertices);result.SetUVs(0,uv);result.SetNormals(normals);result.SetTriangles(indices,0);result.RecalculateTangents();result.RecalculateBounds();return result;
  }
  static Mesh Reduce(Mesh source,int target){var simplify=new MeshSimplifier(source);var options=SimplificationOptions.Default;options.MaxIterationCount=180;options.Agressiveness=10;options.VertexLinkDistance=0.000001;simplify.SimplificationOptions=options;simplify.SimplifyMesh(Mathf.Min(1,target/(source.triangles.Length/3f)));var result=simplify.ToMesh();result.RecalculateBounds();result.RecalculateTangents();return result;}
  // Keep complete disconnected leaves, rather than punching random triangle holes in the crown.
  static Mesh LeafBudget(Mesh source,int budget,float expansion){
   var v=source.vertices;var indices=source.triangles;var parent=new int[v.Length];var weld=new Dictionary<Vector3Int,int>();for(int i=0;i<v.Length;i++){var key=Vector3Int.RoundToInt(v[i]*100000);if(weld.TryGetValue(key,out int first))parent[i]=first;else{parent[i]=i;weld[key]=i;}}
   Func<int,int> find=null;find=i=>{while(parent[i]!=i){parent[i]=parent[parent[i]];i=parent[i];}return i;};
   for(int i=0;i<indices.Length;i+=3){int a=find(indices[i]),b=find(indices[i+1]),c=find(indices[i+2]);parent[b]=a;parent[c]=a;}
   var groups=new Dictionary<int,List<int>>();for(int i=0;i<indices.Length;i+=3){int key=find(indices[i]);if(!groups.ContainsKey(key))groups[key]=new List<int>();groups[key].AddRange(new[]{indices[i],indices[i+1],indices[i+2]});}
   Debug.Log("FOREST_LEAF_COMPONENTS "+groups.Count);if(groups.Count<100)throw new Exception("Cannot safely thin a connected crown");
   var chosen=new List<int>();var centers=new Dictionary<int,Vector3>();foreach(var group in groups.OrderBy(g=>unchecked((uint)(g.Key*374761393+713)))){if(chosen.Count+group.Value.Count>budget*3)continue;var unique=group.Value.Distinct().ToArray();Vector3 center=Vector3.zero;foreach(int index in unique)center+=v[index];center/=unique.Length;foreach(int index in unique)centers[index]=center;chosen.AddRange(group.Value);}
   var mapping=new Dictionary<int,int>();var positions=new List<Vector3>();var normals=new List<Vector3>();var uv=new List<Vector2>();var n=source.normals;var tex=source.uv;
   for(int i=0;i<chosen.Count;i++){int old=chosen[i];if(!mapping.TryGetValue(old,out int next)){next=positions.Count;mapping[old]=next;positions.Add(centers[old]+(v[old]-centers[old])*expansion);normals.Add(n[old]);uv.Add(tex[old]);}chosen[i]=next;}
   var result=new Mesh{indexFormat=IndexFormat.UInt32};result.SetVertices(positions);result.SetNormals(normals);result.SetUVs(0,uv);result.SetTriangles(chosen,0);result.RecalculateTangents();result.RecalculateBounds();return result;
  }
  static void Save(UnityEngine.Object asset,string path){var existing=AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);if(existing==null)AssetDatabase.CreateAsset(asset,path);else{EditorUtility.CopySerialized(asset,existing);UnityEngine.Object.DestroyImmediate(asset);} }
  static Material Surface(string name,string diffuse,string normal,string rough){var m=new Material(Shader.Find("Hushwake/OriginalArt")){name=name,color=Color.white};m.SetTexture("_MainTex",Resources.Load<Texture2D>("Fidelity/"+diffuse));m.SetTexture("_NormalMap",Resources.Load<Texture2D>("Fidelity/"+normal));m.SetTexture("_RoughnessMap",Resources.Load<Texture2D>("Fidelity/"+rough));m.SetFloat("_HasNormal",1);m.SetFloat("_HasRoughness",1);m.SetFloat("_Cull",2);return m;}
  public static void Run(){
   FidelityImport.ConfigureTextures();System.IO.Directory.CreateDirectory(Folder);AssetDatabase.Refresh();
   var asset=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Editor/ArtSource/tree_small_02.fbx");var filter=asset.GetComponentInChildren<MeshFilter>();var renderer=filter.GetComponent<Renderer>();var source=filter.sharedMesh;var bounds=renderer.bounds;Debug.Log("FOREST_SOURCE_BOUNDS "+bounds);
   var branch=Surface("Photographed fine branches","tree_small_02_branch_diff","tree_small_02_branch_nor_gl","tree_small_02_branch_rough");
   var leaf=Surface("Photographed foliage","tree_small_02_leaves_diff","tree_small_02_leaves_nor_gl","tree_small_02_leaves_rough");leaf.SetTexture("_AlphaMap",Resources.Load<Texture2D>("Fidelity/tree_small_02_leaves_alpha"));leaf.SetFloat("_HasAlpha",1);leaf.SetFloat("_Wind",1);leaf.SetFloat("_Cull",0);leaf.color=new Color(.95f,1,.91f);leaf.SetColor("_EmissionColor",new Color(.012f,.019f,.008f));
   var trunk=Surface("Photographed mature trunk","tree_small_02_Diffuse","bark_brown_02_nor_gl","bark_brown_02_Rough");
   Save(branch,Folder+"/branch.mat");Save(leaf,Folder+"/leaf.mat");Save(trunk,Folder+"/trunk.mat");AssetDatabase.SaveAssets();
   var root=new GameObject("Mature woodland tree");var lods=new LOD[2];int total=0;
   for(int lod=0;lod<2;lod++){
    var renderers=new List<Renderer>();for(int part=0;part<source.subMeshCount;part++){
     string type=renderer.sharedMaterials[part].name.Contains("leaves")?"leaf":renderer.sharedMaterials[part].name.Contains("trunk")?"trunk":"branch";string path=Folder+"/"+type+"-lod"+lod+".asset";
     Mesh reduced;if(lod==0){var original=Part(source,part,filter.transform.localToWorldMatrix,bounds);int target=type=="leaf"?30000:type=="trunk"?3500:2500;Debug.Log("FOREST_REDUCE "+type+" sourceTriangles="+original.triangles.Length/3);reduced=Reduce(original,target);UnityEngine.Object.DestroyImmediate(original);}
     else reduced=Reduce(AssetDatabase.LoadAssetAtPath<Mesh>(Folder+"/"+type+"-lod0.asset"),type=="leaf"?7000:type=="trunk"?850:650);
     int leafBudget=lod==0?30000:7000;if(type=="leaf"&&reduced.triangles.Length/3>leafBudget){var thinned=LeafBudget(reduced,leafBudget,lod==0?2.5f:1.6f);UnityEngine.Object.DestroyImmediate(reduced);reduced=thinned;}
     int count=reduced.triangles.Length/3;Debug.Log("FOREST_LOD "+lod+" "+type+" triangles="+count);if(count==0||count>50000)throw new Exception("Unbounded or empty forest mesh");if(lod==0)total+=count;reduced.name=type+" forest LOD "+lod;Save(reduced,path);AssetDatabase.SaveAssets();
     var child=new GameObject(type+" canopy LOD "+lod,typeof(MeshFilter),typeof(MeshRenderer));child.transform.SetParent(root.transform,false);child.GetComponent<MeshFilter>().sharedMesh=AssetDatabase.LoadAssetAtPath<Mesh>(path);var r=child.GetComponent<MeshRenderer>();r.sharedMaterial=AssetDatabase.LoadAssetAtPath<Material>(Folder+"/"+type+".mat");r.shadowCastingMode=type=="leaf"?ShadowCastingMode.Off:ShadowCastingMode.On;renderers.Add(r);
    }lods[lod]=new LOD(lod==0?.32f:.025f,renderers.ToArray());
   }
   var group=root.AddComponent<LODGroup>();group.SetLODs(lods);group.RecalculateBounds();PrefabUtility.SaveAsPrefabAsset(root,Folder+"/mature-tree.prefab");UnityEngine.Object.DestroyImmediate(root);AssetDatabase.SaveAssets();if(total>55000)throw new Exception("Forest near mesh exceeds budget");Debug.Log("FOREST_IMPORT_SUCCEEDED nearTriangles="+total);
  }
 }
}
