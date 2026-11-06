using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace GoogleSheetConfig
{
    public abstract class SingleParser<T> : BaseParser
        where T : ScriptableObject
    {
        private const string ColParameterName = "Parameter";
        private const string ColValueName = "Value";

        private readonly Dictionary<string, JToken> _tokenByParameter = new();

        public override void Parse(JArray jArray)
        {
            _tokenByParameter.Clear();

            var settingAssetsT = AssetUtility.LoadAsset<T>();
            if (settingAssetsT == null)
            {
                settingAssetsT = OnCreateSettingsAsset();
                
                if (settingAssetsT != null)
                {
                    Debug.Log($"Create missed {settingAssetsT.GetType().Name} asset for json:{jArray}");
                }
                else
                {
                    Debug.LogError($"No settings asset (ScriptableObject) found for {GetType().Name}, json:{jArray}");
                }
            }

            if (settingAssetsT != null)
            {
                foreach (var jToken in jArray)
                {
                    var parameterName = jToken.Value<string>(ColParameterName);
                    _tokenByParameter.Add(parameterName, jToken);
                }

                OnParseSettings(settingAssetsT);

                EditorUtility.SetDirty(settingAssetsT);
            }
            
            Debug.Log($"Parse {GetType().Name} completed");
        }

        protected abstract T OnCreateSettingsAsset();
        protected abstract void OnParseSettings(T settings);

        protected int GetPositiveInt(string key)
        {
            var jTokenParameter = _tokenByParameter[key];
            return GetPositiveInt(jTokenParameter, ColValueName);
        }

        protected float GetPositiveFloat(string key)
        {
            var jTokenParameter = _tokenByParameter[key];
            return GetPositiveFloat(jTokenParameter, ColValueName);
        }

        protected float GetFloat(string key)
        {
            var jTokenParameter = _tokenByParameter[key];
            return GetFloat(jTokenParameter, ColValueName);
        }

        protected string GetString(string key)
        {
            var jTokenParameter = _tokenByParameter[key];
            return GetString(jTokenParameter, ColValueName);
        }

        protected TEnum GetEnumInt<TEnum>(string key) where TEnum : struct, Enum
        {
            var jTokenParameter = _tokenByParameter[key];
            return GetEnumInt<TEnum>(jTokenParameter, ColValueName);
        }
        
        protected TEnum GetEnumString<TEnum>(string key) where TEnum : struct, Enum
        {
            var jTokenParameter = _tokenByParameter[key];
            return GetEnumString<TEnum>(jTokenParameter, ColValueName);
        }

        protected JToken GetParameterToken(string parameterName)
        {
            _tokenByParameter.TryGetValue(parameterName, out var token);
            return token;
        }

        protected bool GetBool(string key)
        {
            if (!_tokenByParameter.TryGetValue(key, out var jTokenParameter))
            {
                Debug.LogError($"Parameter key '{key}' not found in table");
                return false;
            }
            return GetBool(jTokenParameter, ColValueName);
        }

        protected int GetLayerMask(string key)
        {
            if (!_tokenByParameter.TryGetValue(key, out var jTokenParameter))
            {
                Debug.LogError($"Parameter key '{key}' not found");
                return 0;
            }
            return ParseLayerMask(jTokenParameter, ColValueName);
        }

        protected Vector2 GetVector2(string key)
        {
            if (!_tokenByParameter.TryGetValue(key, out var token))
            {
                Debug.LogError($"Параметр '{key}' не найден");
                return Vector2.zero;
            }
            return GetVector2(token, ColValueName);
        }
    }
}