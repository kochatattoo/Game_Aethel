using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.Compilation;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEngine;

namespace CodeBase.EditorTools
{
    /// <summary>Repeatable checks that do not change the user's scenes or saves.</summary>
    public static class RefactoringValidation
    {
        private const string Output = "Library/Refactor/";

        [MenuItem("Tools/PetProject/Refactoring/Run Regression Checks")]
        public static void RunChecks()
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("Stop Play mode first");
            var previousScene = SceneManager.GetActiveScene();
            var isolatedScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            SceneManager.SetActiveScene(isolatedScene);
            try
            {
                var checks = new List<Check>();
                RunFixture("PetProject.Character.Tests", "PetProject.Character.Tests.CharacterMigrationTests", checks);
                RunFixture("PetProject.SaveStorage.Tests", "PetProject.SaveStorage.Tests.JsonSlotStoreTests", checks);
                RunFixture("PetProject.Architecture.Tests", "PetProject.Architecture.Tests.ArchitectureRegressionTests", checks);
                RunFixture("PetProject.Architecture.Tests", "PetProject.Architecture.Tests.LevelDataValidationTests", checks);
                RunFixture("PetProject.Tactics.Tests", "PetProject.Tactics.Tests.GridRulesTests", checks);
                RunFixture("PetProject.Tactics.Tests", "PetProject.Tactics.Tests.RandomRulesTests", checks);
                RunFixture("PetProject.Tactics.Tests", "PetProject.Tactics.Tests.EncounterTests", checks);
                try { SaveStorageSmoke.Checks(); checks.Add(new Check { name = "Save adapters (4 assertions)", success = true }); }
                catch (Exception error) { checks.Add(new Check { name = "Save adapters", error = error.ToString() }); }

                var assemblies = CompilationPipeline.GetAssemblies(AssembliesType.Editor)
                    .Where(a => a.sourceFiles.Any(IsOwnSource)).ToArray();
                foreach (var assembly in CompilationPipeline.GetAssemblies(AssembliesType.Player).Where(a => a.sourceFiles.Any(IsOwnSource)))
                {
                    if (assembly.name.StartsWith("Assembly-CSharp", StringComparison.Ordinal))
                        checks.Add(new Check { name = "Explicit assembly: " + assembly.name, error = string.Join(", ", assembly.sourceFiles.Where(IsOwnSource)) });
                    if ((assembly.flags & AssemblyFlags.EditorAssembly) == 0)
                        foreach (var dependency in assembly.assemblyReferences.Where(a => (a.flags & AssemblyFlags.EditorAssembly) != 0))
                            checks.Add(new Check { name = "Runtime dependency: " + assembly.name, error = dependency.name });
                }
                var missing = new List<string>();
                foreach (var guid in AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/_project" }))
                {
                    var path = AssetDatabase.GUIDToAssetPath(guid);
                    var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                    if (prefab == null) continue;
                    foreach (var node in prefab.GetComponentsInChildren<Transform>(true))
                        if (GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(node.gameObject) > 0)
                            missing.Add(path + ": " + node.name);
                }
                checks.Add(new Check { name = "Own prefab script references", success = missing.Count == 0, error = string.Join("\n", missing) });
                var result = new ValidationResult
                {
                    utc = DateTime.UtcNow.ToString("O"),
                    passed = checks.Count(c => c.success), total = checks.Count,
                    checks = checks.ToArray(),
                    assemblies = assemblies.Select(a => new AssemblyInfo
                    {
                        name = a.name,
                        editor = (a.flags & AssemblyFlags.EditorAssembly) != 0,
                        sources = a.sourceFiles.Where(IsOwnSource).OrderBy(s => s).ToArray(),
                        references = a.assemblyReferences.Select(r => r.name).OrderBy(s => s).ToArray()
                    }).OrderBy(a => a.name).ToArray()
                };
                Directory.CreateDirectory(Output);
                File.WriteAllText(Output + "validation.json", JsonUtility.ToJson(result, true));
                if (result.passed != result.total)
                    throw new InvalidOperationException(string.Join("\n", checks.Where(c => !c.success).Select(c => c.name + ": " + c.error)));
                Debug.Log($"Refactoring checks: {result.passed}/{result.total}; assemblies: {assemblies.Length}");
            }
            finally
            {
                SceneManager.SetActiveScene(previousScene);
                EditorSceneManager.CloseScene(isolatedScene, true);
            }
        }

