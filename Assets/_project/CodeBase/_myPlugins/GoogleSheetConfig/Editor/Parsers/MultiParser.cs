using System;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace GoogleSheetConfig
{
    public abstract class MultiParser<T> : BaseParser
        where T : ScriptableObject
    {
        public override void Parse(JArray jArray)
        {
            var settingAssetsT = AssetUtility.LoadAssets<T>();

            foreach (var jToken in jArray)
            {
                var settingsT = settingAssetsT.FirstOrDefault(GetFindSettingsPredicate(jToken));
                if (settingsT == null)
                {
                    settingsT = OnCreateSettingsAsset(jToken);
                    
                    if (settingsT != null)
                    {
                        settingAssetsT.Add(settingsT);
                        Debug.Log($"Create missed {settingsT.GetType().Name} asset for json:{jToken}");
                    }
                    else
                    {
                        Debug.LogError($"No settings asset (ScriptableObject) found for {GetType().Name}, json:{jToken}");
                        continue;
                    }
                }
                
                OnParseToken(jToken, settingsT);
            }

            foreach (var settingT in settingAssetsT)
            {
                EditorUtility.SetDirty(settingT);
            }
            
            Debug.Log($"Parse {GetType().Name} completed");
        }

        protected abstract Func<T, bool> GetFindSettingsPredicate(JToken jToken);

        protected abstract T OnCreateSettingsAsset(JToken jToken);
        
        protected abstract void OnParseToken(JToken jToken, T settings);
    }
}