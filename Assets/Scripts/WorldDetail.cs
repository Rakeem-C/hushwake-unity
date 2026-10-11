using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
namespace Hushwake {
 public static class WorldDetail {
  static Material grass;
  static Material paving;
  static Texture2D waterCoverage;static Material waterMaterial;static float[] waterField;static int waterSize;
  public static bool Submerged(Vector3 point){if(waterField==null)return false;int x=Mathf.Clamp(Mathf.RoundToInt((point.x+125)/250*(waterSize-1)),0,waterSize-1),z=Mathf.Clamp(Mathf.RoundToInt((point.z+125)/250*(waterSize-1)),0,waterSize-1);return waterField[z*waterSize+x]>point.y-.04f;}
  static Mesh Create(List<Vector3> p,List<int> t,List<Vector2> uv,List<Color> c){var m=new Mesh{indexFormat=IndexFormat.UInt32};m.SetVertices(p);m.SetTriangles(t,0);m.SetUVs(0,uv);m.SetColors(c);m.RecalculateNormals();m.RecalculateBounds();m.RecalculateTangents();return m;}
  static void Render(Transform parent,string name,Mesh mesh,Material mat,bool shadows=true){var obj=new GameObject(name,typeof(MeshFilter),typeof(MeshRenderer));obj.transform.SetParent(parent,false);obj.GetComponent<MeshFilter>().sharedMesh=mesh;var r=obj.GetComponent<MeshRenderer>();r.sharedMaterial=mat;r.shadowCastingMode=shadows?ShadowCastingMode.On:ShadowCastingMode.Off;obj.AddComponent<GeneratedMeshLifetime>();}
  public static void Paving(Transform parent,Vector3 a,Vector3 b,float width){if(paving==null){paving=new Material(Resources.Load<Material>("OriginalArt/cobble"));paving.color=new Color(.58f,.62f,.53f);}var p=new List<Vector3>();var t=new List<int>();var uv=new List<Vector2>();var c=new List<Color>();var side=Vector3.Cross(Vector3.up,(b-a).normalized)*width*.5f;int sections=Mathf.Max(1,Mathf.CeilToInt(Vector3.Distance(a,b)/.6f));for(int i=0;i<=sections;i++){var center=Vector3.Lerp(a,b,i/(float)sections);foreach(var sign in new[]{-1,1}){var v=center+side*sign;v.y=WorldArt.Height(v.x,v.z)+.025f;p.Add(v);uv.Add(new Vector2(sign<0?0:width,i*.6f));c.Add(Color.white);}if(i>0){int n=i*2;t.AddRange(new[]{n-2,n,n-1,n-1,n,n+1});}}Render(parent,"Weathered cobble road",Create(p,t,uv,c),paving,false);}
  sealed class GrassTile {public readonly List<Vector3> p=new List<Vector3>();public readonly List<int> t=new List<int>();public readonly List<Vector2> uv=new List<Vector2>();public readonly List<Color> c=new List<Color>();}
  public static void Meadow(Transform parent,Vector3 center,List<Vector3> reserved,List<Vector3> houses,int seed){
   if(grass==null){grass=new Material(Shader.Find("Hushwake/Meadow"));grass.SetTexture("_MainTex",Resources.Load<Texture2D>("OriginalArt/meadow"));}
   var random=new System.Random(seed);var tiles=new Dictionary<Vector2Int,GrassTile>();
   for(int i=0;i<5600;i++){float x=center.x+((float)random.NextDouble()-.5f)*62,z=center.z+((float)random.NextDouble()-.5f)*62;var point=WorldArt.Ground(x,z);
    if(reserved.Any(q=>WorldLayout.Distance(q,point)<2)||houses.Any(q=>WorldLayout.Distance(q,point)<3.6f)||WorldLayout.Distance(point,center)<4||Submerged(point))continue;
    float density=Mathf.PerlinNoise(x*.24f+10,z*.24f+10);if(density<.36f)continue;
    var key=new Vector2Int(Mathf.FloorToInt(x/8),Mathf.FloorToInt(z/8));if(!tiles.TryGetValue(key,out var tile)){tile=new GrassTile();tiles[key]=tile;}var p=tile.p;var t=tile.t;var uv=tile.uv;var c=tile.c; for(int blade=0;blade<5;blade++){float a=(float)random.NextDouble()*Mathf.PI*2,h=.16f+(float)random.NextDouble()*.30f;var offset=new Vector3(((float)random.NextDouble()-.5f)*.55f,0,((float)random.NextDouble()-.5f)*.55f);var root=point+offset;root.y=WorldArt.Height(root.x,root.z);var side=new Vector3(Mathf.Cos(a),0,Mathf.Sin(a))*.02f;var lean=new Vector3(Mathf.Sin(a),0,Mathf.Cos(a))*.14f;int n=p.Count;var tint=Color.Lerp(new Color(.045f,.09f,.018f),new Color(.10f,.19f,.04f),(float)random.NextDouble());
     for(int segment=0;segment<=3;segment++){float f=segment/3f;var middle=root+Vector3.up*h*f+lean*f*f;var width=side*(1-f*.9f);p.AddRange(new[]{middle-width,middle+width});uv.AddRange(new[]{new Vector2(0,f),new Vector2(1,f)});c.AddRange(new[]{tint,tint});if(segment>0){int k=n+segment*2;t.AddRange(new[]{k-2,k+1,k-1,k-2,k,k+1});}}
    }
   }foreach(var tile in tiles.Values){var meadow=Create(tile.p,tile.t,tile.uv,tile.c);var normals=new Vector3[tile.p.Count];for(int i=0;i<normals.Length;i++)normals[i]=Vector3.up;meadow.normals=normals;Render(parent,"Meadow tufts and wind",meadow,grass,false);}
  }
  public static void Settlement(Transform parent,Vector3 town,List<Vector3> houses){
   Paving(parent,town+Vector3.back*3,town+Vector3.forward*19,4.7f);
   foreach(var home in houses){Paving(parent,town,home+Vector3.back*2,1.8f);var pot=WorldArt.Original("prop-keg",parent,home+new Vector3(2.2f,0,-1));pot.localScale=Vector3.one*.4f;var crate=WorldArt.Original("prop-crate",parent,home+new Vector3(-2.15f,0,-1.1f));crate.localScale=Vector3.one*.5f;}
   for(int i=0;i<4;i++){var pos=town+new Vector3(i%2==0?-3:3,0,4+i*3);pos.y=WorldArt.Height(pos.x,pos.z);var lamp=WorldArt.Original("lantern",parent,pos);lamp.localScale=Vector3.one*.45f;}
  }
  public static void Water(Transform parent){
   var bytes=Resources.Load<TextAsset>("OriginalWater").bytes;int size=Mathf.RoundToInt(Mathf.Sqrt(bytes.Length/4));var h=new float[size*size];System.Buffer.BlockCopy(bytes,0,h,0,bytes.Length);waterField=h;waterSize=size;
   if(waterCoverage==null){var pixels=new byte[size*size*4];for(int i=0;i<h.Length;i++)for(int channel=0;channel<4;channel++)pixels[i*4+channel]=h[i]<-100?(byte)0:(byte)255;waterCoverage=new Texture2D(size,size,TextureFormat.RGBA32,false,true){name="Original smooth shoreline coverage",filterMode=FilterMode.Bilinear,wrapMode=TextureWrapMode.Clamp};waterCoverage.SetPixelData(pixels,0);waterCoverage.Apply(false,true);waterMaterial=new Material(Shader.Find("Hushwake/Water"));waterMaterial.SetTexture("_Coverage",waterCoverage);}
   var p=new List<Vector3>();var tris=new List<int>();var uv=new List<Vector2>();var c=new List<Color>();float cell=250f/(size-1);
   System.Action<int,int,int> quad=(x,z,span)=>{float y=h[z*size+x];if(y<-100)return;int n=p.Count;float px=x*cell-125,pz=z*cell-125,w=cell*span;p.AddRange(new[]{new Vector3(px,y,pz),new Vector3(px+w,y,pz),new Vector3(px,y,pz+w),new Vector3(px+w,y,pz+w)});for(int i=0;i<4;i++){uv.Add(new Vector2(p[n+i].x,p[n+i].z));c.Add(Color.white);}tris.AddRange(new[]{n,n+2,n+1,n+1,n+2,n+3});};
   // Fine shoreline samples, coarse coplanar interiors: smoother banks without a million water quads.
   for(int z=0;z<size-1;z+=4)for(int x=0;x<size-1;x+=4){float a=h[z*size+x],b=h[z*size+x+4],d=h[(z+4)*size+x],e=h[(z+4)*size+x+4],mid=h[(z+2)*size+x+2];if(a>-100&&Mathf.Abs(a-b)<.01f&&Mathf.Abs(a-d)<.01f&&Mathf.Abs(a-e)<.01f&&Mathf.Abs(a-mid)<.01f)quad(x,z,4);else for(int dz=0;dz<4;dz++)for(int dx=0;dx<4;dx++)quad(x+dx,z+dz,1);}
   Render(parent,"Original river lakes and coast",Create(p,tris,uv,c),waterMaterial,false);
  }
  public static void Chamber(Transform parent,Color hue,string dungeon,string room){
   var floor=WorldArt.Shape(parent,PrimitiveType.Cube,new Vector3(0,-.31f,0),new Vector3(26,.6f,26),hue,"Original masonry chamber floor");floor.GetComponent<Renderer>().sharedMaterial=Resources.Load<Material>("OriginalArt/courtstone");
   var mat=new Material(floor.GetComponent<Renderer>().sharedMaterial);mat.mainTextureScale=Vector2.one*10;floor.GetComponent<Renderer>().sharedMaterial=mat;
   var original=WorldArt.Original("room-"+dungeon+"-"+room,parent,Vector3.zero);original.localScale=Vector3.one*1.75f;
   foreach(var t in original.GetComponentsInChildren<Transform>())if(t.name.StartsWith("neighbor-facade:")||(t.name.StartsWith("facade-sector:")&&t.name.EndsWith(":2")))t.gameObject.SetActive(false);
  }
 }
 public sealed class GeneratedMeshLifetime:MonoBehaviour {void OnDestroy(){var f=GetComponent<MeshFilter>();if(f!=null&&f.sharedMesh!=null)Destroy(f.sharedMesh);}}
}
