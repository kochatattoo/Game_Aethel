using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using CodeBase.Infrastructure.Services.SaveLoad;
using CodeBase.TacticsSandbox;
using Cysharp.Threading.Tasks;
using PetProject.Tactics;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Zenject;
using Object = UnityEngine.Object;

namespace CodeBase.EditorTools
{
    public static class TacticsRuntimeSmoke
    {
        private const string Key = "PetProject.Tactics.Smoke";
        private const string Output = "Library/Tactics/runtime-smoke.json";

        [InitializeOnLoadMethod]
        private static void Register()
        {
            EditorApplication.playModeStateChanged -= OnPlayMode;
            EditorApplication.playModeStateChanged += OnPlayMode;
        }

        [MenuItem("Tools/PetProject/Tactics/Run Runtime Checks")]
        public static void Start()
        {
            TacticsSandboxTools.RequireCleanEditMode();
            if (SessionState.GetBool(Key, false)) throw new InvalidOperationException("A tactics smoke is already pending");
            TacticsSandboxTools.EnsureScene();
            var scenes = TacticsSandboxTools.CaptureSceneSetup();
            SessionState.SetString(Key + ".Scenes", scenes);
            SessionState.SetString(Key + ".Progress", ProgressHash());
            SessionState.SetString(Key + ".Files", SaveFilesHash());
            Directory.CreateDirectory("Library/Tactics");
            File.WriteAllText(Output, "{\"pending\":true}");
            try
            {
                EditorSceneManager.OpenScene(TacticsSandboxTools.ScenePath, OpenSceneMode.Single);
                SessionState.SetBool(Key, true);
                EditorApplication.isPlaying = true;
            }
            catch
            {
                SessionState.SetBool(Key, false);
                TacticsSandboxTools.RestoreSceneSetup(scenes);
                throw;
            }
        }

        private static void OnPlayMode(PlayModeStateChange state)
        {
            if (!SessionState.GetBool(Key, false)) return;
            if (state == PlayModeStateChange.EnteredPlayMode) RunAsync().Forget(Debug.LogException);
            if (state != PlayModeStateChange.EnteredEditMode) return;
            try
            {
                TacticsSandboxTools.RestoreSceneSetup(SessionState.GetString(Key + ".Scenes", ""));
                var result = JsonUtility.FromJson<Result>(File.ReadAllText(Output));
                result.sceneRestored = true;
                result.saveIsolation = ProgressHash() == SessionState.GetString(Key + ".Progress", "") &&
                    SaveFilesHash() == SessionState.GetString(Key + ".Files", "");
                result.success &= result.saveIsolation;
                File.WriteAllText(Output, JsonUtility.ToJson(result, true));
            }
            finally { SessionState.SetBool(Key, false); }
        }

