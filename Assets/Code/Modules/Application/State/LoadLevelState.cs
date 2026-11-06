using CodeBase.CharacterIntegration;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace CodeBase.Infrastructure.State
{
    public sealed class LoadLevelState : IPayloadedState<string>
    {
        private readonly IGameStateMachine _states;
        private readonly SceneLoader _scenes;
        private readonly IWorldBuilder _world;
        private readonly ILoadingScreen _curtain;
        private readonly CharacterMenuService _menu;

        public LoadLevelState(IGameStateMachine states, SceneLoader scenes, IWorldBuilder world,
            ILoadingScreen curtain, CharacterMenuService menu)
        {
            _states = states;
            _scenes = scenes;
            _world = world;
            _curtain = curtain;
            _menu = menu;
        }

        public void Enter(string sceneName)
        {
            _menu.BeforeWorldLoad();
            _world.CleanUp();
            _scenes.Load(sceneName, () => BuildAsync().Forget(_menu.LoadingFailed));
        }

        private async UniTask BuildAsync()
        {
            await _world.BuildAsync();
            _menu.WorldLoaded();
            _curtain.Hide();
            _states.Enter<GameLoopState>();
        }

        public void Exit() { }
    }
}
