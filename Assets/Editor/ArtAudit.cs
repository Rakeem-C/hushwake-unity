using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
namespace Hushwake.Editor {
 public static class ArtAudit {
  public static void Run(){int checks=0;Action<bool,string> test=(ok,message)=>{checks++;if(!ok)throw new Exception("Art audit failed: "+message);};
   foreach(var name in new[]{"nera","mara","tavi","imri","sava","oya","bren","sel","rohn","pella","isca","vessa","eiren","tree-0","tree-1","tree-2","tree-3","tree-4","tree-5","lantern","thread-rat","gate-eel","sentinel"}){
    var model=Resources.Load<GameObject>("OriginalArt/"+name);test(model!=null,"original model "+name);var meshes=model.GetComponentsInChildren<MeshFilter>();test(meshes.Length>=2,"detailed multipart geometry "+name);test(meshes.All(m=>m.sharedMesh!=null&&m.sharedMesh.vertexCount>0),"valid shared meshes "+name);test(model.GetComponentsInChildren<Renderer>().All(r=>r.sharedMaterial!=null&&r.sharedMaterial.shader.isSupported),"supported materials "+name);
   }
   foreach(var region in Content.Load().regions)for(int i=0;i<3;i++){var house=Resources.Load<GameObject>($"OriginalArt/house-{region.id}-{i}");test(house!=null,"regional house "+region.id);test(house.GetComponentsInChildren<MeshFilter>().Sum(m=>m.sharedMesh.vertexCount)>3000,"authored facade geometry");}
   test(Resources.Load<TextAsset>("OriginalHeight").bytes.Length==257*257*4,"original heightfield");
   test(Resources.Load<TextAsset>("OriginalWater").bytes.Length==1025*1025*4,"fine original shoreline samples");
   foreach(var dungeon in Content.Load().dungeons){test(Resources.Load<GameObject>("OriginalArt/boss-"+dungeon.id)!=null,"original regional boss");foreach(var room in dungeon.rooms)test(Resources.Load<GameObject>($"OriginalArt/room-{dungeon.id}-{room.id}")!=null,"original room architecture");foreach(var puzzle in dungeon.puzzles)for(int i=0;i<puzzle.steps.Length;i++)test(Resources.Load<GameObject>($"OriginalArt/station-{puzzle.id}-{i}")!=null,"original station art");}
   foreach(var tex in new[]{"substrate","meadow"})test(Resources.Load<Texture2D>("OriginalArt/"+tex)!=null,"original ground texture "+tex);
   foreach(var s in new[]{"Hushwake/OriginalArt","Hushwake/Landscape","Hushwake/Meadow","Hushwake/Sky","Hushwake/Finish","Hushwake/ContactShadow"})test(Shader.Find(s)!=null,"shader "+s);
   var surface=WorldArt.TerrainMesh();test(surface.vertexCount<100000,"terrain retains a bounded rendering workload");test(surface.bounds.size.x>249&&surface.bounds.size.z>249,"terrain covers the full authored atlas");UnityEngine.Object.DestroyImmediate(surface);
   var west=WorldArt.TerrainPatch(0,0,32);var east=WorldArt.TerrainPatch(32,0,32);test(west.vertexCount==1089,"smooth terrain uses shared vertices");for(int z=0;z<=32;z++){test(Vector3.Distance(west.vertices[z*33+32],east.vertices[z*33])<.0001f,"seam position "+z);test(Vector3.Distance(west.normals[z*33+32],east.normals[z*33])<.0001f,"seam normal "+z);}UnityEngine.Object.DestroyImmediate(west);UnityEngine.Object.DestroyImmediate(east);
   foreach(var asset in new[]{"grass_ground","rock_ground_02","bark_brown_02","cobblestone_floor_02","denim_fabric","white_plaster_02"}){var texture=Resources.Load<Texture2D>("Fidelity/"+asset+"_Diffuse");test(texture!=null&&texture.width==2048&&texture.mipmapCount>1,"high resolution mipmapped albedo "+asset);test(Resources.Load<Texture2D>("Fidelity/"+asset+"_nor_gl")!=null,"high resolution normal "+asset);}
   test(Resources.Load<Texture2D>("Fidelity/tree_small_02_leaves_diff").width==2048,"photographed leaf albedo resolution");test(Resources.Load<Texture2D>("Fidelity/tree_small_02_leaves_alpha")!=null,"separate leaf silhouette mask");
   foreach(var key in new[]{"nera","mara","tree-0","house-hearth-0"}){
    var prefab=Resources.Load<GameObject>("OriginalArt/"+key);int vertices=prefab.GetComponentsInChildren<MeshFilter>().Sum(f=>f.sharedMesh.vertexCount);var instance=WorldArt.Original(key,null,Vector3.zero);
    var visible=instance.GetComponentsInChildren<MeshRenderer>().Where(r=>r.enabled).ToArray();test(visible.Sum(r=>r.GetComponent<MeshFilter>().sharedMesh.vertexCount)==vertices,"batching preserves authored geometry "+key);test(visible.Length<=prefab.GetComponentsInChildren<MeshRenderer>().Length,"batching reduces draw pieces "+key);
    if(key=="nera")test(instance.GetComponentsInChildren<Transform>().Any(t=>t.name=="leftLeg"&&t.GetComponentsInChildren<MeshRenderer>().Any(r=>r.enabled)),"animated Nera leg pivot retains visible geometry");
    if(key=="nera"||key=="mara")foreach(string joint in new[]{"knee","elbow","shoulder"})test(instance.GetComponentsInChildren<Transform>().Count(t=>t.name.StartsWith("rig:"+joint+":")&&t.GetComponentsInChildren<MeshRenderer>().Any(r=>r.enabled))==2,"both articulated "+joint+" joints preserve visible geometry "+key);
    UnityEngine.Object.DestroyImmediate(instance.gameObject);
   }
   var forest=Resources.Load<GameObject>("Fidelity/Forest/mature-tree");test(forest!=null,"bounded photographic forest model");var lods=forest.GetComponent<LODGroup>().GetLODs();test(lods.Length==2,"forest has near and distant LODs");int previous=int.MaxValue;
   foreach(var lod in lods){int triangles=lod.renderers.Sum(r=>r.GetComponent<MeshFilter>().sharedMesh.triangles.Length/3);test(triangles>0&&triangles<45000&&triangles<previous,"forest LOD budget decreases");previous=triangles;test(lod.renderers.Length==3,"three shared forest materials");foreach(var r in lod.renderers)test(r.sharedMaterial!=null&&r.sharedMaterial.shader.isSupported&&r.bounds.size.magnitude>1,"valid forest geometry and materials");}
   WorldShade.Begin();WorldShade.Tree(Vector3.zero,10);test(WorldShade.Sample(Vector3.zero).r<.8f,"tree root contact shade");test(WorldShade.Sample(new Vector3(50,0,50)).r==1,"distant terrain remains unshaded");WorldShade.Finish();WorldShade.Clear();test(WorldShade.Sample(Vector3.zero)==Color.white,"indoor transition clears forest shade");
   var ground=Resources.Load<Texture2D>("Fidelity/leafy_grass_Diffuse");test(ground!=null&&ground.width==2048,"detailed forest floor texture");
   test(CameraVisibility.Blocks(new Bounds(new Vector3(0,2,-4),Vector3.one*2),new Vector3(0,3,-10),new Vector3(0,1,0)),"foreground facade triggers camera cutaway");
   test(!CameraVisibility.Blocks(new Bounds(new Vector3(0,2,4),Vector3.one*2),new Vector3(0,3,-10),new Vector3(0,1,0)),"background facade remains visible");
   test(!CameraVisibility.Blocks(new Bounds(new Vector3(5,2,-4),Vector3.one),new Vector3(0,3,-10),new Vector3(0,1,0)),"off-axis scenery remains visible");
   var profiles=JsonUtility.FromJson<ImportOriginalArt.ProfileIndex>(Resources.Load<TextAsset>("OriginalArt/material-profiles").text);test(profiles.unlit.Length==65&&profiles.translucent.Length==2,"complete original material profiles");
   foreach(int id in profiles.unlit)test(Resources.Load<Material>("OriginalArt/material-"+id).shader.name=="Hushwake/OriginalUnlit","original unlit glow "+id);
   foreach(var entry in profiles.translucent){var m=Resources.Load<Material>("OriginalArt/material-"+entry.id);test(m.shader.name=="Hushwake/OriginalTransparent"&&Mathf.Abs(m.color.a-entry.opacity)<.001f,"original translucency "+entry.id);}
   Debug.Log("HUSHWAKE_ART_AUDIT_SUCCEEDED checks="+checks);
  }
 }
}