        private static async UniTask RunAsync()
        {
            var result = new Result();
            var checks = new List<string>();
            try
            {
                await UniTask.Yield();
                Require(Object.FindObjectsByType<ProjectContext>(FindObjectsSortMode.None).Length == 0,
                    "Prototype must not bootstrap the campaign DI container");
                // Use a fresh in-memory fixture even if the user customized the saved prototype settings.
                foreach (var previous in Object.FindObjectsByType<TacticsSandboxBehaviour>(FindObjectsSortMode.None))
                    Object.Destroy(previous.gameObject);
                await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);
                var host = new GameObject("Tactics runtime fixture").AddComponent<TacticsSandboxBehaviour>();
                Require(host.IsInitialized && host.ViewActorCount == 2, "Both actor views initialized");
                checks.Add("Standalone scene initializes two actor views without campaign services");

                var command = TacticalCommand.Move("first-move", "hero", new GridPosition(2, 1));
                Require(host.Execute(command).Accepted, "Movement accepted");
                Require(Actor(host, "hero").ActionPoints == 2, "Path spends actions");
                Require(Math.Abs(host.ViewPosition("hero").x - 2) < .001f && Math.Abs(host.ViewPosition("hero").z - 1) < .001f,
                    "View follows authoritative cell");
                var revision = host.Snapshot.Revision; var random = host.Snapshot.RandomState;
                Require(host.Execute(command).IsReplay, "Retry recognized");
                Require(host.Snapshot.Revision == revision && host.Snapshot.RandomState == random && Actor(host, "hero").ActionPoints == 2,
                    "Retry does not spend actions or randomness");
                Require(!host.Execute(TacticalCommand.Move("wall", "hero", new GridPosition(3, 2))).Accepted && host.Snapshot.Revision == revision,
                    "Blocked move has no mutation");
                checks.Add("Movement, view position, command retry and blocked destination are consistent");

                Require(host.Execute(TacticalCommand.EndTurn("hero-end", "hero")).Accepted, "Hero ends turn");
                Require(host.Execute(TacticalCommand.Move("enemy-move", "enemy", new GridPosition(3, 1))).Accepted, "Enemy moves into contact");
                Require(host.Execute(TacticalCommand.Attack("enemy-attack", "enemy", "hero")).Accepted, "Attack accepted");
                Require(Actor(host, "enemy").ActionPoints == 0 && host.Snapshot.CurrentActorId == "enemy", "Zero actions do not auto-end turn");
                Require(host.Execute(TacticalCommand.EndTurn("enemy-end", "enemy")).Accepted && host.Snapshot.Round == 2, "Next round starts");
                checks.Add("Action budget and explicit turn ending advance the round");

                for (var i = 0; i < 100 && !host.Snapshot.IsCompleted; ++i)
                {
                    var active = host.Snapshot.CurrentActorId;
                    var other = host.Snapshot.Actors.Single(a => a.Id != active);
                    var next = Actor(host, active).ActionPoints > 0
                        ? TacticalCommand.Attack("fight-" + i, active, other.Id)
                        : TacticalCommand.EndTurn("fight-" + i, active);
                    Require(host.Execute(next).Accepted, "Fight command accepted");
                }
                Require(host.Snapshot.IsCompleted, "Encounter reaches victory");
                var dead = host.Snapshot.Actors.Single(a => !a.IsAlive);
                Require(!host.ViewActorIsActive(dead.Id), "Defeated view is hidden");
                checks.Add("d20 combat completes, declares a winner and hides the defeated actor");

                for (var i = 0; i < 10; ++i) host.ResetEncounter();
                Require(host.ViewActorCount == 2 && host.Snapshot.Revision == 0 && host.Snapshot.Actors.All(a => a.IsAlive), "Reset reuses two views");
                var before = host.Snapshot.Revision;
                for (var i = 0; i < 10; ++i) await UniTask.Yield();
                Require(host.Snapshot.Revision == before, "Frame updates do not execute game rules");
                checks.Add("Ten resets preserve view count; idle frames do not advance simulation");
                result.success = true;
            }
            catch (Exception error) { result.error = error.ToString(); Debug.LogException(error); }
            finally
            {
                result.utc = DateTime.UtcNow.ToString("O");
                result.checks = checks.ToArray();
                File.WriteAllText(Output, JsonUtility.ToJson(result, true));
                EditorApplication.isPlaying = false;
            }
        }

        private static ActorSnapshot Actor(TacticsSandboxBehaviour host, string id) => host.Snapshot.Actors.Single(a => a.Id == id);
        private static void Require(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
        private static string ProgressHash() => Hash(PlayerPrefs.HasKey("Progress") + ":" + PlayerPrefs.GetString("Progress", ""));
        private static string SaveFilesHash()
        {
            var root = SaveEnvironment.Current.StorageRoot;
            if (!Directory.Exists(root)) return "absent";
            var data = new StringBuilder("exists");
            foreach (var file in Directory.GetFiles(root, "*", SearchOption.AllDirectories).OrderBy(p => p, StringComparer.Ordinal))
                data.Append(file.Substring(root.Length)).Append(':').Append(Hash(Convert.ToBase64String(File.ReadAllBytes(file))));
            return Hash(data.ToString());
        }
        private static string Hash(string value)
        {
            using var algorithm = SHA256.Create();
            return Convert.ToBase64String(algorithm.ComputeHash(Encoding.UTF8.GetBytes(value)));
        }

        [Serializable] private sealed class Result
        {
            public bool success, sceneRestored, saveIsolation;
            public string utc, error;
            public string[] checks;
        }
    }
}
