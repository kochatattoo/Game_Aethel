using UnityEngine;

namespace AudioSystem.Components.Interfaces.Components
{
    public interface IPortalComponent
    {
        float CalculateDistance(Vector3 position);
        public float CalculateWeight(Vector3 position);
    }
}
