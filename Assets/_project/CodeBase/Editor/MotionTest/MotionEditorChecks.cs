using System;
using System.IO;
using CodeBase.MotionTest;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CodeBase.EditorTools
{
    public static class MotionEditorChecks
    {
        [Serializable] private sealed class Result
        {
            public bool success, manualObjectPreserved, undoRestoredRoot, undoInvalidatedBake, redoRestoredRoot;
            public string error;
        }
        [MenuItem("Tools/PetProject/Motion/Run Editor Checks")]
        public static void Run()
        {
            TacticsSandboxTools.RequireCleanEditMode();
            var host = Object.FindFirstObjectByType<MotionTestController>();
            if (host == null || host.gameObject.scene.path != MotionTestSceneTools.ScenePath)
                throw new InvalidOperationException("Open Test_MotionScene first");
            var result = new Result();
            var sentinel = new GameObject("Motion check manual object");
            try
            {
                var previousRoot = host.World.GeneratedRoot;
                MotionTestSceneTools.Rebuild(host.World); Undo.FlushUndoRecordObjects();
                result.manualObjectPreserved = sentinel != null && sentinel.transform.parent == null;
                Require(result.manualObjectPreserved, "Manual object removed by generation");
                Require(host.World.GeneratedRoot != previousRoot, "Root not replaced");
                Undo.PerformUndo();
                result.undoRestoredRoot = host.World.GeneratedRoot != null && host.World.GeneratedRoot == previousRoot;
                result.undoInvalidatedBake = host.World.BakedSignature == "";
                Require(result.undoRestoredRoot && result.undoInvalidatedBake, "Undo did not restore geometry and invalidate NavMesh");
                Undo.PerformRedo();
                result.redoRestoredRoot = host.World.GeneratedRoot != null && host.World.GeneratedRoot != previousRoot;
                Require(result.redoRestoredRoot && host.World.BakedSignature == "", "Redo did not restore generation safely");
                Require(sentinel != null, "Manual object disappeared after Undo/Redo");
                result.success = true;
            }
            catch (Exception error) { result.error = error.ToString(); Debug.LogException(error); }
            finally
            {
                if (sentinel != null) Object.DestroyImmediate(sentinel);
                // External NavMeshData is regenerated after Undo, leaving a valid saved scene for the user.
                MotionTestSceneTools.Rebuild(host.World);
                EditorSceneManager.SaveScene(host.gameObject.scene);
                Directory.CreateDirectory("Library/MotionTest");
                File.WriteAllText("Library/MotionTest/editor-checks.json", JsonUtility.ToJson(result, true));
            }
        }
        private static void Require(bool valid, string message) { if (!valid) throw new InvalidOperationException(message); }
    }
}
