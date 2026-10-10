using System.Linq;
using UnityEngine;

namespace Hushwake
{
    public sealed class CameraVisibility : MonoBehaviour
    {
        MeshRenderer[] pieces = new MeshRenderer[0]; Transform subject; float next;
        public void Track(Transform world, Transform player)
        {
            pieces = world.GetComponentsInChildren<MeshRenderer>().Where(r => r.enabled && r.GetComponent<MeshFilter>() != null && r.GetComponentInParent<ArtMotion>() == null && r.GetComponent<GeneratedMeshLifetime>() == null).ToArray();
            subject = player; next = 0;
        }
        public static bool Blocks(Bounds bounds, Vector3 camera, Vector3 focus)
        {
            var delta = focus - camera; var ray = new Ray(camera, delta.normalized);
            return bounds.IntersectRay(ray, out var distance) && distance > .15f && distance < delta.magnitude - .35f;
        }
        void LateUpdate()
        {
            if (subject == null || Time.unscaledTime < next) return; next = Time.unscaledTime + .12f;
            var origin = transform.position;
            foreach (var renderer in pieces)
            {
                if (renderer == null) continue;
                bool blocked = Blocks(renderer.bounds, origin, subject.position + Vector3.up * .4f) || Blocks(renderer.bounds, origin, subject.position + Vector3.up * 1.1f) || Blocks(renderer.bounds, origin, subject.position + Vector3.up * 1.8f);
                renderer.enabled = !blocked;
            }
        }
    }
}
