using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace CodeBase.EditorTools
{
    /// <summary>Удаляет заменённые runtime-компоненты из префабов, сцен и SO-конфигов.</summary>
    public static class EncounterAssetCleanup
    {
        private static readonly HashSet<string> RemovedScriptPaths = new()
        {
            "Assets/Code/Modules/Gameplay/Enemies/AgentMoveToHero.cs",
            "Assets/Code/Modules/Gameplay/Enemies/AnimateAlongAgent.cs",
            "Assets/Code/Modules/Gameplay/Enemies/Attack.cs",
            "Assets/Code/Modules/Gameplay/Enemies/AttackRuntime.cs",
            "Assets/Code/Modules/Gameplay/Enemies/CheckAttackRange.cs",
            "Assets/Code/Modules/Gameplay/Enemies/EnemyDeath.cs",
            "Assets/Code/Modules/Gameplay/Enemies/EnemyHealth.cs",
            "Assets/Code/Modules/Gameplay/Enemies/EnemyVision.cs",
            "Assets/Code/Modules/Gameplay/Enemies/Follow.cs",
            "Assets/Code/Modules/Gameplay/Enemies/IAttack.cs",
            "Assets/Code/Modules/Gameplay/Enemies/PhysicsDebug.cs",
            "Assets/Code/Modules/Gameplay/Enemies/RotateToHero.cs",
            "Assets/Code/Modules/Gameplay/Enemies/TriggerObserver.cs",
            "Assets/Code/Modules/Gameplay/World/EnemySpawners/SpawnPoint.cs",
            "Assets/Code/Modules/Gameplay/Components/Audio/EnemyAudio.cs",
            "Assets/Code/Modules/Gameplay/Hero/HeroFacade.cs",
            "Assets/Code/Modules/Gameplay/Hero/HeroDeath.cs",
            "Assets/Code/Modules/Gameplay/Hero/HeroAttackRuntime.cs",
            "Assets/Code/Modules/Gameplay/Camera/CameraFollow.cs",
            "Assets/Code/Modules/Gameplay/Sensors/BladeSensor.cs",
            "Assets/Code/Modules/Gameplay/Configs/HeroAttackConfig.cs",
            "Assets/Code/Modules/Gameplay/Configs/EnemyAttackConfig.cs",
            "Assets/Code/Modules/Gameplay/Configs/AttackConfig.cs"
        };

        private static readonly string[] RemovedConfigPaths =
        {
            "Assets/Configs/Settings/DragonAttackConfig.asset",
            "Assets/Configs/Settings/DragonHitConfig.asset",
            "Assets/Configs/Settings/HeroAttackConfig.asset",
            "Assets/Configs/Settings/HeroBladeConfig.asset",
            "Assets/Configs/Settings/SkeletonAttackConfig.asset",
            "Assets/Configs/Settings/SkeletonBladeAttackConfig.asset"
        };

        [MenuItem("Tools/PetProject/Encounter/Cleanup Legacy Assets")]
        public static void Run()
        {
            var removed = new List<string>();
            foreach (var path in RemovedConfigPaths)
                if (AssetDatabase.DeleteAsset(path)) removed.Add(path);
            foreach (var guid in AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Prefabs" }))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var root = PrefabUtility.LoadPrefabContents(path);
                try
                {
                    var count = CleanComponents(root.transform);
                    if (count == 0) continue;
                    PrefabUtility.SaveAsPrefabAsset(root, path);
                    removed.Add(path + " (" + count + ")");
                }
                finally { PrefabUtility.UnloadPrefabContents(root); }
            }
            foreach (var guid in AssetDatabase.FindAssets("t:Scene", new[] { "Assets/Scenes" }))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
                var count = 0;
                foreach (var root in scene.GetRootGameObjects()) count += CleanComponents(root.transform);
                if (count == 0) continue;
                EditorSceneManager.SaveScene(scene, path);
                removed.Add(path + " (" + count + ")");
            }
            EditorSceneManager.OpenScene("Assets/Scenes/Bootstrap.unity", OpenSceneMode.Single);
            var missing = FindMissingScripts();
            Directory.CreateDirectory("Library/Encounter");
            File.WriteAllLines("Library/Encounter/asset-cleanup.txt",
                new[] { "Removed: " + removed.Count, "Missing after cleanup: " + missing.Count,
                    string.Join("\n", removed), "MISSING:", string.Join("\n", missing) });
            if (missing.Count > 0) throw new InvalidOperationException("Missing Scripts остались: " + missing.Count);
            Debug.Log("Encounter asset cleanup завершён: удалены legacy-компоненты и конфиги");
        }

        [MenuItem("Tools/PetProject/Encounter/Run Asset Checks")]
        public static void Check()
        {
            var missing = FindMissingScripts();
            Directory.CreateDirectory("Library/Encounter");
            File.WriteAllLines("Library/Encounter/asset-checks.txt", missing);
            if (missing.Count > 0) throw new InvalidOperationException("Missing Scripts: " + missing.Count);
            Debug.Log("Missing Scripts не обнаружены");
        }

        private static int CleanComponents(Transform root)
        {
            var removed = 0;
            foreach (var component in root.GetComponentsInChildren<MonoBehaviour>(true))
            {
                if (component == null) continue;
                var script = MonoScript.FromMonoBehaviour(component);
                var path = AssetDatabase.GetAssetPath(script);
                if (!RemovedScriptPaths.Contains(path)) continue;
                Object.DestroyImmediate(component);
                ++removed;
            }
            foreach (var node in root.GetComponentsInChildren<Transform>(true))
                if (GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(node.gameObject) > 0)
                    removed += GameObjectUtility.RemoveMonoBehavioursWithMissingScript(node.gameObject);
            return removed;
        }

        private static List<string> FindMissingScripts()
        {
            var missing = new List<string>();
            foreach (var guid in AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Prefabs" }))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var root = PrefabUtility.LoadPrefabContents(path);
                try
                {
                    foreach (var node in root.GetComponentsInChildren<Transform>(true))
                        if (GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(node.gameObject) > 0)
                            missing.Add(path + ": " + node.name);
                }
                finally { PrefabUtility.UnloadPrefabContents(root); }
            }
            foreach (var guid in AssetDatabase.FindAssets("t:Scene", new[] { "Assets/Scenes" }))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
                foreach (var root in scene.GetRootGameObjects())
                foreach (var node in root.GetComponentsInChildren<Transform>(true))
                    if (GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(node.gameObject) > 0)
                        missing.Add(path + ": " + node.name);
                EditorSceneManager.CloseScene(scene, true);
            }
            return missing;
        }
    }
}
