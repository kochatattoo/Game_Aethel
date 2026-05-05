using AudioSystem.Components.Interfaces.Components;

namespace Infrastructure.AudioSystem.Components.Occlusions
{
    public interface IOcclusionService
    {
        void RegisterEmitter(IOcclusionEmitter emitter);
        void UnregisterEmitter(IOcclusionEmitter emitter);
    }
}
