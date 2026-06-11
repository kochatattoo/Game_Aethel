namespace Infrastructure.AudioSystem.Abstractions
{
    public interface IAudioSwitch : IWwiseWrapper<AK.Wwise.Switch>
    {
        bool IsValid();
    }
}
