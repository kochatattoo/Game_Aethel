using System;
using System.IO;
using System.Linq;
using CodeBase.TacticsSandbox;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CodeBase.EditorTools
{
    public static class TacticsSandboxTools
    {
        public const string ScenePath = "Assets/_project/Scenes/Prototypes/TacticsSandbox.unity";

        [MenuItem("Tools/PetProject/Tactics/Open Sandbox Scene")]
        public static void OpenScene()
        {
            RequireCleanEditMode();
            EnsureScene();
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            TacticsSandboxWindow.Open();
        }

        public static void RequireCleanEditMode()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling || BuildPipeline.isBuildingPlayer)
                throw new InvalidOperationException("Editor is busy");
            for (var i = 0; i < SceneManager.sceneCount; ++i)
                if (SceneManager.GetSceneAt(i).isDirty || string.IsNullOrEmpty(SceneManager.GetSceneAt(i).path))
                    throw new InvalidOperationException("Save or close unsaved scenes before opening the prototype. No scene was changed.");
        }

        [MenuItem("Tools/PetProject/Tactics/Create Sandbox Asset")]
        public static void CreateAsset()
        {
            RequireCleanEditMode();
            EnsureScene();
        }

        public static void EnsureScene()
        {
            // Never overwrite the user's edits to an existing prototype scene.
            if (File.Exists(ScenePath)) return;
            const string parent = "Assets/_project/Scenes";
            if (!AssetDatabase.IsValidFolder(parent + "/Prototypes")) AssetDatabase.CreateFolder(parent, "Prototypes");
            var previous = SceneManager.GetActiveScene();
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            try
            {
                SceneManager.SetActiveScene(scene);
                new GameObject("Tactics sandbox").AddComponent<TacticsSandboxBehaviour>();
                var cameraRoot = new GameObject("Prototype camera");
                cameraRoot.tag = "MainCamera";
                var camera = cameraRoot.AddComponent<Camera>();
                var center = new Vector3(3.5f, 0, 2);
                camera.transform.position = center + new Vector3(0, 9, -7);
                camera.transform.rotation = Quaternion.LookRotation(center - camera.transform.position);
                camera.orthographic = true;
                camera.orthographicSize = 5.3f;
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = new Color(.035f, .045f, .07f);
                var light = new GameObject("Prototype light").AddComponent<Light>();
                light.type = LightType.Directional;
                light.intensity = 1.3f;
                light.transform.rotation = Quaternion.Euler(50, -30, 0);
                if (!EditorSceneManager.SaveScene(scene, ScenePath)) throw new IOException("Could not save prototype scene");
            }
            finally
            {
                SceneManager.SetActiveScene(previous);
                EditorSceneManager.CloseScene(scene, true);
            }
            Debug.Log("Tactics prototype scene created: " + ScenePath);
        }

        internal static string CaptureSceneSetup() => JsonUtility.ToJson(new SceneSetupData
        {
            Scenes = EditorSceneManager.GetSceneManagerSetup().Select(s =>
                new SceneData { Path = s.path, Loaded = s.isLoaded, Active = s.isActive }).ToArray()
        });

        internal static void RestoreSceneSetup(string json)
        {
            var data = JsonUtility.FromJson<SceneSetupData>(json);
            EditorSceneManager.RestoreSceneManagerSetup(data.Scenes.Select(s =>
                new SceneSetup { path = s.Path, isLoaded = s.Loaded, isActive = s.Active }).ToArray());
        }

        [Serializable] private sealed class SceneSetupData { public SceneData[] Scenes; }
        [Serializable] private sealed class SceneData { public string Path; public bool Loaded, Active; }
    }
}
