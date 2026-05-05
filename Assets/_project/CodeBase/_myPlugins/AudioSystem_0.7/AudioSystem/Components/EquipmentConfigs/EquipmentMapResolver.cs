namespace AudioSystem.Components.EquipmentConfigs
{
    public class EquipmentMapResolver<T>
    {
        private readonly EquipmentMapping<T> _maping;
        private readonly AK.Wwise.Switch _defaultSwitch;

        public EquipmentMapResolver(EquipmentMapConfig<T> mapConfig)
        {
          _maping = new(mapConfig);
            _defaultSwitch = mapConfig.DefaultSwitch.WwiseSwitch;
        }

        public AK.Wwise.Switch GetEquipmentAudioKey(T equipmentType)
        {
            if(_maping.Map.TryGetValue(equipmentType,out AK.Wwise.Switch value))
            {
                return value;
            }

            return _defaultSwitch;
        }
    }
}
