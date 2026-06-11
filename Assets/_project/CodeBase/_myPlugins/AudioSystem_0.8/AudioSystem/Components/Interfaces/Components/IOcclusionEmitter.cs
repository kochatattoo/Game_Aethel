using UnityEngine;

namespace AudioSystem.Components.Interfaces.Components
{
    public interface IOcclusionEmitter
    {
        Transform EmitterTransform { get; }

        void OverridePosition(Vector3 newPosition);
        void RestoreOriginalPosition();
    }
}
