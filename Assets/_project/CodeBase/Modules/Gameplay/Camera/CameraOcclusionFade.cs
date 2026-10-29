using System.Collections.Generic;
using UnityEngine;

namespace CodeBase.Infrastructure.Utils
{
    [RequireComponent(typeof(Camera))]
    public class CameraOcclusionFade: MonoBehaviour
    {
        [Range(0f, 1f)] public float sphereRadius = 0.3f;

        private Transform _player;                
        private static int _layerMask;
        private readonly List<Renderer> hiddenRenderers = new List<Renderer>();

        public void Construct(Transform player)
        {
            _player = player;
            _layerMask = 1 << LayerMask.NameToLayer("WallFade");
            Debug.Log("CameraOcvclisionConstruct");
        }

        private void LateUpdate()
        {
            if (_player == null) return;

            Vector3 dir = (transform.position - _player.position).normalized;
            float dist = Vector3.Distance(transform.position, _player.position);

            for (int i = hiddenRenderers.Count - 1; i >= 0; i--)
            {
                var rend = hiddenRenderers[i];
                if (rend == null || !IsBlocking(rend, dir, dist))
                {
                    if (rend != null)
                        rend.enabled = true;
                    hiddenRenderers.RemoveAt(i);
                }
            }

            RaycastHit[] hits = Physics.SphereCastAll(_player.position, sphereRadius, dir, dist, _layerMask);
            foreach (var hit in hits)
            {
                var rend = hit.collider.GetComponent<Renderer>();
                if (rend != null && !hiddenRenderers.Contains(rend))
                {
                    rend.enabled = false;
                    hiddenRenderers.Add(rend);
                }
            }
        }

        private bool IsBlocking(Renderer rend, Vector3 dir, float dist)
        {
            if (rend == null)
                return false;

            if (Physics.Raycast(_player.position, dir, out RaycastHit hit, dist, _layerMask))
            {
                return hit.collider.GetComponent<Renderer>() == rend;
            }
            return false;
        }
    }
}
