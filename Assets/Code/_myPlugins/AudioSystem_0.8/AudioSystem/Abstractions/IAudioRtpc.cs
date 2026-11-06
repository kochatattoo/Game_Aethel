namespace Infrastructure.AudioSystem.Abstractions
{
    public interface IAudioRtpc : IWwiseWrapper<AK.Wwise.RTPC>
    {
        bool IsValid();
    }
}
