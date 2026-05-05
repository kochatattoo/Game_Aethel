namespace AudioSystem.Components.EquipmentConfigs.EquipmentResolver
{
    public class BootsEquipmentResolver: IEquipmentResolver<BootsType>
    {
        private readonly EquipmentMapResolver<BootsType> _resolvedMap;

        public BootsEquipmentResolver(BootsMapConfig mapData)
        {
            _resolvedMap = new EquipmentMapResolver<BootsType>(mapData);
        }

        public AK.Wwise.Switch GetEquipmentAudioKey(BootsType equipmentType) => 
            _resolvedMap.GetEquipmentAudioKey(equipmentType);
    }
}
