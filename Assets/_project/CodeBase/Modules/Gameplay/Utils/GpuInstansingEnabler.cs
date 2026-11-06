using UnityEngine;

namespace CodeBase.Utils
{
    [RequireComponent(typeof(MeshRenderer))]
    public class GpuInstansingEnabler : MonoBehaviour
    {
        private void Awake()
        {
            MaterialPropertyBlock materialPropertyBlock = new();
            MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
            meshRenderer.SetPropertyBlock(materialPropertyBlock);
        }
    }
}
