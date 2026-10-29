using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Code.UI.Windows;
using CodeBase.CharacterIntegration;
using CodeBase.Data;
using CodeBase.Enemies;
using CodeBase.Hero;
using CodeBase.Infrastructure.Services.ObjectPool;
using CodeBase.Infrastructure.Services.PersistentProgress;
using CodeBase.Infrastructure.Services.SaveLoad;
using CodeBase.Infrastructure.State;
using CodeBase.Logic;
using CodeBase.UI.Services.Windows;
using CodeBase.UI.Windows;
using Cysharp.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using Zenject;
using Object = UnityEngine.Object;

namespace CodeBase.EditorTools
{
    public static class RefactoringGameplaySmoke
    {
        [MenuItem("Tools/PetProject/Refactoring/Validate Gameplay Lifecycle")]
        public static void Run() => Validate().Forget(Debug.LogException);

        private static async UniTask Validate()
        {
            if (!EditorApplication.isPlaying || !SessionState.GetBool("PetProject.CharacterMenu.Smoke", false))
                throw new InvalidOperationException("Start the isolated Save Storage smoke first");
            var checks = new List<string>();
            var result = new Result();
            try
            {
                var container = ProjectContext.Instance.Container;
                var menu = container.Resolve<CharacterMenuService>();
                var registry = container.Resolve<IProgressRegistry>();
                var slots = container.Resolve<ISaveSlotService>();
                var saves = container.Resolve<ISaveLoadService>();
                var progress = container.Resolve<IPersistentProgressService>();
                Require(slots.ActiveSlotId == "slot-3", "New profile slot survived restart");
                checks.Add("Active slot and new profile restored across Play mode restart");
                menu.Continue();
                await WaitForWorld();
                var hero = Object.FindFirstObjectByType<HeroFacade>();
                Require(hero != null && hero.GetComponent<UnityEngine.AI.NavMeshAgent>().isOnNavMesh, "Hero navigation");
                Require(Math.Abs(hero.Health.Max - 50) < .001f, "Hero health binding");
                var readers = registry.Readers.Count;
                var allSpawners = Object.FindObjectsByType<SpawnPoint>(FindObjectsSortMode.None);
                Require(allSpawners.Length > 1 && allSpawners.All(s => s.Slain || s.GetComponentInChildren<EnemyDeath>() != null), "Awaited enemy restoration");
                checks.Add("Hero, navigation, stats and all living spawners restored before world readiness");

                for (var i = 0; i < 10; i++)
                {
                    menu.OpenAttributes(); await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);
                    Require(Object.FindObjectsByType<DemoWindow>(FindObjectsSortMode.None).Length == 1 && MenuInputScope.IsBlocked && Time.timeScale == 0, "Open attributes");
                    menu.Resume(); await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);
                    Require(!MenuInputScope.IsBlocked && Math.Abs(Time.timeScale - 1) < .001f, "Release menu input and pause");
                }
                Require(registry.Readers.Count == readers, "Menu does not register world readers");
                checks.Add("10 attribute open/close cycles: one view, no retained input blocks or progress readers");

                for (var i = 0; i < 10; i++)
                {
                    await container.Resolve<IWindowService>().OpenAsync(WindowId.Option);
                    var window = Object.FindFirstObjectByType<OptionWindow>();
                    Require(window != null, "Pooled option window opens");
                    window.CloseButton.onClick.Invoke();
                    Require(container.Resolve<IPoolService>().GetPool<OptionWindow>().FreeCount == 1, "Option window returns exactly once");
                }
                checks.Add("10 pooled option window cycles with stable available instance count");

