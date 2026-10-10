using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
namespace Hushwake {
 public static class WorldDetail {
  static Material grass;
  static Mesh Create(List<Vector3> p,List<int> t,List<Vector2> uv,List<Color> c){var m=new Mesh{indexFormat=IndexFormat.UInt32};m.SetVertices(p);m.SetTriangles(t,0);m.SetUVs(0,uv);m.SetColors(c);m.RecalculateNormals();m.RecalculateBounds();m.RecalculateTangents();return m;}
  static void Render(Transform parent,string name,Mesh mesh,Material mat,bool shadows=true){var obj=new GameObject(name,typeof(MeshFilter),typeof(MeshRenderer));obj.transform.SetParent(parent,false);obj.GetComponent<MeshFilter>().sharedMesh=mesh;var r=obj.GetComponent<MeshRenderer>();r.sharedMaterial=mat;r.shadowCastingMode=shadows?ShadowCastingMode.On:ShadowCastingMode.Off;obj.AddComponent<GeneratedMeshLifetime>();}
  public static void Paving(Transform parent,Vector3 a,Vector3 b,float width){var p=new List<Vector3>();var t=new List<int>();var uv=new List<Vector2>();var c=new List<Color>();var side=Vector3.Cross(Vector3.up,(b-a).normalized)*width*.5f;int sections=Mathf.Max(1,Mathf.CeilToInt(Vector3.Distance(a,b)/.6f));for(int i=0;i<=sections;i++){var center=Vector3.Lerp(a,b,i/(float)sections);foreach(var sign in new[]{-1,1}){var v=center+side*sign;v.y=WorldArt.Height(v.x,v.z)+.025f;p.Add(v);uv.Add(new Vector2(sign<0?0:width,i*.6f));c.Add(Color.white);}if(i>0){int n=i*2;t.AddRange(new[]{n-2,n,n-1,n-1,n,n+1});}}Render(parent,"Weathered cobble road",Create(p,t,uv,c),Resources.Load<Material>("OriginalArt/cobble"),false);}
  public static void Meadow(Transform parent,Vector3 center,List<Vector3> reserved,List<Vector3> houses,int seed){
   if(grass==null){grass=new Material(Shader.Find("Hushwake/Meadow"));grass.SetTexture("_MainTex",Resources.Load<Texture2D>("OriginalArt/meadow"));}
   var random=new System.Random(seed);var p=new List<Vector3>();var t=new List<int>();var uv=new List<Vector2>();var c=new List<Color>();
   for(int i=0;i<1800;i++){float x=center.x+((float)random.NextDouble()-.5f)*62,z=center.z+((float)random.NextDouble()-.5f)*62;var point=WorldArt.Ground(x,z);
    if(reserved.Any(q=>WorldLayout.Distance(q,point)<2)||houses.Any(q=>WorldLayout.Distance(q,point)<3.6f)||WorldLayout.Distance(point,center)<4)continue;
    float density=Mathf.PerlinNoise(x*.24f+10,z*.24f+10);if(density<.36f)continue;
    for(int blade=0;blade<7;blade++){float a=(float)random.NextDouble()*Mathf.PI*2,h=.16f+(float)random.NextDouble()*.35f;var offset=new Vector3(((float)random.NextDouble()-.5f)*.55f,0,((float)random.NextDouble()-.5f)*.55f);var root=point+offset;root.y=WorldArt.Height(root.x,root.z);var side=new Vector3(Mathf.Cos(a),0,Mathf.Sin(a))*.045f;var lean=new Vector3(Mathf.Sin(a),0,Mathf.Cos(a))*.13f;int n=p.Count;
     p.AddRange(new[]{root-side,root+side,root+side*.5f+Vector3.up*h+lean,root-side*.5f+Vector3.up*h+lean});uv.AddRange(new[]{new Vector2(0,0),new Vector2(1,0),new Vector2(1,1),new Vector2(0,1)});var tint=Color.Lerp(new Color(.35f,.43f,.23f),new Color(.65f,.63f,.37f),(float)random.NextDouble());for(int k=0;k<4;k++)c.Add(tint);t.AddRange(new[]{n,n+2,n+1,n,n+3,n+2});
    }
   }Render(parent,"Meadow tufts and wind",Create(p,t,uv,c),grass,false);
  }
  public static void Settlement(Transform parent,Vector3 town,List<Vector3> houses){
   Paving(parent,town+Vector3.back*3,town+Vector3.forward*19,4.7f);
   foreach(var home in houses){Paving(parent,town,home+Vector3.back*2,1.8f);var pot=WorldArt.Original("prop-keg",parent,home+new Vector3(2.2f,0,-1));pot.localScale=Vector3.one*.4f;var crate=WorldArt.Original("prop-crate",parent,home+new Vector3(-2.15f,0,-1.1f));crate.localScale=Vector3.one*.5f;}
   for(int i=0;i<4;i++){var pos=town+new Vector3(i%2==0?-3:3,0,4+i*3);pos.y=WorldArt.Height(pos.x,pos.z);var lamp=WorldArt.Original("lantern",parent,pos);lamp.localScale=Vector3.one*.45f;}
  }
  public static void Chamber(Transform parent,Color hue){
   var floor=WorldArt.Shape(parent,PrimitiveType.Cube,new Vector3(0,-.31f,0),new Vector3(26,.6f,26),hue,"Original masonry chamber floor");floor.GetComponent<Renderer>().sharedMaterial=Resources.Load<Material>("OriginalArt/courtstone");
   for(int side=0;side<4;side++)for(int i=0;i<7;i++){var pos=new Vector3((i-3)*3.6f,0,12.75f);pos=Quaternion.Euler(0,side*90,0)*pos;WorldArt.Shape(parent,PrimitiveType.Cube,pos+Vector3.up*.3f,new Vector3(.65f,.65f,.8f),new Color(.34f,.37f,.34f),"Stone buttress");var lamp=WorldArt.Original("lantern",parent,pos+Vector3.up*2.8f);lamp.localScale=Vector3.one*.3f;}
   for(int i=0;i<5;i++){var stone=WorldArt.Shape(parent,PrimitiveType.Cube,new Vector3(-12.6f,.7f,i*4-8),new Vector3(.6f,1.4f,1.2f),hue,"Copper channel support");}
  }
 }
 public sealed class GeneratedMeshLifetime:MonoBehaviour {void OnDestroy(){var f=GetComponent<MeshFilter>();if(f!=null&&f.sharedMesh!=null)Destroy(f.sharedMesh);}}
}
