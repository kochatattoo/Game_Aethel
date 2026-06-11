namespace Infrastructure.AudioSystem.Abstractions
{
    public interface IAudioState : IWwiseWrapper<AK.Wwise.State>
    {
        bool IsValid();
    }
}
