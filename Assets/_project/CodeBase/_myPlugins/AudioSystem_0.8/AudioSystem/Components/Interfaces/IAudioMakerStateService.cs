using Infrastructure.AudioSystem.Events;

namespace Infrastructure.AudioSystem.Components.Interfaces
{
    public interface IAudioMakerStateService
    {
        void EnterState(IAudioMaker maker, AudioEventAsset eventAsset, AudioParameterAsset parameterAsset, float fadeDuration, bool isLoop);
        void ExitState(IAudioMaker maker, AudioEventAsset eventAsset, float fadeDuration, bool isLoop);
    }
}
