using System;
using System.IO;
using CodeBase.Hero;
using CodeBase.MotionTest;
using Unity.Cinemachine;
using Unity.Cinemachine.TargetTracking;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace CodeBase.EditorTools
{
    public static class MotionTestSceneTools
    {
        public const string ScenePath = "Assets/_project/Scenes/TEST/Test_MotionScene.unity";
        public const string DataRoot = "Assets/_project/StaticData/Tests/Motion";
        public const string HeroPath = "Assets/_project/Prefabs/Tests/MotionTestHero.prefab";
        public const string InputPath = "Assets/_project/InputSystem/Tests/MotionTest.inputactions";

        [InitializeOnLoadMethod]
        private static void RegisterUndo()
        {
            Undo.undoRedoPerformed -= InvalidateBakeAfterUndo;
            Undo.undoRedoPerformed += InvalidateBakeAfterUndo;
        }
        private static void InvalidateBakeAfterUndo()
        {
            // NavMeshData is an external generated asset; Undo of geometry requires a fresh bake.
            if (EditorApplication.isPlaying) return;
            var world = Object.FindFirstObjectByType<MotionTestWorld>();
            if (world == null) return;
            world.BakedSignature = ""; EditorUtility.SetDirty(world);
            EditorSceneManager.MarkSceneDirty(world.gameObject.scene);
        }

        [MenuItem("Tools/PetProject/Motion/Open Test Motion Scene")]
        public static void Open()
        {
            TacticsSandboxTools.RequireCleanEditMode();
            if (!File.Exists(ScenePath)) Create();
            else EditorSceneManager.OpenScene(ScenePath);
            MotionTestWindow.Open();
        }

        public static void Create()
        {
            TacticsSandboxTools.RequireCleanEditMode();
            if (File.Exists(ScenePath)) throw new InvalidOperationException("The motion scene already exists; it was not overwritten");
            Directory.CreateDirectory(DataRoot);
            Directory.CreateDirectory("Assets/_project/Prefabs/Tests");
            Directory.CreateDirectory("Assets/_project/InputSystem/Tests");
            AssetDatabase.Refresh();
            var config = AssetDatabase.LoadAssetAtPath<MotionTestConfig>(DataRoot + "/MotionTestConfig.asset");
            if (config == null)
            {
                config = ScriptableObject.CreateInstance<MotionTestConfig>();
                config.Floor = MaterialAsset("Floor", new Color(.1f, .16f, .2f));
                config.Obstacle = MaterialAsset("Obstacle", new Color(.48f, .65f, .73f));
                config.Ramp = MaterialAsset("Ramp", new Color(.95f, .65f, .25f));
                config.Platform = MaterialAsset("Platform", new Color(.47f, .34f, .25f));
                config.Expensive = MaterialAsset("CostTwo", new Color(.38f, .32f, .48f));
                config.Decoration = MaterialAsset("Sphere", new Color(.3f, .65f, .57f));
                config.GridMaterial = new Material(Shader.Find("PetProject/Motion Grid")) { name = "Motion grid" };
                AssetDatabase.CreateAsset(config.GridMaterial, DataRoot + "/Grid.mat");
                AssetDatabase.CreateAsset(config, DataRoot + "/MotionTestConfig.asset");
            }
            var hero = CreateHero();
            var input = CreateInput();
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var root = new GameObject("Test Motion — controller");
            var host = root.AddComponent<MotionTestController>(); host.TestBudget = config.ActionBudget;
            var world = new GameObject("Motion environment").AddComponent<MotionTestWorld>(); world.Config = config;
            host.World = world; host.HeroPrefab = hero; host.InputAsset = input;
            world.Generate();
            var sun = new GameObject("Directional light").AddComponent<Light>();
            sun.type = LightType.Directional; sun.intensity = 1.6f; sun.color = new Color(1, .94f, .84f);
            sun.shadows = LightShadows.Soft; sun.transform.rotation = Quaternion.Euler(48, -35, 0);
            RenderSettings.sun = sun; RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(.45f, .57f, .7f);
            RenderSettings.ambientEquatorColor = new Color(.3f, .37f, .43f); RenderSettings.ambientGroundColor = new Color(.18f, .2f, .24f);
            var camera = new GameObject("Main Camera").AddComponent<Camera>(); camera.tag = "MainCamera";
            camera.orthographic = true; camera.orthographicSize = 14; camera.farClipPlane = 400;
            camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = new Color(.025f, .045f, .07f);
            camera.gameObject.AddComponent<AudioListener>(); camera.gameObject.AddComponent<CinemachineBrain>();
            var pivot = new GameObject("Camera focus").transform; pivot.position = config.Spawn + Vector3.up * 1.8f;
            var cm = new GameObject("Cinemachine isometric camera").AddComponent<CinemachineCamera>();
            cm.Follow = pivot; cm.LookAt = pivot; cm.Lens.ModeOverride = LensSettings.OverrideModes.Orthographic;
            cm.Lens.OrthographicSize = 14; cm.Lens.FarClipPlane = 400;
            var orbital = cm.gameObject.AddComponent<CinemachineOrbitalFollow>();
            orbital.OrbitStyle = CinemachineOrbitalFollow.OrbitStyles.Sphere; orbital.Radius = 38;
            orbital.TrackerSettings.BindingMode = BindingMode.WorldSpace; orbital.TrackerSettings.PositionDamping = Vector3.zero;
            orbital.HorizontalAxis.Value = 45; orbital.VerticalAxis.Value = 45;
            orbital.VerticalAxis.Range = new Vector2(30, 65); orbital.RadialAxis.Value = 1;
            orbital.HorizontalAxis.Recentering.Enabled = false; orbital.VerticalAxis.Recentering.Enabled = false;
            cm.gameObject.AddComponent<CinemachineRotationComposer>();
            var cameraRig = root.AddComponent<MotionCameraController>();
            cameraRig.VirtualCamera = cm; cameraRig.Orbit = orbital; cameraRig.Pivot = pivot;
            host.GameCamera = camera; host.CameraRig = cameraRig;
            Bake(world);
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            Selection.activeGameObject = root;
            SceneView.lastActiveSceneView?.LookAt(config.Spawn, Quaternion.Euler(45, 45, 0), 24);
            Debug.Log("Test_MotionScene created: 100 x 100, existing hero, NavMesh and Cinemachine 3");
        }

        private static Material MaterialAsset(string name, Color color)
        {
            var material = new Material(Shader.Find("Universal Render Pipeline/Lit")) { name = "Motion " + name };
            material.SetColor("_BaseColor", color); material.SetFloat("_Smoothness", .15f);
            AssetDatabase.CreateAsset(material, DataRoot + "/" + name + ".mat"); return material;
        }
        private static GameObject CreateHero()
        {
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(HeroPath); if (existing != null) return existing;
            var original = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_project/Resources_moved/Hero/Hero.prefab");
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(original);
            try
            {
                PrefabUtility.UnpackPrefabInstance(instance, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
                instance.name = "MotionTestHero";
                foreach (var component in instance.GetComponentsInChildren<Component>(true))
                {
                    if (component == null || component is Transform || component is Renderer || component is MeshFilter ||
                        component is Animator || component is NavMeshAgent || component is HeroAnimator || component is HeroPathFollower) continue;
                    Object.DestroyImmediate(component);
                }
                foreach (var child in instance.GetComponentsInChildren<Transform>(true)) child.gameObject.layer = 2;
                var animator = instance.GetComponent<Animator>(); animator.applyRootMotion = false; animator.fireEvents = false;
                animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
                var follower = instance.GetComponent<HeroPathFollower>(); follower.enabled = true;
                return PrefabUtility.SaveAsPrefabAsset(instance, HeroPath);
            }
            finally { Object.DestroyImmediate(instance); }
        }
        private static InputActionAsset CreateInput()
        {
            var existing = AssetDatabase.LoadAssetAtPath<InputActionAsset>(InputPath); if (existing != null) return existing;
            var asset = ScriptableObject.CreateInstance<InputActionAsset>();
            var map = asset.AddActionMap("Motion");
            map.AddAction("Point", InputActionType.PassThrough, "<Mouse>/position");
            map.AddAction("Move", InputActionType.Button, "<Mouse>/leftButton");
            map.AddAction("Look", InputActionType.PassThrough, "<Mouse>/delta");
            map.AddAction("Orbit", InputActionType.Button, "<Mouse>/rightButton");
            map.AddAction("Zoom", InputActionType.PassThrough, "<Mouse>/scroll/y");
            map.AddAction("YawLeft", InputActionType.Button, "<Keyboard>/q");
            map.AddAction("YawRight", InputActionType.Button, "<Keyboard>/e");
            map.AddAction("Focus", InputActionType.Button, "<Keyboard>/f");
            File.WriteAllText(InputPath, asset.ToJson()); Object.DestroyImmediate(asset);
            AssetDatabase.ImportAsset(InputPath); return AssetDatabase.LoadAssetAtPath<InputActionAsset>(InputPath);
        }

        public static void Rebuild(MotionTestWorld world)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Rebuild is only available in Edit Mode");
            Undo.IncrementCurrentGroup(); var undoGroup = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName("Rebuild motion terrain");
            // DestroyObjectImmediate flushes differential RecordObject entries. Capture the complete
            // owner before removing its referenced root, otherwise Undo restores objects but loses the binding.
            Undo.RegisterCompleteObjectUndo(world, "Rebuild motion terrain");
            if (world.GeneratedRoot != null) Undo.DestroyObjectImmediate(world.GeneratedRoot.gameObject);
            world.GeneratedRoot = null; world.Generate();
            Undo.RegisterCreatedObjectUndo(world.GeneratedRoot.gameObject, "Create motion terrain");
            Bake(world); EditorSceneManager.MarkSceneDirty(world.gameObject.scene); Undo.CollapseUndoOperations(undoGroup);
        }
        [MenuItem("Tools/PetProject/Motion/Rebuild And Save Test Scene")]
        public static void RebuildAndSave()
        {
            var host = Object.FindFirstObjectByType<MotionTestController>();
            if (host == null || host.gameObject.scene.path != ScenePath) throw new InvalidOperationException("Open Test_MotionScene first");
            Rebuild(host.World); EditorSceneManager.SaveScene(host.gameObject.scene);
        }
        private static void Bake(MotionTestWorld world)
        {
            Physics.SyncTransforms();
            world.Surface.BuildNavMesh();
            var path = DataRoot + "/MotionNavMesh.asset";
            var existing = AssetDatabase.LoadAssetAtPath<NavMeshData>(path);
            var built = world.Surface.navMeshData;
            if (existing == null) AssetDatabase.CreateAsset(built, path);
            else
            {
                world.Surface.RemoveData(); EditorUtility.CopySerialized(built, existing);
                world.Surface.navMeshData = existing; Object.DestroyImmediate(built); world.Surface.AddData();
                EditorUtility.SetDirty(existing);
            }
            world.BakedSignature = world.Signature; EditorUtility.SetDirty(world);
            AssetDatabase.SaveAssets();
        }
    }
}
