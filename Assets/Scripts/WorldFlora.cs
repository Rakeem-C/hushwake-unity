using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
namespace Hushwake {
 public static class WorldFlora {
  static Material material;
  public static void Add(Transform parent,Vector3 center,List<Vector3> reserved,List<Vector3> houses,int seed){
   if(material==null){material=new Material(Shader.Find("Hushwake/OriginalArt"));material.SetFloat("_Cull",0);material.SetFloat("_Glossiness",.08f);}
   var random=new System.Random(seed+721);var vertices=new List<Vector3>();var colors=new List<Color>();var triangles=new List<int>();
   System.Action<Vector3,Vector3,Vector3,Color> face=(a,b,c,color)=>{int n=vertices.Count;vertices.AddRange(new[]{a,b,c});colors.AddRange(new[]{color.linear,color.linear,color.linear});triangles.AddRange(new[]{n,n+1,n+2});};
   for(int i=0;i<320;i++){
    float x=center.x+((float)random.NextDouble()-.5f)*40,z=center.z+((float)random.NextDouble()-.5f)*40;var root=WorldArt.Ground(x,z);
    if(reserved.Any(p=>WorldLayout.Distance(p,root)<2.2f)||houses.Any(p=>WorldLayout.Distance(p,root)<3.4f)||WorldDetail.Submerged(root))continue;
    bool flower=i%3==0;float height=.18f+(float)random.NextDouble()*.35f;var tip=root+Vector3.up*height;
    face(root+Vector3.left*.015f,root+Vector3.right*.015f,tip,new Color(.13f,.29f,.12f));
    if(flower){Color petal=i%2==0?new Color(.55f,.40f,.69f):new Color(.89f,.76f,.40f);for(int k=0;k<6;k++){float a=k*Mathf.PI/3;var direction=new Vector3(Mathf.Cos(a),0,Mathf.Sin(a));var side=Vector3.Cross(Vector3.up,direction)*.065f;var end=tip+direction*.15f+Vector3.up*.035f;face(tip,end-side,end+side,petal);face(tip+Vector3.up*.018f,tip+direction*.04f+side*.2f,tip+direction*.04f-side*.2f,new Color(.94f,.75f,.24f));}}
    else{float heading=(float)random.NextDouble()*6.28f;for(int k=0;k<8;k++){float a=heading+k*Mathf.PI*2/8;var direction=new Vector3(Mathf.Cos(a),0,Mathf.Sin(a));var spine=root+Vector3.up*.07f;for(int j=1;j<7;j++){float t=j/7f;var p=spine+direction*t*.65f+Vector3.up*Mathf.Sin(t*Mathf.PI)*.28f;var lateral=Vector3.Cross(Vector3.up,direction);float length=(1-t*.8f)*.14f;foreach(float side in new[]{-1f,1f}){var end=p+lateral*side*length+direction*.075f-Vector3.up*.04f;var ridge=Vector3.Lerp(p,end,.55f)+Vector3.up*.035f;var edge=direction*.032f;face(p,ridge,end,new Color(.17f,.36f,.10f));face(p-edge,ridge,end,new Color(.12f,.27f,.065f));face(p+edge,end,ridge,new Color(.22f,.43f,.13f));}}}}
   }
   var mesh=new Mesh{name="Fern fronds and meadow flowers",indexFormat=IndexFormat.UInt32};mesh.SetVertices(vertices);mesh.SetColors(colors);mesh.SetTriangles(triangles,0);var normals=new Vector3[vertices.Count];for(int i=0;i<normals.Length;i++)normals[i]=Vector3.up;mesh.normals=normals;mesh.RecalculateBounds();
   var obj=new GameObject("Fern fronds and meadow flowers",typeof(MeshFilter),typeof(MeshRenderer));obj.transform.SetParent(parent,false);obj.GetComponent<MeshFilter>().sharedMesh=mesh;var renderer=obj.GetComponent<MeshRenderer>();renderer.sharedMaterial=material;renderer.shadowCastingMode=ShadowCastingMode.Off;obj.AddComponent<GeneratedMeshLifetime>();
  }
 }
}
