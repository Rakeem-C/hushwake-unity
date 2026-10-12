using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace Hushwake.Editor {
 public static class CharacterPortraits {
  public static void Run(){EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);RenderSettings.ambientLight=new Color(.55f,.59f,.65f);RenderSettings.ambientMode=UnityEngine.Rendering.AmbientMode.Flat;RenderSettings.fog=false;
   var key=new GameObject("Portrait key",typeof(Light)).GetComponent<Light>();key.type=LightType.Directional;key.intensity=1.4f;key.transform.rotation=Quaternion.Euler(25,-30,0);
   var fill=new GameObject("Portrait fill",typeof(Light)).GetComponent<Light>();fill.type=LightType.Directional;fill.intensity=.55f;fill.color=new Color(.65f,.8f,1);fill.transform.rotation=Quaternion.Euler(20,140,0);
   var cam=new GameObject("Portrait camera",typeof(Camera)).GetComponent<Camera>();cam.clearFlags=CameraClearFlags.SolidColor;cam.backgroundColor=new Color(.085f,.105f,.125f);cam.orthographic=true;cam.orthographicSize=1.55f;cam.nearClipPlane=.1f;cam.farClipPlane=30;
   var rt=new RenderTexture(720,960,24,RenderTextureFormat.ARGB32);cam.targetTexture=rt;Directory.CreateDirectory(".qa/characters");
   foreach(string id in CharacterFinish.Keys){var actor=WorldArt.Original(id,null,Vector3.zero);float front=id=="nera"?1:-1;cam.transform.position=new Vector3(.95f,1.7f,front*5);cam.transform.LookAt(new Vector3(0,1.35f,0));cam.Render();RenderTexture.active=rt;var image=new Texture2D(720,960,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,720,960),0,0);image.Apply();File.WriteAllBytes(".qa/characters/"+id+".png",image.EncodeToPNG());Object.DestroyImmediate(image);Object.DestroyImmediate(actor.gameObject);}
   RenderTexture.active=null;cam.targetTexture=null;Object.DestroyImmediate(rt);Debug.Log("HUSHWAKE_CHARACTER_PORTRAITS_SUCCEEDED");
  }
 }
}

