using Code.Hero.Data;
using UnityEngine;

namespace CodeBase.CharacterIntegration
{
    [CreateAssetMenu(menuName = "PetProject/Character Menu Config")]
    public sealed class CharacterMenuConfig : ScriptableObject
    {
        public string InitialLevel = "Level_2";
        public CharacterBalance Balance = new();
    }
}
