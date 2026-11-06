using Infrastructure.AudioSystem.Events;
using System.Collections.Generic;
using UnityEngine;

namespace AudioSystem.Components.EquipmentConfigs
{
    public class EquipmentMapConfig<T> : ScriptableObject
    {
        [SerializeField]
        private List<EquipmentAudioData<T>> _equipmentAudioDatas = new();

        [field: SerializeField]
        public AudioSwitchAsset DefaultSwitch {  get; private set; }

        public IReadOnlyList<EquipmentAudioData<T>> EquipmentAudioData => _equipmentAudioDatas;
    }
}
