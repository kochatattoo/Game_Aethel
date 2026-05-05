using Infrastructure.AudioSystem.Events;
using UnityEngine;


namespace Infrastructure.AudioSystem.Components.Materials
{
    public class SurfaceMaterialWwiseSwitch : MonoBehaviour, ISurfaceMaterial<AK.Wwise.Switch>
    {
        [field: SerializeField]
        private AudioSwitchAsset _switchAsset;

        public AK.Wwise.Switch SurfaceKey => _switchAsset.WwiseSwitch;
    }
}
