using UnityEngine;
namespace Hushwake {
 [RequireComponent(typeof(Camera))]public sealed class VisualFinish:MonoBehaviour {
  Material material;long stamp;void Awake(){material=new Material(Shader.Find("Hushwake/Finish"));}
  void OnPreCull(){if(FrameDiagnostics.visible)stamp=FrameDiagnostics.Stamp();}
  void OnRenderImage(RenderTexture source,RenderTexture destination){Graphics.Blit(source,destination,material);if(FrameDiagnostics.visible)FrameDiagnostics.render=FrameDiagnostics.Milliseconds(stamp);}
  void OnDestroy(){if(material!=null)Destroy(material);}
 }
}
