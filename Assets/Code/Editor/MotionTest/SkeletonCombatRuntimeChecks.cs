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
using UnityEngine.AI;
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

                MotionEditorInputGate.InputFixtureActive = true;
                var explorationMouse = InputSystem.AddDevice<Mouse>("Skeleton exploration fixture mouse");
                try
                {
                    await ClickTarget(explorationMouse, host, target);
                    await WaitFor(() => host.NormalMoving || director.Active || director.Pending,
                        "Real pointer click starts following", 5000);
                }
                finally
                {
                    InputSystem.RemoveDevice(explorationMouse);
                    MotionEditorInputGate.InputFixtureActive = false;
                }
                Require(director.IsEnemySelected && target.GetComponent<MotionTargetHighlighter>().IsVisible,
                    "Attack target selection enables skeleton highlighter");
                Require(host.Hud.TargetHealthVisible && host.Hud.TargetHealthValue.Contains("5/5"),
                    "Target HUD shows selected skeleton health");
                Require(host.Hud.HeroHealthValue.Contains("12/12"), "HUD shows hero health");
                report.checks.Add("Real pointer click enables target highlighter and health HUD");
                await WaitFor(() => director.Active && director.Snapshot != null, "Detection creates Encounter", 20000);
                Require(director.Snapshot.Actors.Single(a => a.Id == MotionTestController.HeroActorId).ActionPoints == 4,
                    "Initial hero AP comes from world budget");
                report.checks.Add("Click follows skeleton; detection creates Encounter with preserved world AP");

                await WaitFor(() => director.PlayerTurnReady, "First hero turn", 15000);
                await UniTask.DelayFrame(2);
                Require(!host.Hud.ModeButton.interactable, "Free mode exit is disabled during real combat");
                var enemyStart = director.Snapshot.GetActor(MotionEnemyTarget.SkeletonActorId).Position;
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
                await WaitFor(() => director.IsDefeated || director.PlayerTurnReady && director.Snapshot.Round >= 2,
                    "BehaviourTree completes enemy turn", 15000);
                var afterEnemy = director.Snapshot;
                Require(afterEnemy.GetActor(MotionEnemyTarget.SkeletonActorId).Position != enemyStart ||
                    afterEnemy.GetActor(MotionTestController.HeroActorId).Health < 12,
                    "BehaviourTree moves skeleton toward hero or attacks in melee");
                report.checks.Add("BehaviourTree executes movement or attack during enemy turn");
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
                        Require(target.gameObject.activeSelf, "Skeleton corpse stays in the scene");
                        Require(!target.GetComponent<CapsuleCollider>().enabled,
                            "Corpse no longer participates in target selection");
                        Require(target.GetComponent<CodeBase.Enemies.EnemyAnimator>().DeathAnimationCompleted,
                            "Skeleton death animation plays to completion");
                        Require(!target.GetComponent<MotionTargetHighlighter>().IsVisible,
                            "Corpse is not highlighted as an attack target");
                        report.checks.Add("Death animation completes; corpse stays visible with a free logical cell");
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
                    if (GridAdjacency.IsAdjacent(hero.Position, enemy.Position))
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
                await CheckOpeningAttack(host, report);
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

        private static async UniTask CheckOpeningAttack(MotionTestController host, Report report)
        {
            var director = host.EncounterDirector;
            director.ResetCombat();
            await UniTask.DelayFrame(3);
            var target = Object.FindFirstObjectByType<MotionEnemyTarget>();
            Require(target != null, "Opening attack: enemy exists");
            Require(host.Map.Cell(target.transform.position, out var enemyCell),
                "Opening attack: enemy cell exists");
            var candidates = new[]
            {
                new GridPosition(enemyCell.X - 1, enemyCell.Y),
                new GridPosition(enemyCell.X + 1, enemyCell.Y),
                new GridPosition(enemyCell.X, enemyCell.Y - 1),
                new GridPosition(enemyCell.X, enemyCell.Y + 1)
            };
            var heroCell = default(GridPosition);
            var heroPoint = default(Vector3);
            var enemyPoint = default(Vector3);
            var found = false;
            foreach (var candidate in candidates)
            {
                if (!host.Map.Board.Contains(candidate) || !host.Map.Board.GetCell(candidate).Walkable) continue;
                var candidateEnemyPoint = host.Map.World(enemyCell);
                var candidateHeroPoint = host.Map.World(candidate);
                if (!NavMesh.SamplePosition(candidateEnemyPoint, out var enemyNavigation, .25f, NavMesh.AllAreas) ||
                    !NavMesh.SamplePosition(candidateHeroPoint, out var heroNavigation, .25f, NavMesh.AllAreas)) continue;
                heroCell = candidate;
                heroPoint = heroNavigation.position;
                enemyPoint = enemyNavigation.position;
                found = true;
                break;
            }
            Require(found, "Opening attack: adjacent walkable cells exist");
            var enemyAgent = target.GetComponent<NavMeshAgent>();
            enemyAgent.enabled = true;
            host.HeroAgent.enabled = true;
            Require(enemyAgent.Warp(enemyPoint) && host.HeroAgent.Warp(heroPoint),
                "Opening attack: actors placed on NavMesh");
            target.transform.rotation = Quaternion.LookRotation(enemyPoint - heroPoint);
            Physics.SyncTransforms();
            host.WorldActions.Set(MotionTestController.HeroActorId, 6, 6);
            host.WorldActions.Set(MotionEnemyTarget.SkeletonActorId, 4, 6);
            MotionEditorInputGate.InputFixtureActive = true;
            var mouse = InputSystem.AddDevice<Mouse>("Skeleton opening fixture mouse");
            try
            {
                await ClickTarget(mouse, host, target);
                Require(director.Active || director.Pending,
                    "Opening pointer click was not accepted: " + host.Notice +
                    $"; pointerOverUi={host.PointerOverUi}; pointerAllowed={host.PointerInputAllowed}");
                await WaitFor(() => director.Active && director.Snapshot != null,
                    "Opening click creates Encounter", 5000);
                try
                {
                    await WaitFor(() => director.PlayerTurnReady && director.Snapshot.Revision > 0,
                        "Opening alignment resolves first hero command", 15000);
                }
                catch (Exception error)
                {
                    var failedSnapshot = director.Snapshot;
                    throw new InvalidOperationException(
                        $"Opening state: active={director.Active}; pending={director.Pending}; " +
                        $"busy={director.PresentationBusy}; notice={host.Notice}; " +
                        $"current={failedSnapshot?.CurrentActorId}; revision={failedSnapshot?.Revision}", error);
                }
                var snapshot = director.Snapshot;
                Require(snapshot.CurrentActorId == MotionTestController.HeroActorId && snapshot.Revision == 1,
                    "Opening attack: hero acts first and no command runs before the attack");
                Require(snapshot.GetActor(MotionTestController.HeroActorId).ActionPoints == 5,
                    "Opening attack: exactly one AP spent");
                Require(heroCell == snapshot.GetActor(MotionTestController.HeroActorId).Position &&
                    enemyCell == snapshot.GetActor(MotionEnemyTarget.SkeletonActorId).Position,
                    "Opening attack: logical cells match actor placement");
                Require(snapshot.GetActor(MotionEnemyTarget.SkeletonActorId).Definition.Damage == 2,
                    "Skeleton tactical damage is two HP");
                report.checks.Add("Real pointer click behind skeleton starts Encounter with hero opening attack");
                await ClickTarget(mouse, host, target);
                await WaitFor(() => director.Snapshot != null && director.Snapshot.Revision >= 2,
                    "Real pointer click attacks skeleton during combat", 5000);
                Require(director.Snapshot.GetActor(MotionTestController.HeroActorId).ActionPoints == 4,
                    "Combat pointer attack spends one AP");
                report.checks.Add("Real pointer click on adjacent skeleton executes combat Attack");
            }
            finally
            {
                InputSystem.RemoveDevice(mouse);
                MotionEditorInputGate.InputFixtureActive = false;
            }
            director.ResetCombat();
            await UniTask.DelayFrame(3);
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

        private static async UniTask ClickTarget(Mouse mouse, MotionTestController host, MotionEnemyTarget target)
        {
            var collider = target.GetComponent<CapsuleCollider>();
            Require(collider != null && collider.enabled, "Skeleton selection capsule is enabled");
            InputSystem.QueueStateEvent(mouse, new MouseState { position = TargetScreenPoint(host, collider) });
            await UniTask.DelayFrame(2);
            InputSystem.QueueStateEvent(mouse, new MouseState { position = TargetScreenPoint(host, collider) }
                .WithButton(MouseButton.Left));
            await UniTask.DelayFrame(2);
            InputSystem.QueueStateEvent(mouse, new MouseState { position = TargetScreenPoint(host, collider) });
            await UniTask.DelayFrame(2);
        }

        private static Vector2 TargetScreenPoint(MotionTestController host, CapsuleCollider collider)
        {
            var point = host.GameCamera.WorldToScreenPoint(collider.bounds.center);
            Require(point.z > 0, "Skeleton selection capsule is visible to camera");
            return point;
        }

        private static async UniTask ClickHud(Mouse mouse, UnityEngine.UI.Button button)
        {
            Require(button.gameObject.activeInHierarchy && button.interactable, "Combat HUD button is available");
            var rect = button.GetComponent<RectTransform>();
            var point = RectTransformUtility.WorldToScreenPoint(null, rect.TransformPoint(rect.rect.center));
            await ClickScreen(mouse, point);
        }

        private static async UniTask ClickScreen(Mouse mouse, Vector2 point)
        {
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
            try
            {
                await UniTask.WaitUntil(condition, cancellationToken: source.Token);
            }
            catch (OperationCanceledException)
            {
                throw new InvalidOperationException("Runtime check timeout: " + message);
            }
        }

        private static void Require(bool value, string message)
        {
            if (!value) throw new InvalidOperationException(message);
        }
    }
}
