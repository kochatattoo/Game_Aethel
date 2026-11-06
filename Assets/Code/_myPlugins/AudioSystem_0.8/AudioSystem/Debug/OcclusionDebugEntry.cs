using AudioSystem.Components.Interfaces.Components;
using UnityEngine;

namespace AudioSystem.DebugComponent
{
    public struct OcclusionDebugEntry
    {
        public IOcclusionEmitter Emitter;
        public Vector3 ListenerPos;
        public Vector3 EmitterPos;
        public float ObstructionValue;
        public bool IsActive;
        public float LastUpdateTime;
    }
}