using Infrastructure.AudioSystem.Utils;
using Shared.Utils.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Infrastructure.AudioSystem.Components.MaterialConfigs
{
    /// <summary>
    /// Конфигурация расчета поверхности для аудиосистемы
    /// </summary>
    [CreateAssetMenu(fileName = nameof(SurfaceAudioKeyResolverConfig), menuName = ScriptableObjectNames.AudioMenu + nameof(SurfaceAudioKeyResolverConfig))]
    public class SurfaceAudioKeyResolverConfig : BaseSurfaceResolverConfig<string>
    {
        [SerializeField, AudioKey(AudioKeyType.Switch)]
        private string _defaultSwitch = "Default";
        public override string DefaultSwitch => _defaultSwitch;

        [SerializeField]
        private List<TagMapping> _tagFallback = new();

        public override IEnumerable<(string Tag, string Key)> GetTagMappings() =>
            _tagFallback.Select(m => (m.Tag, m.SwitchKey));

        [Serializable]
        public struct TagMapping
        {
            public string Tag;
            [AudioKey(AudioKeyType.Switch)]
            public string SwitchKey;
        }
    }
}
