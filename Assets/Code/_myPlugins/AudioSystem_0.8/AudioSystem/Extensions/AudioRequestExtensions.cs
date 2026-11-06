using Infrastructure.AudioSystem.Components.Interfaces;
using Infrastructure.AudioSystem.Factory;

namespace AudioSystem.Extensions
{
    public static class AudioRequestExtensions
    {
        public static AudioRequest WithEnvironment(this AudioRequest request, IAudioEnviromentMaker source)
        {
            if (source == null)
                return request;

            var auxData = source.GetCurrentAuxSendData();
            if (auxData.IsBlended)
                return request.WithBlendedAuxSends(auxData.AuxBusA, auxData.VolumeA, auxData.AuxBusB, auxData.VolumeB);

            else if (auxData.AuxBusA != null)
                return request.WithBus(auxData.AuxBusA);

            return request;
        }
    }
}
