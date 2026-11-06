using UnityEngine;
using VFXSystem.BaseTypes;
using VFXSystem.Components;

namespace CodeBase.Components.Vfx
{
    public class Hitbox : MonoBehaviour, IHitbox
    {
        [field: SerializeField]
        public MaterialType MaterialType { get; private set; }
    }
}