                // Pause AI while checking persistence of death, drops and pickup.
                using (MenuInputScope.Acquire())
                {
                    var first = allSpawners.First(s => !s.Slain);
                    var firstLoot = await KillAndGetDrop(first);
                    saves.SaveProgress();
                    firstLoot.SendMessage("OnTriggerEnter", hero.GetComponent<Collider>());
                    saves.SaveProgress();
                    Require(!registry.Readers.Contains(firstLoot), "Picked loot unregisters");
                    Require(progress.Progress.WorldData.LootData.LootsOnGround.Dict.Count == 0, "Picked saved loot is removed");
                    checks.Add("Enemy death creates one drop; picking saved loot removes its progress entry");

                    var second = allSpawners.First(s => !s.Slain);
                    await KillAndGetDrop(second);
                    saves.SaveProgress(); saves.SaveProgress();
                    Require(progress.Progress.KillData.ClaeredSpawners.Count(id => id == second.Id) == 1, "Killed spawner is captured once");
                    Require(progress.Progress.WorldData.LootData.LootsOnGround.Dict.Count == 1, "Uncollected loot captured once");
                    result.clearedSpawner = second.Id;
                    result.lootValue = progress.Progress.WorldData.LootData.LootsOnGround.Dict.Values.Single().loot.Value;
                }

                var previousHero = hero;
                container.Resolve<IGameStateMachine>().Enter<ReloadGameState>();
                await UniTask.WaitUntil(() => previousHero == null).Timeout(TimeSpan.FromSeconds(60), DelayType.Realtime);
                await WaitForWorld();
                var restoredSpawner = Object.FindObjectsByType<SpawnPoint>(FindObjectsSortMode.None).Single(s => s.Id == result.clearedSpawner);
                var restoredLoot = Object.FindObjectsByType<LootPiece>(FindObjectsSortMode.None).Where(l => l.gameObject.activeInHierarchy).ToArray();
                Require(restoredSpawner.Slain && restoredSpawner.GetComponentInChildren<EnemyDeath>() == null, "Slain enemy does not respawn");
                Require(restoredLoot.Length == 1, "Exactly one saved drop restored");
                saves.SaveProgress();
                Require(saves.LoadProgress().WorldData.LootData.LootsOnGround.Dict.Values.Single().loot.Value == result.lootValue, "Restored drop value preserved");
                Require(registry.Readers.Distinct().Count() == registry.Readers.Count, "Registry contains no duplicate readers");
                checks.Add("World reload preserves slain spawner and one drop without duplicate registrations");
                result.success = true;
            }
            catch (Exception error) { result.error = error.ToString(); throw; }
            finally
            {
                result.utc = DateTime.UtcNow.ToString("O"); result.checks = checks.ToArray();
                Directory.CreateDirectory("Library/Refactor");
                File.WriteAllText("Library/Refactor/gameplay-smoke.json", JsonUtility.ToJson(result, true));
            }
        }

        private static async UniTask<LootPiece> KillAndGetDrop(SpawnPoint spawner)
        {
            var previous = Object.FindObjectsByType<LootPiece>(FindObjectsSortMode.None).ToHashSet();
            var death = spawner.GetComponentInChildren<EnemyDeath>();
            ((IHealth)death.Health).Current = 0;
            await UniTask.WaitUntil(() => Object.FindObjectsByType<LootPiece>(FindObjectsSortMode.None).Any(l => !previous.Contains(l)))
                .Timeout(TimeSpan.FromSeconds(10), DelayType.Realtime);
            await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);
            Require(spawner.Slain, "Spawner observes enemy death");
            return Object.FindObjectsByType<LootPiece>(FindObjectsSortMode.None).Single(l => !previous.Contains(l));
        }

        private static async UniTask WaitForWorld() => await UniTask.WaitUntil(() =>
            Object.FindFirstObjectByType<HeroFacade>() != null && !MenuInputScope.IsBlocked && Math.Abs(Time.timeScale - 1) < .001f)
            .Timeout(TimeSpan.FromSeconds(60), DelayType.Realtime);
        private static void Require(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
        [Serializable] private sealed class Result { public bool success; public string utc, error, clearedSpawner; public int lootValue; public string[] checks; }
    }
}
