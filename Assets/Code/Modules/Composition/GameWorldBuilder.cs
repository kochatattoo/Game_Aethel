using System;
using System.Collections.Generic;
using Code.Character.Data;
using CodeBase.CharacterIntegration;
using CodeBase.Enemies;
using CodeBase.Infrastructure.Factory;
using CodeBase.Infrastructure.Services.PersistentProgress;
using CodeBase.Infrastructure.Services;
using CodeBase.Infrastructure.Services.StaticData;
using CodeBase.Infrastructure.Utils;
using CodeBase.StaticData;
using CodeBase.UI.Services.Factory;
using Cysharp.Threading.Tasks;
using PetProject.Encounter;
using PetProject.Navigation;
using Unity.AI.Navigation;
using Unity.Cinemachine;
using Unity.Cinemachine.TargetTracking;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace CodeBase.Composition
{
    /// <summary>Собирает Encounter, HUD и сохранение в основной игровой сцене.</summary>
    public sealed class GameWorldBuilder : IWorldBuilder
    {
        private readonly IGameFactory _factory;
        private readonly IUIFactory _ui;
        private readonly IStaticDataService _catalog;
        private readonly IPersistentProgressService _progress;
        private readonly IProgressRegistry _registry;
        private readonly ICharacterStatsSource _stats;
        private readonly IRandomService _random;
        private readonly ICharacterDataService _characterData;
        private readonly NavigationConfig _navigationTemplate;
        private readonly EncounterRulesConfig _rulesTemplate;
        private readonly InputActionAsset _inputAsset;
        private readonly GameObject _heroPrefab;
        private readonly GameObject _enemyPrefab;

        public GameWorldBuilder(IGameFactory factory, IStaticDataService catalog,
            IPersistentProgressService progress, IProgressRegistry registry, ICharacterStatsSource stats, IUIFactory ui,
            IRandomService random, ICharacterDataService characterData, NavigationConfig navigationTemplate,
            EncounterRulesConfig rulesTemplate,
            InputActionAsset inputAsset, GameObject heroPrefab, GameObject enemyPrefab)
        {
            _factory = factory;
            _catalog = catalog;
            _progress = progress;
            _registry = registry;
            _stats = stats;
            _ui = ui;
            _random = random;
            _characterData = characterData;
            _navigationTemplate = navigationTemplate;
            _rulesTemplate = rulesTemplate;
            _inputAsset = inputAsset;
            _heroPrefab = heroPrefab;
            _enemyPrefab = enemyPrefab;
        }

        public void CleanUp() => _factory.CleanUp();

        public async UniTask BuildAsync()
        {
            var scene = SceneManager.GetActiveScene().name;
            var data = _catalog.ForLevel(scene);
            foreach (var loot in _progress.Progress.WorldData.LootData.LootsOnGround.Dict)
                if (loot.Key.StartsWith(scene + "_", StringComparison.Ordinal)) await _factory.CreateLootFromPool(loot.Key);
            foreach (var trigger in data.SaveTriggers) await _factory.CreateSaveTrigger(trigger.Position, trigger.Id);
            if (!string.IsNullOrWhiteSpace(data.LevelTransferData.LevelTo)) await _factory.CreateTransferToPoint(data.LevelTransferData);

            var host = CreateEncounter(data);
            var health = host.HeroController.Health;
            if (health != null)
            {
                health.BindStats(_stats);
                await _ui.CreateHud(health);
            }
            await _registry.ApplyAsync(_progress.Progress);
        }

        private EncounterController CreateEncounter(LevelStaticData data)
        {
            var surface = FindNavMeshSurface();
            var config = UnityEngine.Object.Instantiate(_navigationTemplate);
            config.AgentTypeId = surface.agentTypeID;
            var settings = NavMesh.GetSettingsByID(config.AgentTypeId);
            config.AgentRadius = settings.agentRadius;
            config.AgentHeight = settings.agentHeight;
            config.Spawn = data.InitialHeroPosition;
            config.UseCustomOrigin = true;
            config.CustomOrigin = data.InitialHeroPosition - new Vector3(config.WorldSize * .5f, 0, config.WorldSize * .5f);

            var world = new GameObject("Encounter world").AddComponent<EncounterWorld>();
            world.Config = config;
            world.Surface = surface;
            world.GeneratedRoot = surface.transform;
            world.BakedSignature = world.Signature;

            var rules = UnityEngine.Object.Instantiate(_rulesTemplate);
            var character = _characterData.CurrentCharacter.Value;
            if (character != null)
                rules.SetHeroStats(Mathf.RoundToInt(character.Stats.MaxHealth.Value),
                    Mathf.Max(1, Mathf.RoundToInt(character.Stats.Damage.Value)));

            var hostObject = new GameObject("Encounter controller");
            hostObject.SetActive(false);
            var host = hostObject.AddComponent<EncounterController>();
            host.World = world;
            host.CombatRules = rules;
            host.ActionBudget = rules.ActionsPerTurn;
            host.HeroPrefab = _heroPrefab;
            host.EnemyPrefab = _enemyPrefab;
            host.EnemySpawns = CreateSpawns(data.EnemySpawners);
            host.InputAsset = _inputAsset;
            host.UseLegacyHud = true;
            host.GameCamera = Camera.main;
            if (host.GameCamera == null) throw new InvalidOperationException("На уровне отсутствует Main Camera");
            host.CameraRig = CreateCameraRig(hostObject, host.GameCamera, data.InitialHeroPosition);
            hostObject.SetActive(true);

            host.EncounterDirector.EnemyDefeated += OnEnemyDefeated;
            InitializeLoot(host, data);
            foreach (var reader in host.Hero.GetComponentsInChildren<ISavedProgressReader>()) _registry.Register(reader);
            return host;
        }

        private EnemySpawnDefinition[] CreateSpawns(IEnumerable<EnemySpawnerData> spawners)
        {
            var result = new List<EnemySpawnDefinition>();
            foreach (var spawner in LevelDataValidation.UniqueSpawners(spawners))
            {
                if (_progress.Progress.KillData.ClaeredSpawners.Contains(spawner.Id)) continue;
                result.Add(new EnemySpawnDefinition(spawner.position, spawner.Id, id: spawner.Id));
            }
            return result.ToArray();
        }

        private void InitializeLoot(EncounterController host, LevelStaticData data)
        {
            var spawners = new Dictionary<string, EnemySpawnerData>();
            foreach (var spawner in LevelDataValidation.UniqueSpawners(data.EnemySpawners))
                spawners[spawner.Id] = spawner;
            foreach (var enemy in host.EncounterDirector.Enemies)
            {
                var loot = enemy.Instance.GetComponentInChildren<LootSpawner>(true);
                if (loot == null) continue;
                var min = 1;
                var max = 2;
                if (spawners.TryGetValue(enemy.Id, out var spawn)
                    && _catalog.ForMonster(spawn.MonsterTypeID) is { } monster)
                {
                    min = monster.MinLoot;
                    max = monster.MaxLoot;
                }
                loot.Setloot(min, max);
                loot.Construct(_factory, _random);
            }
        }

        private void OnEnemyDefeated(string id)
        {
            var cleared = _progress.Progress.KillData.ClaeredSpawners;
            if (!cleared.Contains(id)) cleared.Add(id);
        }

        private static NavMeshSurface FindNavMeshSurface()
        {
            foreach (var root in SceneManager.GetActiveScene().GetRootGameObjects())
            foreach (var surface in root.GetComponentsInChildren<NavMeshSurface>(true))
                if (surface.navMeshData != null) return surface;
            throw new InvalidOperationException("На уровне отсутствует baked NavMeshSurface");
        }

        private static IsometricCameraController CreateCameraRig(GameObject host, Camera camera, Vector3 heroPosition)
        {
            if (camera.GetComponent<CinemachineBrain>() == null) camera.gameObject.AddComponent<CinemachineBrain>();
            var pivot = new GameObject("Camera focus").transform;
            pivot.position = heroPosition + Vector3.up * 1.8f;
            var virtualCamera = new GameObject("Cinemachine isometric camera").AddComponent<CinemachineCamera>();
            virtualCamera.Follow = pivot;
            virtualCamera.LookAt = pivot;
            virtualCamera.Lens.ModeOverride = LensSettings.OverrideModes.Orthographic;
            virtualCamera.Lens.OrthographicSize = 14;
            virtualCamera.Lens.FarClipPlane = 400;
            var orbital = virtualCamera.gameObject.AddComponent<CinemachineOrbitalFollow>();
            orbital.OrbitStyle = CinemachineOrbitalFollow.OrbitStyles.Sphere;
            orbital.Radius = 38;
            orbital.TrackerSettings.BindingMode = BindingMode.WorldSpace;
            orbital.TrackerSettings.PositionDamping = Vector3.zero;
            orbital.HorizontalAxis.Value = 45;
            orbital.VerticalAxis.Value = 45;
            orbital.VerticalAxis.Range = new Vector2(30, 65);
            orbital.RadialAxis.Value = 1;
            orbital.HorizontalAxis.Recentering.Enabled = false;
            orbital.VerticalAxis.Recentering.Enabled = false;
            virtualCamera.gameObject.AddComponent<CinemachineRotationComposer>();
            var rig = host.AddComponent<IsometricCameraController>();
            rig.VirtualCamera = virtualCamera;
            rig.Orbit = orbital;
            rig.Pivot = pivot;
            return rig;
        }
    }
}
