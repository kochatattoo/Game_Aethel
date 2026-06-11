namespace Infrastructure.AudioSystem.Abstractions
{
    public interface IAudioEvent: IWwiseWrapper<AK.Wwise.Event>
    {
        bool IsValid();
    }
}
