using UnityEngine;
namespace Hushwake {
 public sealed class RainAtmosphere:MonoBehaviour {
  Mesh mesh;Material mat;Vector3[] vertices;float[] phases;Vector2[] locations;public bool indoors;
  void Awake(){var obj=new GameObject("Fine rain",typeof(MeshFilter),typeof(MeshRenderer));obj.transform.SetParent(transform,false);mesh=new Mesh{name="Rain streaks"};vertices=new Vector3[480];phases=new float[120];locations=new Vector2[120];var uv=new Vector2[480];var tris=new int[720];var random=new System.Random(193);for(int i=0;i<120;i++){phases[i]=(float)random.NextDouble();locations[i]=new Vector2(((float)random.NextDouble()-.5f)*24,((float)random.NextDouble()-.5f)*24);int n=i*4,j=i*6;uv[n]=Vector2.zero;uv[n+1]=Vector2.right;uv[n+2]=Vector2.one;uv[n+3]=Vector2.up;tris[j]=n;tris[j+1]=n+2;tris[j+2]=n+1;tris[j+3]=n;tris[j+4]=n+3;tris[j+5]=n+2;}mesh.vertices=vertices;mesh.uv=uv;mesh.triangles=tris;mesh.bounds=new Bounds(Vector3.up*4,Vector3.one*35);obj.GetComponent<MeshFilter>().sharedMesh=mesh;mat=new Material(Shader.Find("Hushwake/Rain"));obj.GetComponent<MeshRenderer>().sharedMaterial=mat;obj.GetComponent<MeshRenderer>().shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;}
  void Update(){transform.rotation=Quaternion.identity;transform.GetChild(0).gameObject.SetActive(!indoors);if(indoors)return;for(int i=0;i<120;i++){float y=8-Mathf.Repeat(Time.time*5+phases[i]*8,8);var p=new Vector3(locations[i].x,y,locations[i].y);vertices[i*4]=p;vertices[i*4+1]=p+Vector3.right*.015f;vertices[i*4+2]=p+new Vector3(-.05f,.50f,0);vertices[i*4+3]=p+new Vector3(-.065f,.50f,0);}mesh.vertices=vertices;}
  void OnDestroy(){Destroy(mesh);Destroy(mat);}
 }
}
