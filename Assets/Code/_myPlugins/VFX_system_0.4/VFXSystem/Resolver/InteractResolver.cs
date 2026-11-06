using JetBrains.Annotations;
using UnityEngine;

namespace VFXSystem.Resolver
{
    public static class InteractResolver
    {
        [CanBeNull]
        public static Material MaterialResolve(GameObject gameObject)
        {
            var renderer = gameObject.GetComponentInChildren<MeshRenderer>();

            if (renderer == null)
                renderer = gameObject.GetComponentInParent<MeshRenderer>();

            if (renderer != null)
            {
                Material visualMat = renderer.sharedMaterial;

                if (visualMat != null)
                { 
                    return visualMat;
                }
            }

            return null;
        }
    }
}
