using Infrastructure.AudioSystem.Utils;
using UnityEngine;


namespace Infrastructure.AudioSystem.Components.Materials
{
    public class SurfaceMaterialAudioKey : MonoBehaviour, ISurfaceMaterial<string>
    {
        // Убрать и переделать на мапу соответствий Материал - Ключ

        [field:SerializeField, AudioKeyAttribute(AudioKeyType.Switch)]
        public string SurfaceKey { get; private set; }
    }
}
