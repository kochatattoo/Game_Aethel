using Shared.Utils.Constants;
using UnityEngine;

namespace Infrastructure.AudioSystem.Parameters
{

    [CreateAssetMenu(fileName = nameof(WwiseToMaterialConfig), menuName = ScriptableObjectNames.AudioMaping + nameof(WwiseToMaterialConfig))]
    public class WwiseToMaterialConfig : BaseWwiseToMaterialConfig<AK.Wwise.Switch>
    {

    }
}
