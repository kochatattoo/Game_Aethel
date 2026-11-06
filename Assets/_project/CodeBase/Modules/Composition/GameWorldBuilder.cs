using CodeBase.UI.Services.Factory;
using System;
using CodeBase.CharacterIntegration;
using CodeBase.Hero;
using CodeBase.Infrastructure.Factory;
using CodeBase.Infrastructure.Services.PersistentProgress;
using CodeBase.Infrastructure.Services.StaticData;
using CodeBase.Infrastructure.Utils;
using CodeBase.UI.Elements;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CodeBase.Composition
{
    /// <summary>Connects concrete world and presentation types at the composition boundary.</summary>
    public sealed class GameWorldBuilder : IWorldBuilder
    {
        private readonly IGameFactory _factory;
        private readonly IUIFactory _ui;
        private readonly IStaticDataService _catalog;
        private readonly IPersistentProgressService _progress;
        private readonly IProgressRegistry _registry;
        private readonly ICharacterStatsSource _stats;

        public GameWorldBuilder(IGameFactory factory, IStaticDataService catalog,
            IPersistentProgressService progress, IProgressRegistry registry, ICharacterStatsSource stats, IUIFactory ui)
        {
            _factory = factory;
            _catalog = catalog;
            _progress = progress;
            _registry = registry;
            _stats = stats;
            _ui = ui;
        }

        public void CleanUp() => _factory.CleanUp();

        public async UniTask BuildAsync()
        {
            var scene = SceneManager.GetActiveScene().name;
            var data = _catalog.ForLevel(scene);
            foreach (var spawner in CodeBase.StaticData.LevelDataValidation.UniqueSpawners(data.EnemySpawners))
                await _factory.CreateSpawner(spawner.position, spawner.Id, spawner.MonsterTypeID);
            foreach (var loot in _progress.Progress.WorldData.LootData.LootsOnGround.Dict)
                if (loot.Key.StartsWith(scene + "_", StringComparison.Ordinal)) await _factory.CreateLootFromPool(loot.Key);
            foreach (var trigger in data.SaveTriggers) await _factory.CreateSaveTrigger(trigger.Position, trigger.Id);
            if (!string.IsNullOrWhiteSpace(data.LevelTransferData.LevelTo)) await _factory.CreateTransferToPoint(data.LevelTransferData);

            var hero = await _factory.CreateHero(data.InitialHeroPosition);
            hero.Health.BindStats(_stats);
            await _ui.CreateHud(hero.Health);
            var camera = Camera.main;
            if (camera == null) throw new InvalidOperationException("На уровне отсутствует Main Camera");
            camera.GetComponent<CameraFollow>().Construct(hero.transform);
            camera.GetComponent<CameraOcclusionFade>().Construct(hero.transform);
            await _registry.ApplyAsync(_progress.Progress);
        }
    }
}
