using CodeBase.CharacterIntegration;
using CodeBase.Infrastructure.Factory;
using CodeBase.UI.Services.Factory;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace CodeBase.Composition
{
    public sealed class ApplicationWarmup : IApplicationWarmup
    {
        private readonly IGameFactory _game;
        private readonly IUIFactory _ui;
        public ApplicationWarmup(IGameFactory game, IUIFactory ui) { _game = game; _ui = ui; }
        public async UniTask PrepareAsync()
        {
            await UniTask.WhenAll(_game.WarmUpAsync(), _ui.CreateUIRootAsync());
            _ui.WarmUp();
        }
    }

    public sealed class MenuShortcutHandler : ITickable
    {
        private readonly CharacterMenuService _menu;
        public MenuShortcutHandler(CharacterMenuService menu) => _menu = menu;
        public void Tick() => _menu.Tick();
    }

    public sealed class ApplicationHost : IApplicationHost
    {
        public static IApplicationHost Current { get; set; } = new ApplicationHost();
        public void Quit() => Application.Quit();
    }
}
