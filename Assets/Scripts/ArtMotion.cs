using UnityEngine;
using System.Collections.Generic;
namespace Hushwake {
 public sealed class ArtMotion:MonoBehaviour {
  sealed class Joint {public Transform pivot;public Quaternion rest;public string kind;public float side;}
  readonly List<Joint> joints=new List<Joint>();Vector3 last;float blend,phase;
  void Start(){foreach(var t in GetComponentsInChildren<Transform>()){
   string kind=t.name.StartsWith("rig:")?t.name.Split(':')[1]:t.name.Contains(":hip:")||t.name=="leftLeg"||t.name=="rightLeg"?"hip":t.name.Contains(":sole:")?"ankle":t.name.Contains(":arm:")?"shoulder":null;
   if(kind==null)continue;
   float side=t.name.Contains("left")||t.name.Contains(":-1")?-1:1;
   joints.Add(new Joint{pivot=t,rest=t.localRotation,kind=kind,side=side});
  }last=transform.position;}
  void LateUpdate(){long stamp=FrameDiagnostics.visible?FrameDiagnostics.Stamp():0;try{Animate();}finally{if(FrameDiagnostics.visible)FrameDiagnostics.actors+=FrameDiagnostics.Milliseconds(stamp);}}
  void Animate(){float dt=Mathf.Max(Time.deltaTime,.001f);float distance=(transform.position-last).magnitude;last=transform.position;
   float speed=distance<10?distance/dt:0;blend=Mathf.Lerp(blend,Mathf.Clamp01(speed/3),1-Mathf.Exp(-dt*9));phase+=dt*(5+Mathf.Min(speed,8));
   if(Camera.main!=null&&(Camera.main.transform.position-transform.position).sqrMagnitude>40*40)return;
   foreach(var j in joints){float wave=Mathf.Sin(phase+(j.side<0?Mathf.PI:0));float hip=wave*25*blend,knee=Mathf.Max(0,-wave)*38*blend,angle=0;
    switch(j.kind){case "hip":angle=hip;break;case "knee":angle=knee;break;case "ankle":angle=-hip-knee*.65f;break;case "shoulder":angle=-wave*18*blend;break;case "elbow":angle=-8-Mathf.Max(0,wave)*19*blend;break;}
    j.pivot.localRotation=j.rest*Quaternion.Euler(angle,0,0);
   }
  }
 }
}
