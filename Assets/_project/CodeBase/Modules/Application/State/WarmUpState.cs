using CodeBase.CharacterIntegration;
using Cysharp.Threading.Tasks;

namespace CodeBase.Infrastructure.State
{
    public sealed class WarmUpState : IState
    {
        private readonly IGameStateMachine _states;
        private readonly IApplicationWarmup _warmup;
        private readonly CharacterMenuService _menu;
        public WarmUpState(IGameStateMachine states, IApplicationWarmup warmup, CharacterMenuService menu)
        {
            _states = states;
            _warmup = warmup;
            _menu = menu;
        }

        public void Enter() => PrepareAsync().Forget(_menu.LoadingFailed);
        public void Exit() { }

        private async UniTask PrepareAsync()
        {
            await _warmup.PrepareAsync();
            await _menu.InitializeAsync();
            _states.Enter<MainMenuState>();
        }
    }
}
