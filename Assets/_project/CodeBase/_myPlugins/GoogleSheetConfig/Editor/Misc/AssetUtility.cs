using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace GoogleSheetConfig
{
    public class AssetUtility
    {
        public static T LoadAsset<T>() where T : ScriptableObject
        {
            return LoadAssets<T>().FirstOrDefault();
        }

        public static T LoadAsset<T>(string assetName) where T : ScriptableObject
        {
            var guids = AssetDatabase.FindAssets($"t:{typeof(T).Name} {assetName}");

            if (guids.Length == 0) 
                return null;

            return AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guids[0]));
        }
        public static T LoadAssetByPath<T>(string assetPath) where T : ScriptableObject
        {
            return AssetDatabase.LoadAssetAtPath<T>(assetPath);
        }

        public static List<T> LoadAssets<T>() where T : ScriptableObject
        {
            var guids = AssetDatabase.FindAssets("t:" + typeof(T).Name);
            if (guids.Length == 0)
            {
                Debug.LogError("No assets (ScriptableObject) found for " + typeof(T).Name);
                return null;
            }

            var listAssets = new List<T>();
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                listAssets.Add(AssetDatabase.LoadAssetAtPath<T>(path));
            }

            return listAssets;
        }

        public static T LoadOrCreateAsset<T>(string assetPath) where T : ScriptableObject
        {
            EnsureDirectoryExists(assetPath);

            var existing = AssetDatabase.LoadAssetAtPath<T>(assetPath);
            if (existing != null) 
                return existing;

            var asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, assetPath);
            AssetDatabase.SaveAssets();
            return asset;
        }

        public static T CreateScriptableObject<T>(string path) where T : ScriptableObject
        {
            EnsureDirectoryExists(path);
            
            var asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }
        
        public static T CreateScriptableObject<T>(string className, string path) where T : ScriptableObject
        {
            EnsureDirectoryExists(path);
            
            var asset = ScriptableObject.CreateInstance(className);
            AssetDatabase.CreateAsset(asset, path);
            return asset as T;
        }
        
        private static void EnsureDirectoryExists(string assetPath)
        {
            var directory = Path.GetDirectoryName(assetPath);

            if (string.IsNullOrEmpty(directory))
                return;

            if (!AssetDatabase.IsValidFolder(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }
    }
}