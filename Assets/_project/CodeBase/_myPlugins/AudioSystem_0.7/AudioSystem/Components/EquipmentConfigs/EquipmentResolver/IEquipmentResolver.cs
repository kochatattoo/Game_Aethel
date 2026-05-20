namespace AudioSystem.Components.EquipmentConfigs.EquipmentResolver
{
    public interface IEquipmentResolver<T>
    {
        AK.Wwise.Switch GetEquipmentAudioKey(T equipmentType);
    }
}
