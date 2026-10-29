using Assets._project.CodeBase.Infrastructure.Services.AIServices.BlackboardSystem;
using Assets._project.CodeBase.Infrastructure.Services.Input;
using CodeBase.Hero;
using CodeBase.Infrastructure.AssetManagement;
using Cysharp.Threading.Tasks;
using Infrastructure.AudioSystem;
using Infrastructure.AudioSystem.Factory.GameComponents;
using Infrastructure.AudioSystem.Zones;
using UnityEngine;
using VFXSystem.Service;

namespace CodeBase.Infrastructure.Factory
{
    public sealed class HeroFactory : IHeroFactory
    {
        private readonly WorldAssetFactory _assets;
        private readonly IInputHandlerService _input;
        private readonly IBlackboardService _blackboard;
        private readonly IAudioFacade _audio;
        private readonly IFootstepAudioProcessorFactory _footsteps;
        private readonly IAudioZoneRegistry _zones;
        private readonly IVFXFacade _vfx;

        public HeroFactory(WorldAssetFactory assets, IInputHandlerService input, IBlackboardService blackboard,
            IAudioFacade audio, IFootstepAudioProcessorFactory footsteps, IAudioZoneRegistry zones, IVFXFacade vfx)
        {
            _assets = assets;
            _input = input;
            _blackboard = blackboard;
            _audio = audio;
            _footsteps = footsteps;
            _zones = zones;
            _vfx = vfx;
        }

        public async UniTask<HeroFacade> CreateAsync(Vector3 position)
        {
            var instance = await _assets.CreateAsync(AssetAddress.HeroPath, position);
            var hero = instance.GetComponent<HeroFacade>();
            hero.Construct(_input, _blackboard, _audio, _footsteps, _zones, _vfx);
            hero.Initialize();
            return hero;
        }
    }
}
