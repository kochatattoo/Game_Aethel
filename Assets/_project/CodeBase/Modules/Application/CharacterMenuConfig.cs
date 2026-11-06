using Code.Hero.Data;
using UnityEngine;

namespace CodeBase.CharacterIntegration
{
    [CreateAssetMenu(menuName = "PetProject/Character Menu Config")]
    public sealed class CharacterMenuConfig : ScriptableObject, ICharacterMenuSettings
    {
        public string InitialLevel = "Level_2";
        public CharacterBalance Balance = new();

        string ICharacterMenuSettings.InitialLevel => InitialLevel;
        CharacterBalance ICharacterMenuSettings.Balance => Balance;
    }
}
