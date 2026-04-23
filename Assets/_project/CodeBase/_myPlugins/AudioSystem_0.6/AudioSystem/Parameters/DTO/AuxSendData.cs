namespace Infrastructure.AudioSystem.Parameters.DTO
{
    public struct AuxSendData
    {
        public AK.Wwise.AuxBus AuxBusA;
        public float VolumeA;
        public AK.Wwise.AuxBus AuxBusB;
        public float VolumeB;
        public bool IsBlended;
    }
}
