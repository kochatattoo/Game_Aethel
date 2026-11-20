using UniRx;

namespace Code.Character.Data
{
    public interface ICharacterDataService
    {
        IReadOnlyReactiveProperty<CharacterData> CurrentCharacter { get; }
        void SetCharacter(CharacterData data);
    }
}