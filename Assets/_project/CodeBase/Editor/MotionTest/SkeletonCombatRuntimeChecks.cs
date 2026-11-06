using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using CodeBase.MotionTest;
using Cysharp.Threading.Tasks;
using PetProject.Tactics;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using Object = UnityEngine.Object;

namespace CodeBase.EditorTools
{
    public static class SkeletonCombatRuntimeChecks
    {
        private const string Key = "PetProject.SkeletonCombatChecks";
        public const string ReportPath = "Library/MotionTest/skeleton-combat-checks.json";
        [Serializable] private sealed class Report
        {
            public bool pending, success;
            public string error;
            public List<string> checks = new List<string>();
        }

        [InitializeOnLoadMethod] private static void Register()
        {
            EditorApplication.playModeStateChanged -= Changed;
            EditorApplication.playModeStateChanged += Changed;
        }

        [MenuItem("Tools/PetProject/Motion/Run Skeleton Combat Checks")]
        public static void Run()
        {
            TacticsSandboxTools.RequireCleanEditMode();
            EditorSceneManager.OpenScene(MotionTestSceneTools.ScenePath);
            MotionTestSceneTools.EnsureBaked();
            Directory.CreateDirectory("Library/MotionTest");
            File.WriteAllText(ReportPath, JsonUtility.ToJson(new Report { pending = true }, true));
            SessionState.SetBool(Key, true);
            EditorApplication.isPlaying = true;
        }

        private static void Changed(PlayModeStateChange state)
        {
            if (!SessionState.GetBool(Key, false)) return;
            if (state == PlayModeStateChange.EnteredPlayMode) Check().Forget(Debug.LogException);
            if (state != PlayModeStateChange.EnteredEditMode) return;
            var report = JsonUtility.FromJson<Report>(File.ReadAllText(ReportPath));
            report.pending = false;
            File.WriteAllText(ReportPath, JsonUtility.ToJson(report, true));
            SessionState.SetBool(Key, false);
        }

        private static async UniTask Check()
        {
            var report = new Report();
            try
            {
                await UniTask.DelayFrame(3);
                var host = Object.FindFirstObjectByType<MotionTestController>();
                Require(host != null && host.Ready, "Motion host ready: " + host?.Error);
                var director = host.EncounterDirector;
                Require(director != null, "Skeleton combat director exists");
                director.ResetCombat();
                director.SetEnabled(true);
                await UniTask.DelayFrame(3);
                var target = Object.FindFirstObjectByType<MotionEnemyTarget>();
                Require(target != null, "Skeleton target spawned without EnemyFactory");
                Require(!director.Active, "No combat before detection");
                host.WorldActions.Set(MotionTestController.HeroActorId, 4, 6);
                report.checks.Add("Skeleton visual prefab initializes without campaign services or real-time combat");

                Require(director.HandleExplorationClick(target.GetComponentInChildren<Collider>()), "Skeleton click starts following");
                await WaitFor(() => director.Active && director.Snapshot != null, "Detection creates Encounter", 20000);
                Require(director.Snapshot.Actors.Single(a => a.Id == MotionTestController.HeroActorId).ActionPoints == 4,
                    "Initial hero AP comes from world budget");
                report.checks.Add("Click follows skeleton; detection creates Encounter with preserved world AP");

                await WaitFor(() => director.PlayerTurnReady, "First hero turn", 15000);
                await UniTask.DelayFrame(2);
                Require(!host.Hud.ModeButton.interactable, "Free mode exit is disabled during real combat");
                MotionEditorInputGate.InputFixtureActive = true;
                var mouse = InputSystem.AddDevice<Mouse>("Skeleton HUD fixture mouse");
                try
                {
                    var revision = director.Snapshot.Revision;
                    await ClickHud(mouse, host.Hud.EndTurnButton);
                    Require(director.Snapshot.Revision > revision,
                        "Runtime HUD End Turn advances the Encounter");
                }
                finally
                {
                    InputSystem.RemoveDevice(mouse);
                    MotionEditorInputGate.InputFixtureActive = false;
                }
                report.checks.Add("Runtime HUD End Turn works in real combat; free exit is disabled");
                MotionRuntimeChecks.Capture(host, "Library/MotionTest/skeleton-combat-start.png");

                var start = DateTime.UtcNow;
                var decided = false;
                while ((DateTime.UtcNow - start).TotalSeconds < 45)
                {
                    if (director.IsDefeated)
                    {
                        decided = true;
                        report.checks.Add("Hero defeat is produced by Encounter and exposes Reset");
                        director.ResetCombat();
                        await UniTask.DelayFrame(3);
                        Require(!director.Active && host.HeroAgent.enabled, "Defeat reset restores exploration");
                        report.checks.Add("Defeat Reset restores normal movement");
                        break;
                    }
                    if (!director.Active)
                    {
                        decided = true;
                        Require(host.HeroAgent.enabled, "Victory restores NavMesh exploration");
                        report.checks.Add("Skeleton defeat completes Encounter and restores normal movement");
                        break;
                    }
                    if (director.PresentationBusy || director.Snapshot == null)
                    {
                        await UniTask.Yield();
                        continue;
                    }
                    var snapshot = director.Snapshot;
                    if (snapshot.IsCompleted)
                    {
                        await UniTask.Yield();
                        continue;
                    }
                    if (snapshot.CurrentActorId != MotionTestController.HeroActorId)
                    {
                        await UniTask.Yield();
                        continue;
                    }
                    var hero = snapshot.Actors.Single(a => a.Id == MotionTestController.HeroActorId);
                    var enemy = snapshot.Actors.Single(a => a.Id == MotionEnemyTarget.SkeletonActorId);
                    if (!enemy.IsAlive)
                    {
                        await UniTask.Yield();
                        continue;
                    }
                    if (hero.ActionPoints == 0)
                    {
                        await UniTask.Yield();
                        continue;
                    }
                    if (hero.Position.DistanceTo(enemy.Position) == 1)
                    {
                        director.TryHeroAttack();
                    }
                    else
                    {
                        var path = BestApproachPath(host, director, hero, enemy);
                        if (path != null) director.TryHeroMove(path.Cells[path.Cells.Count - 1]);
                        else director.EndTurnFromHud();
                    }
                    await UniTask.Yield();
                }
                var timeoutState = director.Snapshot == null ? "Encounter is null" :
                    string.Join(" | ", director.Snapshot.Actors.Select(a => $"{a.Id}: HP={a.Health}, AP={a.ActionPoints}, cell={a.Position}")) +
                    $" | current={director.Snapshot.CurrentActorId}, busy={director.PresentationBusy}";
                Require(decided, "Encounter did not finish: " + timeoutState);
                MotionRuntimeChecks.Capture(host, "Library/MotionTest/skeleton-combat.png");
                report.success = true;
            }
            catch (Exception error)
            {
                report.error = error.ToString();
                Debug.LogException(error);
            }
            finally
            {
                File.WriteAllText(ReportPath, JsonUtility.ToJson(report, true));
                EditorApplication.isPlaying = false;
            }
        }

