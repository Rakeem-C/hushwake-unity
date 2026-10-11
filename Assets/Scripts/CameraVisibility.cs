using System.Linq;
using UnityEngine;

namespace Hushwake
{
    public sealed class CameraVisibility : MonoBehaviour
    {
        sealed class Piece {public MeshRenderer renderer;public Bounds bounds;public bool visible,cutaway;public float rangeSquared;}
        Piece[] pieces = new Piece[0]; Transform subject; float next;
        public void Track(Transform world, Transform player)
        {
            pieces = world.GetComponentsInChildren<MeshRenderer>().Where(r => r.enabled && r.GetComponent<MeshFilter>() != null).Select(r=>{
                var bounds=r.bounds;bool animated=r.GetComponentInParent<ArtMotion>()!=null,generated=r.GetComponent<GeneratedMeshLifetime>()!=null;
                bool atlas=r.name=="Landscape"||r.name=="Original river lakes and coast";bool meadow=r.name=="Meadow tufts and wind";
                float range=animated?35:meadow?70:bounds.size.sqrMagnitude<25?38:80+bounds.extents.magnitude;
                return new Piece{renderer=r,bounds=bounds,visible=true,cutaway=!animated&&!atlas&&(!generated||bounds.size.sqrMagnitude<25),rangeSquared=atlas?float.MaxValue:range*range};
            }).ToArray();
            subject = player; next = 0;
        }
        public static bool Blocks(Bounds bounds, Vector3 camera, Vector3 focus)
        {
            var delta=focus-camera;float near=0,far=1;var min=bounds.min;var max=bounds.max;
            for(int axis=0;axis<3;axis++){
                if(Mathf.Abs(delta[axis])<.00001f){if(camera[axis]<min[axis]||camera[axis]>max[axis])return false;continue;}
                float a=(min[axis]-camera[axis])/delta[axis],b=(max[axis]-camera[axis])/delta[axis];near=Mathf.Max(near,Mathf.Min(a,b));far=Mathf.Min(far,Mathf.Max(a,b));if(near>far)return false;
            }
            float distance=delta.magnitude;return near*distance>.15f&&near*distance<distance-.35f;
        }
        void LateUpdate()
        {
            if (subject == null || Time.unscaledTime < next) return; next = Time.unscaledTime + .12f;
            var origin = transform.position;
            foreach (var piece in pieces)
            {
                if (piece.renderer == null) continue;
                bool visible=(piece.bounds.center-subject.position).sqrMagnitude<piece.rangeSquared;
                if(visible&&piece.cutaway)visible=!(Blocks(piece.bounds, origin, subject.position + Vector3.up * .4f) || Blocks(piece.bounds, origin, subject.position + Vector3.up * 1.1f) || Blocks(piece.bounds, origin, subject.position + Vector3.up * 1.8f));
                if(piece.visible!=visible){piece.renderer.enabled=visible;piece.visible=visible;}
            }
        }
    }
}
