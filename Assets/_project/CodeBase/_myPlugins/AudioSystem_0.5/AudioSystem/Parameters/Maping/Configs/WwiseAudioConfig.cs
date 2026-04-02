using Shared.Utils.Constants;
using System;
using System.Collections.Generic;
using UnityEngine;
using Infrastructure.AudioSystem.Utils;

namespace Infrastructure.AudioSystem.Parameters
{
    /// <summary>
    /// Центральный реестр связей между строковыми идентификаторами проекта и внутренними объектами Wwise.
    /// Необходим для управления внутри кода (дла дизайнера используем метод прямых указаний на события Wwise)
    /// </summary>
    [CreateAssetMenu(fileName = nameof(WwiseAudioConfig), menuName = ScriptableObjectAudioNames.AudioMenu + "Maping/" + nameof(WwiseAudioConfig))]
    public class WwiseAudioConfig : ScriptableObject
    {
        [SerializeField] 
        private List<EventMapping> _events = new();
        [SerializeField] 
        private List<RtpcMapping> _rtpcs = new();
        [SerializeField] 
        private List<SwitchMapping> _switches = new();
        [SerializeField] 
        private List<StateMapping> _states = new();

        public IReadOnlyList<EventMapping> Events => _events;
        public IReadOnlyList<RtpcMapping> Rtpcs => _rtpcs;
        public IReadOnlyList<SwitchMapping> Switches => _switches;
        public IReadOnlyList<StateMapping> States => _states;

        /// <summary>
        /// Связка строкового идентификатора и конкретного актива Wwise Event.
        /// </summary>
        [Serializable]
        public struct EventMapping
        {
            /// <summary> Строковый ключ (ID), используемый в коде (например, "Footstep"). </summary>
            [AudioKeyAttribute(AudioKeyType.Event)]
            public string Id;
            /// <summary> Ссылка на объект события из Wwise Browser. </summary>
            public AK.Wwise.Event WwiseEvent;
        }

        /// <summary>
        /// Связка строкового идентификатора и конкретного актива Wwise Parameter.
        /// </summary>
        [Serializable]
        public struct RtpcMapping
        {
            /// <summary> Строковый ключ (ID), используемый в коде (например, "Footstep"). </summary>
            [AudioKeyAttribute(AudioKeyType.Parameter)]
            public string Id;
            /// <summary> Ссылка на объект события из Wwise Browser. </summary>
            public AK.Wwise.RTPC WwiseRtpc; 
        }

        /// <summary>
        /// Связка строкового идентификатора и конкретного актива Wwise Switch.
        /// </summary>
        [Serializable]
        public struct SwitchMapping
        {
            /// <summary> Строковый ключ (ID), используемый в коде (например, "Footstep"). </summary>
            [AudioKeyAttribute(AudioKeyType.Switch)]
            public string Id;
            /// <summary> Ссылка на объект события из Wwise Browser. </summary>
            public AK.Wwise.Switch WwiseSwitch;
        }

        /// <summary>
        /// Связка строкового идентификатора и конкретного актива Wwise State.
        /// </summary>
        [Serializable]
        public struct StateMapping
        {
            /// <summary> Строковый ключ (ID), используемый в коде (например, "Footstep"). </summary>
            [AudioKeyAttribute(AudioKeyType.State)]
            public string Id;
            /// <summary> Ссылка на объект события из Wwise Browser. </summary>
            public AK.Wwise.State WwiseState;
        }
    }
}
