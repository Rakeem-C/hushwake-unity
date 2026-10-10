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
   test(Resources.Load<TextAsset>("OriginalWater").bytes.Length==257*257*4,"original water bodies");
   foreach(var dungeon in Content.Load().dungeons){test(Resources.Load<GameObject>("OriginalArt/boss-"+dungeon.id)!=null,"original regional boss");foreach(var room in dungeon.rooms)test(Resources.Load<GameObject>($"OriginalArt/room-{dungeon.id}-{room.id}")!=null,"original room architecture");foreach(var puzzle in dungeon.puzzles)for(int i=0;i<puzzle.steps.Length;i++)test(Resources.Load<GameObject>($"OriginalArt/station-{puzzle.id}-{i}")!=null,"original station art");}
   foreach(var tex in new[]{"substrate","meadow"})test(Resources.Load<Texture2D>("OriginalArt/"+tex)!=null,"original ground texture "+tex);
   foreach(var s in new[]{"Hushwake/OriginalArt","Hushwake/Landscape","Hushwake/Meadow","Hushwake/Sky","Hushwake/Finish"})test(Shader.Find(s)!=null,"shader "+s);
   Debug.Log("HUSHWAKE_ART_AUDIT_SUCCEEDED checks="+checks);
  }
 }
}
