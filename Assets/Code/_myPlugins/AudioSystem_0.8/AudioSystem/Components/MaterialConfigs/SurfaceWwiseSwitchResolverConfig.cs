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
    [CreateAssetMenu(fileName = nameof(SurfaceWwiseSwitchResolverConfig), menuName = ScriptableObjectNames.AudioMenu + nameof(SurfaceWwiseSwitchResolverConfig))]
    public class SurfaceWwiseSwitchResolverConfig : BaseSurfaceResolverConfig<AK.Wwise.Switch>
    {
        // TODO: Подумать над переводом с AK.Wwise на абстракции
        [SerializeField]
        private AK.Wwise.Switch _defaultSwitch;
        public override AK.Wwise.Switch DefaultSwitch => _defaultSwitch;

        [SerializeField]
        private List<WwiseTagMapping> _tagFallback = new();

        public override IEnumerable<(string Tag, AK.Wwise.Switch Key)> GetTagMappings() =>
            _tagFallback.Select(m => (m.Tag, m.SwitchKey));

        [Serializable]
        public struct WwiseTagMapping
        {
            public string Tag;
            public AK.Wwise.Switch SwitchKey;
        }
    }
}
