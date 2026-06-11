using Shared.Utils.Constants;
using UnityEngine;

namespace Infrastructure.AudioSystem.Parameters
{
    [CreateAssetMenu(fileName = nameof(AudioKeyToMaterialConfig), menuName = ScriptableObjectNames.AudioMaping + nameof(AudioKeyToMaterialConfig))]
    public class AudioKeyToMaterialConfig : BaseWwiseToMaterialConfig<string>
    {

    }
}
