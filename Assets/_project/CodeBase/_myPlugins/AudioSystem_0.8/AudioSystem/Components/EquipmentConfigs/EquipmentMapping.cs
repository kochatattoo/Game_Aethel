using System.Collections.Generic;

namespace AudioSystem.Components.EquipmentConfigs
{
    // TODO: Подумать над переводом с AK.Wwise на абстракции
    public class EquipmentMapping<T>
    {
        private readonly Dictionary<T, AK.Wwise.Switch> _map;

        public IReadOnlyDictionary<T, AK.Wwise.Switch> Map => _map;

        public EquipmentMapping(EquipmentMapConfig<T> mapConfig)
        {
            _map = new Dictionary<T, AK.Wwise.Switch>();
            foreach (var equipmentAudioData in mapConfig.EquipmentAudioData)
            {
                _map.Add(equipmentAudioData.Value, equipmentAudioData.Switch.WwiseSwitch);
            }
        }
    }
}
