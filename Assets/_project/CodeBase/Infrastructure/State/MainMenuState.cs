using CodeBase.CharacterIntegration;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace CodeBase.Infrastructure.State
{
    public sealed class MainMenuState : IState
    {
        private readonly CharacterMenuService _menu;
        public MainMenuState(CharacterMenuService menu) => _menu = menu;
        public void Enter() => _menu.ShowMainMenuAsync().Forget(Debug.LogException);
        public void Exit() { }
    }
}
