using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AssemblyBuilder;
using AssemblyBuilder.Model;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using Builder = AssemblyBuilder.AssemblyBuilder;

namespace CodeBase.EditorTools.Architecture
{
    public static class AssemblyBuilderGraphMigration
    {
        private const string Root = "Assets/Code/Scripts/Architecture";
        private const string BuildersRoot = Root + "/Assemblies";
        private const string ExternalRoot = Root + "/External";

        [Serializable]
        private sealed class DefinitionData
        {
            public string name;
            public string[] references = Array.Empty<string>();
        }

        [MenuItem("Tools/PetProject/Architecture/Create Assembly Builder Graph")]
        public static void CreateGraph()
        {
            Directory.CreateDirectory(BuildersRoot);
            Directory.CreateDirectory(ExternalRoot);
            var ownedPaths = AssetDatabase.FindAssets("t:AssemblyDefinitionAsset")
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(path => path.StartsWith("Assets/Code/", StringComparison.Ordinal))
                .OrderBy(path => path, StringComparer.Ordinal)
                .ToArray();
            var builders = new Dictionary<string, Builder>(StringComparer.Ordinal);
            foreach (var path in ownedPaths)
                builders.Add(path, CreateBuilder(path, false));

            foreach (var path in ownedPaths)
            {
                var definition = LoadDefinition(path);
                foreach (var reference in definition.references)
                {
                    var referencedPath = ResolveReference(reference);
                    if (referencedPath == null || builders.ContainsKey(referencedPath)) continue;
                    builders.Add(referencedPath, CreateBuilder(referencedPath, true));
                }
            }

            var packagePath = AssetDatabase.FindAssets("t:AssemblyDefinitionAsset")
                .Select(AssetDatabase.GUIDToAssetPath)
                .First(path => path.Contains("com.vertoker.assemblybuilder") &&
                    path.EndsWith("/AssemblyBuilder.asmdef", StringComparison.Ordinal));
            if (!builders.ContainsKey(packagePath)) builders.Add(packagePath, CreateBuilder(packagePath, true));

            foreach (var path in ownedPaths)
            {
                var definition = LoadDefinition(path);
                var builder = builders[path];
                var parents = new List<BaseAssemblyBuilder>();
                foreach (var reference in definition.references)
                {
                    var referencedPath = ResolveReference(reference);
                    if (referencedPath != null && builders.TryGetValue(referencedPath, out var parent))
                        parents.Add(parent);
                }
                if (definition.name == "PetProject.Editor") parents.Add(builders[packagePath]);
                SetParents(builder, parents);
                EditorUtility.SetDirty(builder);
            }

            var collectionPath = Root + "/AssemblyGraph.asset";
            var collection = AssetDatabase.LoadAssetAtPath<AssemblyBuilderCollection>(collectionPath);
            if (collection == null)
            {
                collection = ScriptableObject.CreateInstance<AssemblyBuilderCollection>();
                AssetDatabase.CreateAsset(collection, collectionPath);
            }
            var serializedCollection = new SerializedObject(collection);
            var items = serializedCollection.FindProperty("_builders");
            items.arraySize = builders.Count;
            var index = 0;
            foreach (var builder in builders.Values.OrderBy(item => item.name, StringComparer.Ordinal))
                items.GetArrayElementAtIndex(index++).objectReferenceValue = builder;
            serializedCollection.ApplyModifiedProperties();
            EditorUtility.SetDirty(collection);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"AssemblyBuilder graph created: {ownedPaths.Length} owned, " +
                $"{builders.Count - ownedPaths.Length} external builders");
        }

