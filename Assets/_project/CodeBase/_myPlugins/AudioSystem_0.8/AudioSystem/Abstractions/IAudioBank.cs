namespace Infrastructure.AudioSystem.Abstractions
{
    public interface IAudioBank : IWwiseWrapper<AK.Wwise.Bank>
    {
        bool IsValid();
    }
}
