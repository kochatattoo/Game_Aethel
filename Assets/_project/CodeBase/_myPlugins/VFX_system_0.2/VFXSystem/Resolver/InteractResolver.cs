using JetBrains.Annotations;
using UnityEngine;

namespace VFXSystem.Resolver
{
    public class InteractResolver
    {
        [CanBeNull]
        public Material MaterialResolve(GameObject gameObject)
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
