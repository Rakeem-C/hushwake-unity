using UnityEditor;
using UnityEngine;
namespace Hushwake.Editor {
 public static class InspectTree {
  public static void Run(){AssetDatabase.Refresh();var root=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Editor/ArtSource/tree_small_02.fbx");foreach(var filter in root.GetComponentsInChildren<MeshFilter>()){var r=filter.GetComponent<Renderer>();Debug.Log("TREE_SOURCE "+filter.name+" vertices="+filter.sharedMesh.vertexCount+" triangles="+filter.sharedMesh.triangles.Length/3+" bounds="+filter.sharedMesh.bounds+" materials="+string.Join(",",System.Array.ConvertAll(r.sharedMaterials,m=>m.name)));}Debug.Log("TREE_INSPECTION_DONE");}
 }
}
