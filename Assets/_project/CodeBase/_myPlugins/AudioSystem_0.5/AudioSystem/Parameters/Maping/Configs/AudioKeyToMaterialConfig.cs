using Shared.Utils.Constants;
using UnityEngine;

namespace Infrastructure.AudioSystem.Parameters
{
    [CreateAssetMenu(fileName = nameof(AudioKeyToMaterialConfig), menuName = ScriptableObjectAudioNames.AudioMenu + "Maping/" + nameof(AudioKeyToMaterialConfig))]
    public class AudioKeyToMaterialConfig : BaseWwiseToMaterialConfig<string>
    {

    }
}
