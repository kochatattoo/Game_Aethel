using Shared.Utils.Constants;
using UnityEngine;

namespace Infrastructure.AudioSystem.Parameters
{

    [CreateAssetMenu(fileName = nameof(WwiseToMaterialConfig), menuName = ScriptableObjectAudioNames.AudioMenu + "Maping/" + nameof(WwiseToMaterialConfig))]
    public class WwiseToMaterialConfig : BaseWwiseToMaterialConfig<AK.Wwise.Switch>
    {

    }
}
