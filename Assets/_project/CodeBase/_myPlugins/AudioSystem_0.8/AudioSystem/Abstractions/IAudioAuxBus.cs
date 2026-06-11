namespace Infrastructure.AudioSystem.Abstractions
{
    public interface IAudioAuxBus : IWwiseWrapper<AK.Wwise.AuxBus>
    {
        bool IsValid();
    }
}
