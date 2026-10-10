using UnityEngine;
namespace Hushwake {
 [RequireComponent(typeof(Camera))]public sealed class VisualFinish:MonoBehaviour {
  Material material;void Awake(){material=new Material(Shader.Find("Hushwake/Finish"));}
  void OnRenderImage(RenderTexture source,RenderTexture destination){Graphics.Blit(source,destination,material);}
  void OnDestroy(){if(material!=null)Destroy(material);}
 }
}
