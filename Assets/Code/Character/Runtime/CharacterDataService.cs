using System;
using UniRx;
namespace Code.Character.Data
{
    public sealed class CharacterDataService : ICharacterDataService,IDisposable
    {
        private readonly ReactiveProperty<CharacterData> _current=new();
        public IReadOnlyReactiveProperty<CharacterData> CurrentCharacter=>_current;
        public void SetCharacter(CharacterData data){var old=_current.Value;_current.Value=data;if(!ReferenceEquals(old,data))old?.Dispose();}
        public void Dispose(){_current.Value?.Dispose();_current.Dispose();}
    }
}

