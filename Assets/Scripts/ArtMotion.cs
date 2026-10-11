using UnityEngine;
namespace Hushwake {
 public sealed class ArtMotion:MonoBehaviour {
  Transform[] hips,arms;Quaternion[] hipBase,armBase;Vector3 last;float blend,phase;
  void Start(){var all=GetComponentsInChildren<Transform>();var legs=new System.Collections.Generic.List<Transform>();var upper=new System.Collections.Generic.List<Transform>();foreach(var t in all){if(t.name.Contains(":hip:")||t.name=="leftLeg"||t.name=="rightLeg")legs.Add(t);if(t.name.Contains("upper-arm")||t.name.Contains(":arm:"))upper.Add(t);}hips=legs.ToArray();arms=upper.ToArray();hipBase=new Quaternion[hips.Length];armBase=new Quaternion[arms.Length];for(int i=0;i<hips.Length;i++)hipBase[i]=hips[i].localRotation;for(int i=0;i<arms.Length;i++)armBase[i]=arms[i].localRotation;last=transform.position;}
  void LateUpdate(){long stamp=FrameDiagnostics.visible?FrameDiagnostics.Stamp():0;try{Animate();}finally{if(FrameDiagnostics.visible)FrameDiagnostics.actors+=FrameDiagnostics.Milliseconds(stamp);}}
  void Animate(){var position=transform.position;var speed=(position-last).magnitude/Mathf.Max(Time.deltaTime,.001f);last=position;if(speed<.001f&&blend<.001f)return;blend=Mathf.Lerp(blend,Mathf.Clamp01(speed/3),Time.deltaTime*9);phase+=Time.deltaTime*(5+speed);for(int i=0;i<hips.Length;i++)hips[i].localRotation=hipBase[i]*Quaternion.Euler(Mathf.Sin(phase+(i%2)*Mathf.PI)*24*blend,0,0);for(int i=0;i<arms.Length;i++)arms[i].localRotation=armBase[i]*Quaternion.Euler(Mathf.Sin(phase+(i%2)*Mathf.PI)*15*blend,0,0);}
 }
}