        private static GridPath BestApproachPath(MotionTestController host, SkeletonCombatDirector director,
            ActorSnapshot hero, ActorSnapshot enemy)
        {
            GridPath best = null;
            var candidates = new[]
            {
                new GridPosition(enemy.Position.X + 1, enemy.Position.Y),
                new GridPosition(enemy.Position.X - 1, enemy.Position.Y),
                new GridPosition(enemy.Position.X, enemy.Position.Y + 1),
                new GridPosition(enemy.Position.X, enemy.Position.Y - 1)
            };
            foreach (var candidate in candidates)
            {
                var path = director.PreviewHeroMove(candidate);
                if (!path.Found) continue;
                if (best == null || path.Cost < best.Cost) best = path;
            }
            if (best == null || best.Cost <= hero.ActionPoints) return best;
            var cost = 0;
            var destination = best.Cells[0];
            for (var i = 1; i < best.Cells.Count; ++i)
            {
                cost += host.Map.Board.GetCell(best.Cells[i]).MoveCost;
                if (cost > hero.ActionPoints) break;
                destination = best.Cells[i];
            }
            return destination == best.Cells[0] ? null : director.PreviewHeroMove(destination);
        }

        private static async UniTask ClickHud(Mouse mouse, UnityEngine.UI.Button button)
        {
            Require(button.gameObject.activeInHierarchy && button.interactable, "Combat HUD button is available");
            var rect = button.GetComponent<RectTransform>();
            var point = RectTransformUtility.WorldToScreenPoint(null, rect.TransformPoint(rect.rect.center));
            InputSystem.QueueStateEvent(mouse, new MouseState { position = point });
            await UniTask.DelayFrame(2);
            InputSystem.QueueStateEvent(mouse, new MouseState { position = point }.WithButton(MouseButton.Left));
            await UniTask.DelayFrame(2);
            InputSystem.QueueStateEvent(mouse, new MouseState { position = point });
            await UniTask.DelayFrame(2);
        }

        private static async UniTask WaitFor(Func<bool> condition, string message, int timeout)
        {
            using var source = new CancellationTokenSource(timeout);
            await UniTask.WaitUntil(condition, cancellationToken: source.Token);
        }

        private static void Require(bool value, string message)
        {
            if (!value) throw new InvalidOperationException(message);
        }
    }
}
