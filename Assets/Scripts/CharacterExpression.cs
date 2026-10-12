using System.Collections.Generic;
using UnityEngine;
namespace Hushwake {
 public sealed class CharacterExpression:MonoBehaviour {
  Transform face;Quaternion rest;float seed;readonly List<Transform> lids=new List<Transform>();
  void Start(){foreach(var t in GetComponentsInChildren<Transform>(true)){if(t.name.EndsWith(":face")){face=t;rest=t.localRotation;}if(t.name.Contains(":eyelid:"))lids.Add(t);}seed=0;foreach(char c in name)seed+=c*.017f;}
  void LateUpdate(){if(face==null||Camera.main==null||(Camera.main.transform.position-transform.position).sqrMagnitude>30*30)return;
   float time=Time.time+seed;float blink=Closure(time);foreach(var lid in lids){bool closed=blink>.015f;if(lid.gameObject.activeSelf!=closed)lid.gameObject.SetActive(closed);if(closed)lid.localScale=new Vector3(1,blink,1);}
   // A restrained eye-line shift and breath make idle faces alive without steering gameplay.
   face.localRotation=rest*Quaternion.Euler(Mathf.Sin(time*.83f)*1.1f,Mathf.Sin(time*.39f)*2.4f,Mathf.Sin(time*.53f)*.6f);
  }
  public static float Closure(float time){float cycle=Mathf.Repeat(time,4.7f);return cycle<.19f?Mathf.Sin(cycle/.19f*Mathf.PI):0;}
 }
}
