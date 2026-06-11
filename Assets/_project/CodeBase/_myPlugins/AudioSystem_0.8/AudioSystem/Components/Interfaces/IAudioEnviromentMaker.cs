using Infrastructure.AudioSystem.Components.Sensors;
using Infrastructure.AudioSystem.Parameters.DTO;

namespace Infrastructure.AudioSystem.Components.Interfaces
{
    public interface IAudioEnviromentMaker : IAudioMaker
    {
        EnviromentResolver EnviromentResolver { get; }
        PortalResolver PortalResolver { get; }
        AuxSendData GetCurrentAuxSendData();
    }
}
