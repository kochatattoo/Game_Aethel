using AudioSystem.Components.Interfaces.Components;
using AudioSystem.DebugComponent;
using System.Collections.Generic;

namespace Infrastructure.AudioSystem.Components.Occlusions
{
    public interface IOcclusionService
    {
        void RegisterEmitter(IOcclusionEmitter emitter);
        void UnregisterEmitter(IOcclusionEmitter emitter);
#if UNITY_EDITOR
        List<OcclusionDebugEntry> DebugEntries { get; }
#endif
    }
}
