using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using CodeBase.MotionTest;
using Cysharp.Threading.Tasks;
using PetProject.Tactics;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using Zenject;
using Object = UnityEngine.Object;

namespace CodeBase.EditorTools
{
    public static class MotionRuntimeChecks
    {
        private const string Key = "PetProject.MotionChecks";
        public const string ReportPath = "Library/MotionTest/runtime-checks.json";
        [Serializable] private sealed class Report
        {
            public bool pending, success, saveIsolation;
            public string error;
            public List<string> checks = new List<string>();
            public double gridMilliseconds, previewMilliseconds;
            public int walkableCells;
        }
        [InitializeOnLoadMethod] private static void Register()
        {
            EditorApplication.playModeStateChanged -= Changed;
            EditorApplication.playModeStateChanged += Changed;
        }
        [MenuItem("Tools/PetProject/Motion/Run Runtime Checks")]
        public static void Run()
        {
            TacticsSandboxTools.RequireCleanEditMode();
            if (!File.Exists(MotionTestSceneTools.ScenePath)) MotionTestSceneTools.Create();
            EditorSceneManager.OpenScene(MotionTestSceneTools.ScenePath);
            Directory.CreateDirectory("Library/MotionTest");
            File.WriteAllText(ReportPath, JsonUtility.ToJson(new Report { pending = true }, true));
            SessionState.SetString(Key + ".SaveHash", SaveHash());
            SessionState.SetBool(Key, true); EditorApplication.isPlaying = true;
        }
        private static void Changed(PlayModeStateChange state)
        {
            if (!SessionState.GetBool(Key, false)) return;
            if (state == PlayModeStateChange.EnteredPlayMode) Check().Forget(Debug.LogException);
            if (state != PlayModeStateChange.EnteredEditMode) return;
            var report = JsonUtility.FromJson<Report>(File.ReadAllText(ReportPath));
            report.saveIsolation = SaveHash() == SessionState.GetString(Key + ".SaveHash", "");
            report.success &= report.saveIsolation; report.pending = false;
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
                Require(host != null && host.Ready, "Standalone scene initialized: " + host?.Error);
                Require(Object.FindObjectsByType<ProjectContext>(FindObjectsSortMode.None).Length == 0, "No campaign DI container");
                Require(host.Map.Board.Width == 100 && host.Map.Board.Height == 100, "100 x 100 board");
                report.gridMilliseconds = host.Map.BuildMilliseconds; report.walkableCells = host.Map.WalkableCount;
                report.checks.Add("100x100 scene, existing hero and independent input initialized without campaign bootstrap");
                await CheckCameraInput(host);
                report.checks.Add("Input System keyboard yaw, right-button orbit, wheel zoom and focus reset drive Cinemachine");
                host.ResetHero();
                Require(host.MoveNormally(new Vector3(-16.5f, 0, -13.5f)), "Normal destination accepted");
                await UniTask.DelayFrame(2); Require(host.IsAnimating, "Run animation active while walking");
                await Settled(host);
                await UniTask.DelayFrame(2); Require(!host.IsAnimating, "Idle after arrival");
                report.checks.Add("NavMesh movement reaches destination; existing Move animation turns on and off");
                var previous = host.Hero.transform.position;
                Require(!host.MoveNormally(new Vector3(-30, 0, 20)), "Isolated room rejected");
                Require(!host.MoveNormally(new Vector3(80, 0, 80)), "Outside world rejected");
                Require(Vector3.Distance(previous, host.Hero.transform.position) < .01f, "Rejected click has no movement");
                report.checks.Add("Unreachable room and outside-world destinations fail without teleporting");
                Require(host.SetCombat(true), "Enter combat requested"); await Settled(host);
                Require(host.Combat && host.Session.Actions == 6, "Combat session starts with six actions");
                var start = host.Session.Position; var one = new GridPosition(start.X + 1, start.Y);
                var revision = host.Session.Revision;
                Require(host.SetPreview(one).Cost == 1 && host.Session.Revision == revision, "Hover does not change state");
                Require(host.TryMove(one), "One-cell movement accepted");
                Require(!host.TryMove(one), "Input while moving blocked");
                await Settled(host);
                Require(host.Session.Actions == 5 && Vector3.Distance(host.Hero.transform.position, host.Map.World(one)) < .01f, "Exact endpoint and action charge");
                var far = new GridPosition(one.X, one.Y - 12);
                var before = host.Session.Revision;
                Require(!host.TryMove(far) && host.Session.Revision == before && host.Session.Actions == 5, "Overbudget move atomic");
                host.NewTurn(); Require(host.Session.Actions == 6, "New test turn restores budget");
                report.checks.Add("Grid preview, exact animated path, single AP charge, busy gate, budget refusal and new turn pass");
                Capture(host, "Library/MotionTest/combat-preview.png");
                Require(host.TryMove(start), "Return move accepted"); host.SetCombat(false);
                await Settled(host); await UniTask.DelayFrame(2);
                Require(!host.Combat && host.MoveNormally(new Vector3(-16.5f, 0, -15.5f)), "Queued mode exit restores NavMesh movement");
                await Settled(host);
                report.checks.Add("Mode exit during accepted movement waits and restores normal navigation");
                host.ResetHero(); host.TestBudget = 12;
                Require(host.MoveNormally(new Vector3(8.5f, 2, 9.5f)), "Upper platform path accepted");
                await Settled(host, 18000);
                Require(host.Hero.transform.position.y > 1.8f, "NavMesh climbed gentle ramp");
                Require(host.SetCombat(true), "Platform combat requested"); await Settled(host);
                var descending = new GridPosition(host.Session.Position.X, host.Session.Position.Y - 8);
                Require(host.SetPreview(descending).Found, "Grid connects platform to ramp");
                Require(host.TryMove(descending), "Ramp movement accepted"); await Settled(host);
                Require(Vector3.Distance(host.Hero.transform.position, host.Map.World(descending)) < .01f && host.Error == null, "Grid ramp motion follows support");
                report.checks.Add("Normal navigation climbs platform; tactical path descends along actual ramp surface");
                host.SetPreview(new GridPosition(descending.X, descending.Y + 3));
                await UniTask.DelayFrame(3); Capture(host, "Library/MotionTest/ramp-preview.png");
                report.previewMilliseconds = host.PreviewMilliseconds;
                for (var i = 0; i < 20; ++i)
                {
                    host.SetCombat(false); host.SetCombat(true); await Settled(host);
                }
                Require(Object.FindObjectsByType<MotionTestController>(FindObjectsSortMode.None).Length == 1, "One controller after transitions");
                Require(host.Error == null, "No transition error");
                report.checks.Add("Twenty mode cycles keep a single controller and valid movement state");
                host.ResetHero(); host.TestBudget = 6;
                await UniTask.DelayFrame(2); Capture(host, "Library/MotionTest/normal-preview.png");
                report.success = true;
            }
            catch (Exception error) { report.error = error.ToString(); Debug.LogException(error); }
            finally
            {
                File.WriteAllText(ReportPath, JsonUtility.ToJson(report, true));
                EditorApplication.isPlaying = false;
            }
        }
        private static async UniTask Settled(MotionTestController host, int timeout = 8000)
        {
            await UniTask.DelayFrame(2);
            using var source = new CancellationTokenSource(timeout);
            await UniTask.WaitUntil(() => host.Error != null || (!host.Busy && !host.NormalMoving), cancellationToken: source.Token);
            Require(host.Error == null, host.Error ?? "Motion settled");
        }
        private static async UniTask CheckCameraInput(MotionTestController host)
        {
            MotionEditorInputGate.InputFixtureActive = true;
            var mouse = InputSystem.AddDevice<Mouse>("Motion fixture mouse");
            var keyboard = InputSystem.AddDevice<Keyboard>("Motion fixture keyboard");
            var pointer = new Vector2(Screen.width * .5f, Screen.height * .5f);
            try
            {
                await UniTask.DelayFrame(2);
                host.CameraRig.Focus();
                InputSystem.QueueStateEvent(keyboard, new KeyboardState(UnityEngine.InputSystem.Key.Q));
                await UniTask.DelayFrame(2);
                Require(Mathf.Abs(host.CameraRig.Yaw) < .1f, "Q rotates yaw by 45 degrees");
                InputSystem.QueueStateEvent(keyboard, new KeyboardState());
                InputSystem.QueueStateEvent(mouse, new MouseState { position = pointer, scroll = new Vector2(0, 1) });
                await UniTask.DelayFrame(2);
                Require(host.CameraRig.Zoom < 13.5f, "Normalized wheel notch changes orthographic zoom");
                InputSystem.QueueStateEvent(mouse, new MouseState { position = pointer, delta = new Vector2(40, 0) }.WithButton(MouseButton.Right));
                await UniTask.DelayFrame(2);
                Require(host.CameraRig.Yaw > 1, "Right-button mouse delta rotates camera");
                InputSystem.QueueStateEvent(mouse, new MouseState { position = pointer });
                InputSystem.QueueStateEvent(keyboard, new KeyboardState(UnityEngine.InputSystem.Key.F));
                await UniTask.DelayFrame(2);
                Require(Mathf.Abs(host.CameraRig.Yaw - 45) < .1f && Mathf.Abs(host.CameraRig.Zoom - 14) < .1f, "F restores view");
            }
            finally
            {
                InputSystem.RemoveDevice(keyboard); InputSystem.RemoveDevice(mouse);
                MotionEditorInputGate.InputFixtureActive = false;
            }
        }
        private static void Require(bool value, string message) { if (!value) throw new InvalidOperationException(message); }
        private static string SaveHash()
        {
            var root = Application.persistentDataPath; var text = new StringBuilder();
            if (Directory.Exists(root))
                foreach (var path in Directory.GetFiles(root, "*", SearchOption.AllDirectories).OrderBy(p => p, StringComparer.Ordinal))
                {
                    if (!path.EndsWith(".json", StringComparison.OrdinalIgnoreCase) && !path.EndsWith(".bak", StringComparison.OrdinalIgnoreCase)) continue;
                    text.Append(path).Append(Convert.ToBase64String(SHA256.Create().ComputeHash(File.ReadAllBytes(path))));
                }
            return Convert.ToBase64String(SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(text.ToString())));
        }
        public static void Capture(MotionTestController host, string path)
        {
            var camera = host.GameCamera; var oldTarget = camera.targetTexture; var oldActive = RenderTexture.active;
            var texture = RenderTexture.GetTemporary(1280, 800, 24);
            var image = new Texture2D(1280, 800, TextureFormat.RGB24, false);
            try
            {
                camera.targetTexture = texture; camera.Render(); RenderTexture.active = texture;
                image.ReadPixels(new Rect(0, 0, 1280, 800), 0, 0); image.Apply();
                File.WriteAllBytes(path, image.EncodeToPNG());
            }
            finally { camera.targetTexture = oldTarget; RenderTexture.active = oldActive; RenderTexture.ReleaseTemporary(texture); Object.DestroyImmediate(image); }
        }
    }
}
