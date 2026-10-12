using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Hushwake.Editor {
 // Authored locally: preserve costume identities and joint hierarchies, replace the face surface.
 public static class CharacterFinish {
  public static readonly string[] Keys={"nera","mara","tavi","imri","sava","oya","bren","sel","rohn","pella","isca","vessa","eiren"};
  const string Folder="Assets/Resources/Characters";
  static void Asset(UnityEngine.Object value,string path){var old=AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);if(old==null)AssetDatabase.CreateAsset(value,path);else{EditorUtility.CopySerialized(value,old);UnityEngine.Object.DestroyImmediate(value);}}
  static Color Hex(int n)=>new Color(((n>>16)&255)/255f,((n>>8)&255)/255f,(n&255)/255f).linear;
  static Mesh Refine(Mesh original){
   // PN-style edge interpolation rounds an authored curved surface without moving its original corners.
   var vertices=original.vertices;var normals=original.normals;var colors=original.colors;var uv=original.uv;var indices=original.triangles;
   var p=new List<Vector3>(vertices);var n=new List<Vector3>(normals);var c=new List<Color>(colors);var u=new List<Vector2>(uv);var edges=new Dictionary<ulong,int>();var tris=new List<int>();
   if(c.Count!=p.Count){c.Clear();for(int i=0;i<p.Count;i++)c.Add(Color.white);}if(u.Count!=p.Count){u.Clear();for(int i=0;i<p.Count;i++)u.Add(Vector2.zero);}
   Func<int,int,int> edge=(a,b)=>{ulong key=((ulong)Mathf.Min(a,b)<<32)|(uint)Mathf.Max(a,b);if(edges.TryGetValue(key,out int found))return found;
    var d=vertices[b]-vertices[a];var middle=(vertices[a]+vertices[b])*.5f;float smooth=Vector3.Dot(normals[a],normals[b])>.55f?1:0;
    middle-=(Vector3.Dot(d,normals[a])*normals[a]-Vector3.Dot(d,normals[b])*normals[b])*.125f*smooth;
    int added=p.Count;p.Add(middle);n.Add((normals[a]+normals[b]).normalized);c.Add((c[a]+c[b])*.5f);u.Add((u[a]+u[b])*.5f);edges[key]=added;return added;};
   for(int i=0;i<indices.Length;i+=3){int a=indices[i],b=indices[i+1],d=indices[i+2],ab=edge(a,b),bd=edge(b,d),da=edge(d,a);tris.AddRange(new[]{a,ab,da,ab,b,bd,da,bd,d,ab,bd,da});}
   var result=new Mesh{name=original.name+" curved refinement",indexFormat=UnityEngine.Rendering.IndexFormat.UInt32};result.SetVertices(p);result.SetNormals(n);result.SetColors(c);result.SetUVs(0,u);result.SetTriangles(tris,0);result.RecalculateTangents();result.RecalculateBounds();return result;
  }
  static Mesh Head(float front,int seed,bool aged,string key){
   // Continuous brow, cheek, jaw and chin planes, with recessed sockets and a soft temple.
   float[,] rings={{-.294f,.012f,.016f,.027f},{-.282f,.082f,.073f,.031f},{-.25f,.137f,.115f,.031f},{-.209f,.176f,.146f,.025f},{-.16f,.201f,.174f,.016f},{-.107f,.224f,.197f,.005f},{-.051f,.234f,.210f,0},{.004f,.236f,.213f,-.003f},{.06f,.234f,.215f,-.008f},{.113f,.223f,.207f,-.014f},{.166f,.207f,.194f,-.019f},{.213f,.18f,.17f,-.022f},{.255f,.136f,.132f,-.024f},{.287f,.08f,.082f,-.024f},{.306f,.008f,.014f,-.024f}};
   const int sides=96,rows=70;var p=new List<Vector3>();var uv=new List<Vector2>();var c=new List<Color>();var tri=new List<int>();
   float width=1+(seed%5-2)*.012f;
   for(int row=0;row<=rows;row++){float t=row/(float)rows*(rings.GetLength(0)-1);int lo=Mathf.Min((int)t,rings.GetLength(0)-2);float f=t-lo;
    Func<int,float> profile=column=>{float a=rings[Mathf.Max(0,lo-1),column],b=rings[lo,column],d=rings[lo+1,column],e=rings[Mathf.Min(rings.GetLength(0)-1,lo+2),column];return .5f*((2*b)+(-a+d)*f+(2*a-5*b+4*d-e)*f*f+(-a+3*b-3*d+e)*f*f*f);};
    float y=Mathf.Lerp(rings[lo,0],rings[lo+1,0],f),rx=profile(1),rz=profile(2),oz=profile(3);
    float jaw=Mathf.Exp(-Mathf.Pow((y+.20f)/.10f,2));rx*=1+jaw*(key=="nera"?-.085f:key=="mara"?.075f:key=="rohn"?.12f:(seed%7-3)*.023f);
    for(int i=0;i<=sides;i++){float a=i/(float)sides*Mathf.PI*2,ahead=Mathf.Max(0,Mathf.Sin(a)),x=Mathf.Cos(a)*rx*width;
     float cheek=Mathf.Exp(-((Mathf.Abs(x)-.15f)*(Mathf.Abs(x)-.15f)/.0025f+(y+.058f)*(y+.058f)/.0036f))*ahead;
     float socket=Mathf.Exp(-((Mathf.Abs(x)-.095f)*(Mathf.Abs(x)-.095f)/.0018f+(y-.035f)*(y-.035f)/.0012f))*ahead;
     float bridge=Mathf.Exp(-Mathf.Pow(x/.028f,2)-Mathf.Pow((y+.003f)/.072f,2))*.031f;
     float tip=Mathf.Exp(-Mathf.Pow(x/(.032f+seed%3*.003f),2)-Mathf.Pow((y+.052f)/.028f,2))*(.049f+seed%4*.004f);
     float wings=Mathf.Exp(-Mathf.Pow((Mathf.Abs(x)-.032f)/.020f,2)-Mathf.Pow((y+.064f)/.018f,2))*.020f;
     float mouth=Mathf.Exp(-Mathf.Pow(x/.044f,4)-Mathf.Pow((y+.154f)/.021f,2));
     float z=Mathf.Sin(a)*rz+oz+cheek*.011f-socket*.010f+(bridge+tip+wings+mouth*.009f)*ahead;
     float age=aged?Mathf.Exp(-Mathf.Pow((y+.015f)/.024f,2))*Mathf.Exp(-Mathf.Pow((Mathf.Abs(x)-.167f)/.02f,2))*ahead:0;
     z-=age*.0025f; p.Add(new Vector3(x,y,front*z));uv.Add(new Vector2(i/(float)sides,row/(float)rows));
     float stubble=key=="rohn"?Mathf.SmoothStep(0,.09f,-y-.105f)*ahead*.12f:0;
     c.Add(new Color(1-stubble,.985f-cheek*.055f-mouth*.14f-stubble,.965f-cheek*.085f-mouth*.11f-stubble));
     if(row>0&&i<sides){int b=row*(sides+1)+i,h=b-sides-1;if(front>0)tri.AddRange(new[]{h,b,h+1,b,b+1,h+1});else tri.AddRange(new[]{h,h+1,b,b,h+1,b+1});}
    }
   }
   var m=new Mesh{name="Sculpted continuous facial planes"};m.SetVertices(p);m.SetUVs(0,uv);m.SetColors(c);m.SetTriangles(tri,0);m.RecalculateNormals();m.RecalculateTangents();m.RecalculateBounds();return m;
  }
  static Texture2D Skin(){const int size=1024;var t=new Texture2D(size,size,TextureFormat.RGB24,true){name="Subtle skin pigmentation",wrapMode=TextureWrapMode.Repeat};var pixels=new Color[size*size];
   for(int y=0;y<size;y++)for(int x=0;x<size;x++){float n=Mathf.PerlinNoise(x*.14f,y*.14f)*.018f+Mathf.PerlinNoise(x*.027f,y*.027f)*.012f;pixels[y*size+x]=new Color(.98f+n,.965f+n,.955f+n);}t.SetPixels(pixels);t.Apply(true,false);return t;
  }
  static Texture2D Weave(){const int size=1024;var t=new Texture2D(size,size,TextureFormat.RGB24,true){name="Fine undyed garment weave",wrapMode=TextureWrapMode.Repeat,anisoLevel=8};var pixels=new Color[size*size];for(int y=0;y<size;y++)for(int x=0;x<size;x++){float tone=.975f+.008f*Mathf.Sin(x*Mathf.PI*.5f)+.008f*Mathf.Cos(y*Mathf.PI*.5f)+.006f*Mathf.PerlinNoise(x*.09f,y*.09f);pixels[y*size+x]=new Color(tone,tone,tone);}t.SetPixels(pixels);t.Apply(true,false);return t;}
  static GameObject Orb(Transform parent,string name,Vector3 at,Vector3 size,Material mat){var obj=GameObject.CreatePrimitive(PrimitiveType.Sphere);obj.name=name;obj.transform.SetParent(parent,false);obj.transform.localPosition=at;obj.transform.localScale=size;UnityEngine.Object.DestroyImmediate(obj.GetComponent<Collider>());obj.GetComponent<MeshRenderer>().sharedMaterial=mat;return obj;}
  static void Curve(Transform parent,string name,Vector3[] points,float width,float depth,Material mat,string assetName){
   var p=new List<Vector3>();var uv=new List<Vector2>();var c=new List<Color>();var tris=new List<int>();const int steps=28,sides=10;
   for(int j=0;j<=steps;j++){float t=j/(float)steps;Vector3 center=Bezier(points,t),tangent=(Bezier(points,Mathf.Min(1,t+.005f))-Bezier(points,Mathf.Max(0,t-.005f))).normalized;
    Vector3 right=Vector3.Cross(tangent,Vector3.forward).normalized;if(right.sqrMagnitude<.1f)right=Vector3.right;Vector3 normal=Vector3.Cross(right,tangent).normalized;
    float taper=.18f+.82f*Mathf.Pow(Mathf.Max(0,Mathf.Sin(t*Mathf.PI)),.35f);
    for(int i=0;i<=sides;i++){float a=i/(float)sides*Mathf.PI*2;p.Add(center+right*Mathf.Cos(a)*width*taper+normal*Mathf.Sin(a)*depth*taper);uv.Add(new Vector2(i/(float)sides,t));c.Add(Color.white);if(j>0&&i<sides){int b=j*(sides+1)+i,h=b-sides-1;tris.AddRange(new[]{h,b,h+1,b,b+1,h+1});}}
   }
   var mesh=new Mesh{name=name};mesh.SetVertices(p);mesh.SetUVs(0,uv);mesh.SetColors(c);mesh.SetTriangles(tris,0);mesh.RecalculateNormals();mesh.RecalculateTangents();mesh.RecalculateBounds();string path=Folder+"/"+assetName+".asset";Asset(mesh,path);var obj=new GameObject(name,typeof(MeshFilter),typeof(MeshRenderer));obj.transform.SetParent(parent,false);obj.GetComponent<MeshFilter>().sharedMesh=AssetDatabase.LoadAssetAtPath<Mesh>(path);obj.GetComponent<MeshRenderer>().sharedMaterial=mat;
  }
  static Vector3 Bezier(Vector3[] p,float t){float a=1-t;return a*a*a*p[0]+3*a*a*t*p[1]+3*a*t*t*p[2]+t*t*t*p[3];}
  static Mesh HairCap(float front){var p=new List<Vector3>();var uv=new List<Vector2>();var c=new List<Color>();var tris=new List<int>();const int sides=64,rows=20;
   for(int j=0;j<=rows;j++)for(int i=0;i<=sides;i++){float a=i/(float)sides*Mathf.PI*2;float facing=Mathf.Max(0,Mathf.Sin(a));float theta=Mathf.Lerp(.012f,1.80f-facing*.67f,j/(float)rows);p.Add(new Vector3(Mathf.Cos(a)*Mathf.Sin(theta)*.248f,Mathf.Cos(theta)*.318f,front*(Mathf.Sin(a)*Mathf.Sin(theta)*.228f-.018f)));uv.Add(new Vector2(i/(float)sides,j/(float)rows));c.Add(Color.white);if(j>0&&i<sides){int b=j*(sides+1)+i,h=b-sides-1;if(front>0)tris.AddRange(new[]{h,h+1,b,b,h+1,b+1});else tris.AddRange(new[]{h,b,h+1,b,b+1,h+1});}}
   var mesh=new Mesh{name="Continuous coiffure shell"};mesh.SetVertices(p);mesh.SetUVs(0,uv);mesh.SetColors(c);mesh.SetTriangles(tris,0);mesh.RecalculateNormals();mesh.RecalculateTangents();mesh.RecalculateBounds();return mesh;
  }
  static void SculptHair(Transform face,string key,float front,Color color,int seed){var hair=Material(key+"-sculpted-hair",color,.30f);hair.shader=Shader.Find("Hushwake/Hair");EditorUtility.SetDirty(hair);var shine=Material(key+"-hair-highlight",Color.Lerp(color,Hex(0xa8b8bc),.13f),.34f);shine.shader=Shader.Find("Hushwake/Hair");EditorUtility.SetDirty(shine);
   string capPath=Folder+"/"+key+"-hair-cap.asset";Asset(HairCap(front),capPath);var cap=new GameObject("Sculpted hair mass",typeof(MeshFilter),typeof(MeshRenderer));cap.transform.SetParent(face,false);cap.GetComponent<MeshFilter>().sharedMesh=AssetDatabase.LoadAssetAtPath<Mesh>(capPath);cap.GetComponent<MeshRenderer>().sharedMaterial=hair;
   for(int i=0;i<18;i++){float x=(i-8.5f)*.022f;float sweep=key=="nera"?.10f:key=="mara"?-.075f:.055f;float irregular=Mathf.Sin(i*2.1f+seed)*.009f;Curve(face,"Swept layered lock",new[]{new Vector3(x-.08f,.275f,front*-.10f),new Vector3(x-.09f,.332f+irregular,front*.13f),new Vector3(x+sweep,.21f,front*.24f),new Vector3(Mathf.Clamp(x+sweep*1.25f,-.245f,.245f),.115f+Mathf.Abs(x)*.24f+irregular,front*.213f)},.014f,.005f,i%4==0?shine:hair,key+"-lock-"+i);}
   for(int side=-1;side<=1;side+=2)for(int i=0;i<4;i++){float z=-.095f+i*.06f;bool longHair=key=="nera"||key=="pella"||key=="eiren";Curve(face,"Face framing tress",new[]{new Vector3(side*.18f,.22f,front*z),new Vector3(side*.265f,.11f,front*z),new Vector3(side*.26f,-.15f,front*(z+.018f)),new Vector3(side*.21f,longHair?-.30f:-.10f,front*(z-.02f))},.030f,.015f,i==1?shine:hair,key+"-side-"+side+"-"+i);}
   if(key=="mara"||key=="tavi"||key=="vessa"||key=="isca"){
    Orb(face,"Coiled hair knot",new Vector3(.035f,.19f,front*-.235f),new Vector3(.17f,.16f,.12f),hair);
    for(int i=0;i<8;i++)Orb(face,"Interwoven braid",new Vector3(.225f+Mathf.Sin(i*2.5f)*.008f,.04f-i*.045f,front*-.13f),new Vector3(.072f-i*.003f,.058f,.063f),i%3==0?shine:hair);
   }
  }
  static Material Material(string key,Color color,float gloss,float metal=0){string path=Folder+"/"+key+".mat";var m=new Material(Shader.Find("Hushwake/OriginalArt")){color=color};m.SetFloat("_Glossiness",gloss);m.SetFloat("_Metallic",metal);Asset(m,path);return AssetDatabase.LoadAssetAtPath<Material>(path);}
  public static void Run(){Directory.CreateDirectory(Folder);AssetDatabase.Refresh();Asset(Skin(),Folder+"/skin-pigmentation.asset");Asset(Weave(),Folder+"/garment-weave.asset");var weave=AssetDatabase.LoadAssetAtPath<Texture2D>(Folder+"/garment-weave.asset");var skin=AssetDatabase.LoadAssetAtPath<Texture2D>(Folder+"/skin-pigmentation.asset");
   foreach(string key in Keys){var source=Resources.Load<GameObject>("OriginalArt/"+key);var obj=UnityEngine.Object.Instantiate(source);obj.name=key;int seed=key.Sum(ch=>(int)ch);bool aged=key=="mara"||key=="sel"||key=="eiren";
    var face=obj.GetComponentsInChildren<Transform>().First(t=>t.name.EndsWith(":face"));float front=key=="nera"?1:-1;
    var head=face.GetComponentsInChildren<MeshFilter>().First(f=>f.name.Contains("sculpted-head"));Asset(Head(front,seed,aged,key),Folder+"/"+key+"-head.asset");head.sharedMesh=AssetDatabase.LoadAssetAtPath<Mesh>(Folder+"/"+key+"-head.asset");
    var headMat=new Material(head.GetComponent<MeshRenderer>().sharedMaterial){name=key+" skin"};headMat.mainTexture=skin;headMat.mainTextureScale=Vector2.one;headMat.SetFloat("_Glossiness",.28f);headMat.SetFloat("_HasNormal",0);Asset(headMat,Folder+"/"+key+"-skin.mat");head.GetComponent<MeshRenderer>().sharedMaterial=AssetDatabase.LoadAssetAtPath<Material>(Folder+"/"+key+"-skin.mat");
    // Hair follows the face pivot rather than remaining fixed when the resident looks toward Nera.
    var names=JsonUtility.FromJson<FidelityImport.Names>(Resources.Load<TextAsset>("Fidelity/material-names").text).names;
    var hairMaterials=new Dictionary<Material,Material>();var fabricMaterials=new Dictionary<Material,Material>();int refined=0;Color hairColor=Color.gray;
    foreach(var r in obj.GetComponentsInChildren<MeshRenderer>()){
     var old=r.sharedMaterial;string authored=old.name;if(authored.StartsWith("material-")&&int.TryParse(authored.Substring(9),out int id))authored=names[id];
     bool organic=authored.Contains("character:")&&(authored.Contains("hair")||authored.Contains("skin")||authored.Contains("cloth")||authored.Contains("leather"));
     var filter=r.GetComponent<MeshFilter>();
     if(authored.Contains("character:")&&authored.Contains("cloth")){if(!fabricMaterials.TryGetValue(old,out var fabric)){var cloth=new Material(old);cloth.mainTexture=weave;cloth.mainTextureScale=Vector2.one*3;cloth.SetFloat("_HasNormal",.22f);cloth.SetFloat("_Glossiness",.10f);cloth.SetFloat("_HasRoughness",0);string clothPath=Folder+"/"+key+"-fabric-"+fabricMaterials.Count+".mat";Asset(cloth,clothPath);fabric=AssetDatabase.LoadAssetAtPath<Material>(clothPath);fabricMaterials[old]=fabric;}r.sharedMaterial=fabric;}
     if(organic&&!authored.Contains("hair")&&!filter.name.Contains("facial-detail")&&filter!=head&&filter.sharedMesh.triangles.Length<12000){string meshPath=Folder+"/"+key+"-curved-"+(refined++)+".asset";Asset(Refine(filter.sharedMesh),meshPath);filter.sharedMesh=AssetDatabase.LoadAssetAtPath<Mesh>(meshPath);}
     if(!authored.Contains("character:hair:"))continue;hairColor=old.color;
     if(!hairMaterials.TryGetValue(old,out var updated)){var mat=new Material(old);mat.shader=Shader.Find("Hushwake/Hair");mat.SetFloat("_Glossiness",.32f);string path=Folder+"/"+key+"-hair-"+hairMaterials.Count+".mat";Asset(mat,path);updated=AssetDatabase.LoadAssetAtPath<Material>(path);hairMaterials[old]=updated;}r.sharedMaterial=updated;
     r.gameObject.SetActive(false);
    }
    foreach(var f in face.GetComponentsInChildren<MeshFilter>().Where(f=>f.name.Contains("facial-detail")).ToArray())f.gameObject.SetActive(false);
    int[] hairPalette={0x2b3b45,0xd9d8c9,0x302b32,0x34363e,0x4e3b32,0x282d30,0x513d32,0x717476,0x393638,0x2c2d34,0x5e4c3c,0x42363e,0xb4ad9c};hairColor=Hex(hairPalette[Array.IndexOf(Keys,key)]);SculptHair(face,key,front,hairColor,seed);
    var skinDetail=Material(key+"-skin-detail",head.GetComponent<MeshRenderer>().sharedMaterial.color,.28f);var brow=Material(key+"-brow",Color.Lerp(hairColor,Hex(0x303032),.25f),.12f);
    for(int side=-1;side<=1;side+=2){Orb(face,"Shaped ear",new Vector3(side*.235f,-.025f,-front*.012f),new Vector3(.080f,.135f,.065f),skinDetail);Curve(face,"Expressive brow",new[]{new Vector3(side*.046f,.090f,front*.215f),new Vector3(side*.078f,.111f,front*.226f),new Vector3(side*.122f,.108f,front*.211f),new Vector3(side*.150f,.087f,front*.183f)},.0055f,.004f,brow,key+"-brow-"+side);
     Curve(face,"Upper eyelid contour",new[]{new Vector3(side*.049f,.038f,front*.216f),new Vector3(side*.076f,.064f,front*.239f),new Vector3(side*.117f,.062f,front*.232f),new Vector3(side*.142f,.039f,front*.211f)},.006f,.005f,skinDetail,key+"-upper-lid-"+side);
     Curve(face,"Lower eyelid contour",new[]{new Vector3(side*.049f,.029f,front*.216f),new Vector3(side*.077f,.010f,front*.233f),new Vector3(side*.116f,.010f,front*.230f),new Vector3(side*.142f,.029f,front*.211f)},.0045f,.004f,skinDetail,key+"-lower-lid-"+side);
    }
    var lip=Material(key+"-mouth-line",Color.Lerp(skinDetail.color,Hex(0x69463d),.50f),.16f);Curve(face,"Quiet mouth expression",new[]{new Vector3(-.047f,-.157f,front*.201f),new Vector3(-.018f,-.162f,front*.214f),new Vector3(.022f,-.160f,front*.214f),new Vector3(.047f,-.155f,front*.201f)},.0018f,.0018f,lip,key+"-mouth");
    foreach(var lid in face.GetComponentsInChildren<Transform>().Where(t=>t.name.Contains(":eyelid:")).ToArray()){lid.gameObject.SetActive(false);lid.localScale=Vector3.one;}
    // Character-specific crafted fasteners catch light without replacing the established outfits.
    var brass=Material("antique-brass",Hex(0xb99758),.46f,.72f);var enamel=Material(key+"-enamel",Color.HSVToRGB((seed%9)/12f,.36f,.43f).linear,.32f);
    var torso=obj.transform;var brooch=Orb(torso,"Tailored character crest",new Vector3(-.17f,1.70f,front*.24f),new Vector3(.066f,.078f,.018f),brass);Orb(brooch.transform,"Enamel inset",new Vector3(0,0,front*.43f),new Vector3(.64f,.60f,.35f),enamel);
    // Readable small details: inset rim, engraved central stud and handmade cuff rivets.
    Orb(brooch.transform,"Engraved center",new Vector3(0,0,front*.67f),new Vector3(.15f,.17f,.12f),brass);
    foreach(var f in obj.GetComponentsInChildren<MeshFilter>().Where(f=>f.name.Contains("forearm")||f.name.Contains("hand-v3"))){if(f.name.EndsWith(":0"))Orb(f.transform.parent,"Cuff fastener",new Vector3(.042f,-.30f,.06f),new Vector3(.016f,.016f,.007f),brass);}
    PrefabUtility.SaveAsPrefabAsset(obj,Folder+"/"+key+".prefab");UnityEngine.Object.DestroyImmediate(obj);
   }
   AssetDatabase.SaveAssets();var used=new HashSet<string>();foreach(string key in Keys)foreach(string path in AssetDatabase.GetDependencies(Folder+"/"+key+".prefab",true))used.Add(path);
   foreach(string path in Directory.GetFiles(Folder,"*-curved-*.asset"))if(!used.Contains(path.Replace('\\','/')))AssetDatabase.DeleteAsset(path);
   AssetDatabase.Refresh();Debug.Log("HUSHWAKE_CHARACTER_FINISH_SUCCEEDED characters="+Keys.Length);
  }
 }
}

