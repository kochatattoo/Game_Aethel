using System.IO;
using CodeBase.CharacterIntegration;
using CodeBase.Composition;
using CodeBase.Infrastructure.Services.SaveLoad;
using UnityEditor;
using UnityEngine;

namespace CodeBase.EditorTools
{
    [InitializeOnLoad]
    internal static class EditorApplicationEnvironment
    {
        static EditorApplicationEnvironment()
        {
            SaveEnvironment.Current = new EditorSaveEnvironment();
            ApplicationHost.Current = new EditorHost();
        }

        private sealed class EditorHost : IApplicationHost
        {
            public void Quit() => EditorApplication.isPlaying = false;
        }

        private sealed class EditorSaveEnvironment : ISaveEnvironment
        {
            private static bool IsSmoke => SessionState.GetBool("PetProject.CharacterMenu.Smoke", false);
            public string StorageRoot => IsSmoke
                ? Path.GetFullPath(Path.Combine("Library/SaveStorageMigration/Smoke", SessionState.GetString("PetProject.SaveStorage.Session", "default")))
                : Path.Combine(Application.persistentDataPath, "Saves");
            public string LegacyKey => IsSmoke ? "PetProject.CharacterMigration.Smoke" : "Progress";
        }
    }
}
