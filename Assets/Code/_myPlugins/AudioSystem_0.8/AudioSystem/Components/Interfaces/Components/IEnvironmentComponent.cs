namespace AudioSystem.Components.Interfaces.Components
{
    // TODO: Подумать над переводом с AK.Wwise на абстракции
    public interface IEnvironmentComponent
    {
        AK.Wwise.AuxBus AuxBus { get; }
        int Priority { get; }
    }
}
