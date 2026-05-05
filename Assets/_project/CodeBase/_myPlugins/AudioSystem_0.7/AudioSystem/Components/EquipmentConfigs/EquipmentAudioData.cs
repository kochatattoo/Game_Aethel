using Infrastructure.AudioSystem.Events;
using System;

namespace AudioSystem.Components.EquipmentConfigs
{
    [Serializable]
    public struct EquipmentAudioData<T> 
    {
       public T Value;
       public AudioSwitchAsset Switch;
    }
}