        private static bool IsOwnSource(string path) => path.Replace('\\', '/').StartsWith("Assets/_project/CodeBase/", StringComparison.Ordinal);

        private static void RunFixture(string assemblyName, string typeName, List<Check> checks)
        {
            var assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().Name == assemblyName);
            var type = assembly?.GetType(typeName);
            if (type == null) { checks.Add(new Check { name = assemblyName, error = "Test assembly not loaded" }); return; }
            var tests = type.GetMethods().Where(m => HasAttribute(m, "TestAttribute")).ToArray();
            foreach (var test in tests)
            {
                var fixture = Activator.CreateInstance(type);
                var check = new Check { name = type.Name + "." + test.Name };
                try
                {
                    foreach (var setup in type.GetMethods().Where(m => HasAttribute(m, "SetUpAttribute"))) setup.Invoke(fixture, null);
                    if (test.ReturnType != typeof(void)) throw new InvalidOperationException("Use Unity Test Runner for asynchronous fixtures");
                    test.Invoke(fixture, null);
                    check.success = true;
                }
                catch (Exception error) { check.error = (error.InnerException ?? error).ToString(); }
                finally
                {
                    try { foreach (var cleanup in type.GetMethods().Where(m => HasAttribute(m, "TearDownAttribute"))) cleanup.Invoke(fixture, null); }
                    catch (Exception error) { check.success = false; check.error += error.ToString(); }
                }
                checks.Add(check);
            }
        }

        private static bool HasAttribute(MethodInfo method, string name) =>
            method.GetCustomAttributes(false).Any(a => a.GetType().FullName == "NUnit.Framework." + name);

        [MenuItem("Tools/PetProject/Refactoring/Build Windows Development")]
        public static void QueueBuild()
        {
            if (EditorApplication.isPlaying || BuildPipeline.isBuildingPlayer) throw new InvalidOperationException("Editor is busy");
            EditorApplication.delayCall += () => Build(false);
        }

        [MenuItem("Tools/PetProject/Refactoring/Build Windows Clean Development")]
        public static void QueueCleanBuild()
        {
            if (EditorApplication.isPlaying || BuildPipeline.isBuildingPlayer) throw new InvalidOperationException("Editor is busy");
            EditorApplication.delayCall += () => Build(true);
        }

        private static void Build(bool clean)
        {
            var folderName = clean ? "RefactoringCleanSmoke" : "RefactoringSmoke";
            var buildFolder = Path.GetFullPath(Path.Combine(Application.dataPath, "../Builds/" + folderName));
            Directory.CreateDirectory(buildFolder);
            var result = new PlayerBuildResult
            {
                utc = DateTime.UtcNow.ToString("O"),
                cleanBuildCache = clean,
                outputPath = Path.Combine(buildFolder, "PetProject.exe"),
                scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray()
            };
            try
            {
                var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
                {
                    scenes = result.scenes,
                    locationPathName = result.outputPath,
                    target = BuildTarget.StandaloneWindows64,
                    options = BuildOptions.Development | (clean ? BuildOptions.CleanBuildCache : BuildOptions.None)
                });
                if (report == null) throw new InvalidOperationException("Unity returned no build report; inspect the Editor console");
                result.result = report.summary.result.ToString();
                result.errors = report.summary.totalErrors;
                result.warnings = report.summary.totalWarnings;
                result.seconds = report.summary.totalTime.TotalSeconds;
                result.messages = report.steps.SelectMany(s => s.messages)
                    .Where(m => m.type == LogType.Error || m.type == LogType.Exception || m.type == LogType.Warning)
                    .Select(m => m.content).ToArray();
            }
            catch (Exception error) { result.result = "Exception"; result.messages = new[] { error.ToString() }; }
            File.WriteAllText(Output + (clean ? "player-build-clean.json" : "player-build.json"), JsonUtility.ToJson(result, true));
            Debug.Log("Refactoring player build: " + result.result);
        }

        [Serializable] private sealed class Check { public string name, error; public bool success; }
        [Serializable] private sealed class ValidationResult { public string utc; public int passed, total; public Check[] checks; public AssemblyInfo[] assemblies; }
        [Serializable] private sealed class AssemblyInfo { public string name; public bool editor; public string[] sources, references; }
        [Serializable] private sealed class PlayerBuildResult
        {
            public string utc, result, outputPath;
            public bool cleanBuildCache;
            public int errors, warnings;
            public double seconds;
            public string[] messages, scenes;
        }
    }
}

