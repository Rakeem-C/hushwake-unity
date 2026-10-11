using UnityEngine;
namespace Hushwake {
 // Soft ground contact survives the balanced lighting profile. Rebuilt for each outdoor world.
 public static class WorldShade {
  const int Size=512;static Color[] pixels;static Texture2D texture;
  public static void Clear(){pixels=null;Shader.SetGlobalTexture("_GroundShade",Texture2D.whiteTexture);}
  public static void Begin(){Clear();pixels=new Color[Size*Size];for(int i=0;i<pixels.Length;i++)pixels[i]=Color.white;}
  static void Stamp(Vector3 center,float rx,float rz,float contact,float shade,bool broken){
   if(pixels==null)return;
   int minX=Mathf.Clamp(Mathf.FloorToInt((center.x-rx+125)/250*Size),0,Size-1),maxX=Mathf.Clamp(Mathf.CeilToInt((center.x+rx+125)/250*Size),0,Size-1);
   int minZ=Mathf.Clamp(Mathf.FloorToInt((center.z-rz+125)/250*Size),0,Size-1),maxZ=Mathf.Clamp(Mathf.CeilToInt((center.z+rz+125)/250*Size),0,Size-1);
   for(int z=minZ;z<=maxZ;z++)for(int x=minX;x<=maxX;x++){
    float wx=(x+.5f)/Size*250-125,wz=(z+.5f)/Size*250-125,d=Mathf.Pow((wx-center.x)/rx,2)+Mathf.Pow((wz-center.z)/rz,2);if(d>=1)continue;
    float soft=(1-d)*(1-d);if(broken)soft*=.35f+.65f*Mathf.PerlinNoise(wx*1.1f+120,wz*1.1f+120);
    int i=z*Size+x;var p=pixels[i];p.r=Mathf.Min(p.r,1-contact*soft);p.g=Mathf.Min(p.g,1-shade*soft);pixels[i]=p;
   }
  }
  public static void Tree(Vector3 point,float height){Stamp(point,1.6f,1.6f,.48f,.15f,false);Stamp(point+new Vector3(-height*.22f,0,height*.35f),height*.32f,height*.42f,.12f,.38f,true);}
  public static void House(Vector3 point){Stamp(point,3.6f,3.6f,.4f,.2f,false);Stamp(point+new Vector3(-2,0,3),4,4,.12f,.3f,false);}
  public static Color Sample(Vector3 p){if(pixels==null)return Color.white;int x=Mathf.Clamp((int)((p.x+125)/250*Size),0,Size-1),z=Mathf.Clamp((int)((p.z+125)/250*Size),0,Size-1);return pixels[z*Size+x];}
  public static void Finish(){if(pixels==null)return;if(texture==null)texture=new Texture2D(Size,Size,TextureFormat.RGBA32,true,true){name="Soft forest ground shade",wrapMode=TextureWrapMode.Clamp,filterMode=FilterMode.Bilinear};texture.SetPixels(pixels);texture.Apply(true,false);Shader.SetGlobalTexture("_GroundShade",texture);}
 }
}