        [MenuItem("Tools/PetProject/Architecture/Configure Core Assemblies")]
        public static void ConfigureCoreAssemblies()
        {
            var characterStats = CreateBuilder(
                "Assets/Code/Modules/CharacterStats/Core/PetProject.CharacterStats.Core.asmdef", false);
            var enemyArchetypes = CreateBuilder(
                "Assets/Code/Modules/EnemyArchetypes/Core/PetProject.EnemyArchetypes.Core.asmdef", false);
            var navigation = CreateBuilder(
                "Assets/Code/Modules/Navigation/Core/PetProject.Navigation.Core.asmdef", false);
            var grid = CreateBuilder(
                "Assets/Code/Modules/Tactics/Runtime/Grid/PetProject.Tactics.Grid.Core.asmdef", false);
            var tactics = AssetDatabase.LoadAssetAtPath<Builder>(BuildersRoot + "/PetProject.Tactics.asset");
            var gameplay = AssetDatabase.LoadAssetAtPath<Builder>(BuildersRoot + "/PetProject.Gameplay.asset");
            var editor = AssetDatabase.LoadAssetAtPath<Builder>(BuildersRoot + "/PetProject.Editor.asset");
            var tacticsTests = AssetDatabase.LoadAssetAtPath<Builder>(BuildersRoot + "/PetProject.Tactics.Tests.asset");

            SetParents(enemyArchetypes, new BaseAssemblyBuilder[] { characterStats });
            SetParents(tactics, new BaseAssemblyBuilder[] { characterStats, grid });
            AppendParents(gameplay, characterStats, enemyArchetypes, navigation, grid);
            AppendParents(editor, grid);
            AppendParents(tacticsTests, grid);
            foreach (var builder in new[] { characterStats, enemyArchetypes, navigation, grid })
                IncludeInCollection(builder);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void AppendParents(Builder builder, params Builder[] parents)
        {
            var serialized = new SerializedObject(builder);
            var publicParents = serialized.FindProperty("_publicParents");
            foreach (var parent in parents)
            {
                var exists = false;
                for (var i = 0; i < publicParents.arraySize; ++i)
                    if (publicParents.GetArrayElementAtIndex(i).objectReferenceValue == parent) exists = true;
                if (exists) continue;
                var index = publicParents.arraySize;
                publicParents.arraySize = index + 1;
                publicParents.GetArrayElementAtIndex(index).objectReferenceValue = parent;
            }
            serialized.ApplyModifiedProperties();
            EditorUtility.SetDirty(builder);
        }

        private static void IncludeInCollection(Builder builder)
        {
            var collection = AssetDatabase.LoadAssetAtPath<AssemblyBuilderCollection>(Root + "/AssemblyGraph.asset");
            var serialized = new SerializedObject(collection);
            var items = serialized.FindProperty("_builders");
            for (var i = 0; i < items.arraySize; ++i)
                if (items.GetArrayElementAtIndex(i).objectReferenceValue == builder) return;
            var index = items.arraySize;
            items.arraySize = index + 1;
            items.GetArrayElementAtIndex(index).objectReferenceValue = builder;
            serialized.ApplyModifiedProperties();
            EditorUtility.SetDirty(collection);
        }

        private static Builder CreateBuilder(string definitionPath, bool isReadonly)
        {
            var definition = AssetDatabase.LoadAssetAtPath<AssemblyDefinitionAsset>(definitionPath);
            if (definition == null) throw new InvalidOperationException("AssemblyDefinition not found: " + definitionPath);
            var directory = isReadonly ? ExternalRoot : BuildersRoot;
            var path = directory + "/" + definition.name + ".asset";
            var builder = AssetDatabase.LoadAssetAtPath<Builder>(path);
            if (builder == null)
            {
                builder = ScriptableObject.CreateInstance<Builder>();
                builder.name = definition.name;
                AssetDatabase.CreateAsset(builder, path);
            }
            var serialized = new SerializedObject(builder);
            serialized.FindProperty("_readonly").boolValue = isReadonly;
            var mode = isReadonly ? AssemblyInheritMode.NoInherit : AssemblyInheritMode.Inherit;
            serialized.FindProperty("_inheritMode").enumValueIndex = Array.IndexOf(
                Enum.GetNames(typeof(AssemblyInheritMode)), mode.ToString());
            var definitions = serialized.FindProperty("_definitions");
            definitions.arraySize = 1;
            definitions.GetArrayElementAtIndex(0).objectReferenceValue = definition;
            serialized.ApplyModifiedProperties();
            EditorUtility.SetDirty(builder);
            return builder;
        }

        private static void SetParents(Builder builder, IReadOnlyList<BaseAssemblyBuilder> parents)
        {
            var serialized = new SerializedObject(builder);
            var publicParents = serialized.FindProperty("_publicParents");
            var privateParents = serialized.FindProperty("_privateParents");
            publicParents.arraySize = parents.Count;
            for (var i = 0; i < parents.Count; ++i)
                publicParents.GetArrayElementAtIndex(i).objectReferenceValue = parents[i];
            privateParents.arraySize = 0;
            serialized.ApplyModifiedProperties();
        }

        private static DefinitionData LoadDefinition(string path)
        {
            var asset = AssetDatabase.LoadAssetAtPath<AssemblyDefinitionAsset>(path);
            return JsonUtility.FromJson<DefinitionData>(asset.text);
        }

        private static string ResolveReference(string reference)
        {
            if (string.IsNullOrEmpty(reference)) return null;
            if (reference.StartsWith("GUID:", StringComparison.Ordinal))
            {
                var path = AssetDatabase.GUIDToAssetPath(reference.Substring(5));
                return string.IsNullOrEmpty(path) ? null : path;
            }
            var guid = AssetDatabase.FindAssets("t:AssemblyDefinitionAsset")
                .FirstOrDefault(candidate => string.Equals(
                    AssetDatabase.LoadAssetAtPath<AssemblyDefinitionAsset>(
                        AssetDatabase.GUIDToAssetPath(candidate))?.name,
                    reference, StringComparison.Ordinal));
            return guid == null ? null : AssetDatabase.GUIDToAssetPath(guid);
        }
    }
}
