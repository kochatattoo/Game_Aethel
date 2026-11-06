using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using UnityEngine;

namespace GoogleSheetConfig
{
    public abstract class BaseParser : ISettingsParser
    {
        public abstract string SheetName { get; }

        public abstract void Parse(JArray jArray);

        protected int GetPositiveInt(JToken jToken, string key)
        {
            var value = jToken.Value<int>(key);
            AssertFalse(value < 0, $"Not a positive integer key:{key}, value:{value}");
            return value;
        }

        protected int GetInt(JToken jToken, string key, int defaultValue = 0)
        {
            var token = jToken[key];
            if (token == null || token.Type == JTokenType.Null)
                return defaultValue;

            try 
            {
                return token.Value<int>(); 
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to parse int for key '{key}': {e.Message}");
                return defaultValue;
            }
        }

        protected float GetPositiveFloat(JToken jToken, string key)
        {
            var value = jToken.Value<float>(key);
            AssertFalse(value < 0, $"Not a positive float key:{key}, value:{value}");
            return value;
        }

        protected float GetFloat(JToken jToken, string key)
        {
            var value = jToken.Value<float>(key);
            return value;
        }

        protected string GetString(JToken jToken, string key)
        {
            var token = jToken[key];

            if (token == null || token.Type == JTokenType.Null)
                return null;

            if (token is JArray array)
            {
                Debug.LogWarning($"Field '{key}' is an array. Joining with space.");
                return string.Join(" ", array.Select(t => t?.Value<string>() ?? ""));
            }

            return token.Value<string>();
        }

        protected T GetEnumInt<T>(JToken jToken, string key) where T : struct, Enum
        {
            var enumInt = GetPositiveInt(jToken, key);
            var enumString = enumInt.ToString();
            var result = Enum.TryParse<T>(enumString, out var enumValue);
            if (!result)
            {
                Debug.LogError($"Failed to parse enum {enumString}, type:{typeof(T).Name}, json:{jToken}");
            }

            return enumValue;
        }

        protected T GetEnumString<T>(JToken jToken, string key) where T : struct, Enum
        {
            var enumString = GetString(jToken, key);
            var result = Enum.TryParse<T>(enumString, out var enumValue);
            if (!result)
            {
                Debug.LogError($"Failed to parse enum {enumString}, type:{typeof(T).Name}, json:{jToken}");
            }

            return enumValue;
        }

        protected TEnum[] GetEnumArray<TEnum>(JToken jToken, string key, char separator = ',')
            where TEnum : struct, Enum
        {
            var token = jToken[key];
            if (token == null || token.Type == JTokenType.Null) 
                return Array.Empty<TEnum>();

            if (token is JArray jArray)
            {
                return jArray.Select(t => GetEnumFromToken<TEnum>(t)).ToArray();
            }

            if (token.Type == JTokenType.String)
            {
                var str = token.Value<string>();
                if (string.IsNullOrWhiteSpace(str)) 
                    return Array.Empty<TEnum>();

                return str.Split(new[] { separator }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => GetEnumFromString<TEnum>(s.Trim()))
                    .ToArray();
            }

            Debug.LogError($"GetEnumArray: unexpected token type {token.Type} for key '{key}'");
            return Array.Empty<TEnum>();
        }

        protected List<T> GetList<T>(JToken jToken, string key, Func<JToken, T> converter, char separator = ',')
        {
            var token = jToken[key];
            if (token == null || token.Type == JTokenType.Null)
                return new List<T>();

            if (token is JArray jArray)
                return jArray.Select(converter).ToList();

            if (token.Type == JTokenType.String)
            {
                var str = token.Value<string>();
                if (string.IsNullOrWhiteSpace(str))
                    return new List<T>();

                return str.Split(new[] { separator }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => converter(new JValue(s.Trim())))
                    .ToList();
            }

            Debug.LogError($"GetList: unexpected token type {token.Type}");
            return new List<T>();
        }

        protected T[] GetArray<T>(JToken jToken, string key, Func<JToken, T> converter, char separator = ',')
        {
            var token = jToken[key];
            if (token == null || token.Type == JTokenType.Null) 
                return Array.Empty<T>();

            if (token is JArray jArray)
                return jArray.Select(converter).ToArray();

            if (token.Type == JTokenType.String)
            {
                var str = token.Value<string>();
                if (string.IsNullOrWhiteSpace(str)) 
                    return Array.Empty<T>();

                return str.Split(new[] { separator }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => converter(new JValue(s.Trim())))
                    .ToArray();
            }

            Debug.LogError($"GetArray: unexpected token type {token.Type}");
            return Array.Empty<T>();
        }

        /// <summary>
        /// Парсит список enum-ов из JToken. Поддерживает JArray и строку с разделителем.
        /// </summary>
        protected List<TEnum> GetEnumList<TEnum>(JToken jToken, string key, char separator = ',')
            where TEnum : struct, Enum
        {
            var token = jToken[key];
            if (token == null || token.Type == JTokenType.Null)
                return new List<TEnum>();

            var result = new List<TEnum>();

            if (token is JArray jArray)
            {
                foreach (var t in jArray)
                {
                    if (TryParseEnumToken<TEnum>(t, out var enumVal))
                        result.Add(enumVal);
                }
                return result;
            }

            if (token.Type == JTokenType.String)
            {
                var str = token.Value<string>();
                if (string.IsNullOrWhiteSpace(str))
                    return result;

                var parts = str.Split(new[] { separator }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var part in parts)
                {
                    if (TryParseEnumString<TEnum>(part.Trim(), out var enumVal))
                        result.Add(enumVal);
                }
                return result;
            }

            Debug.LogError($"GetEnumList: unexpected token type {token.Type} for key '{key}'");
            return result;
        }

        protected bool GetBool(JToken jToken, string key)
        {
            var token = jToken[key];
            if (token == null)
            {
                Debug.LogError($"Key '{key}' not found in json: {jToken}");
                return false;
            }

            if (token.Type == JTokenType.Boolean)
                return token.Value<bool>();

            if (token.Type == JTokenType.Integer)
            {
                int intVal = token.Value<int>();
                AssertFalse(intVal != 0 && intVal != 1, $"Boolean field '{key}' must be 0 or 1, got {intVal}");
                return intVal == 1;
            }

            if (token.Type == JTokenType.String)
            {
                string strVal = token.Value<string>().Trim().ToLower();
                if (strVal == "true" || strVal == "1") 
                    return true;
                if (strVal == "false" || strVal == "0")
                    return false;
                Debug.LogError($"Cannot parse boolean from string '{strVal}' for key '{key}'");
                return false;
            }

            Debug.LogError($"Unsupported JToken type {token.Type} for boolean key '{key}'");
            return false;
        }

        protected bool GetOptionalBool(JToken jToken, string key, bool defaultValue = false)
        {
            var token = jToken[key];

            if (token == null) 
                return defaultValue;

            return GetBool(jToken, key);
        }

        /// <summary>
        /// Парсит строку с названиями слоёв (через запятую) и возвращает LayerMask как int.
        /// </summary>
        protected int ParseLayerMask(JToken jToken, string key)
        {
            var token = jToken[key];
            if (token == null || token.Type == JTokenType.Null)
                return 0;

            List<string> layerNames = new List<string>();

            if (token.Type == JTokenType.String)
            {
                string raw = token.Value<string>();
                layerNames = raw.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                               .Select(s => s.Trim())
                               .Where(s => !string.IsNullOrEmpty(s))
                               .ToList();
            }
            else if (token is JArray jArray)
            {
                foreach (var t in jArray)
                {
                    if (t.Type == JTokenType.String)
                        layerNames.Add(t.Value<string>().Trim());
                }
            }
            else
            {
                Debug.LogError($"[ParseLayerMask] Unexpected token type '{token.Type}' for key '{key}'");
                return 0;
            }

            int mask = 0;
            foreach (string name in layerNames)
            {
                int layerIndex = LayerMask.NameToLayer(name);
                if (layerIndex < 0)
                {
                    Debug.LogWarning($"[ParseLayerMask] Layer '{name}' not found in project. Ignored.");
                    continue;
                }
                mask |= 1 << layerIndex;
            }
            return mask;
        }

        protected void SetPropertyValue<T>(object target, string propertyName, T value)
        {
            PropertyInfo prop = null;
            Type currentType = target.GetType();

            while (currentType != null)
            {
                prop = currentType.GetProperty(propertyName,
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.IgnoreCase);

                if (prop != null) 
                    break;
                currentType = currentType.BaseType;
            }

            if (prop == null || !prop.CanWrite)
            {
                Debug.LogError($"[BaseParser] Property '{propertyName}' not found or read-only in {target.GetType().Name} (or base types).");
                return;
            }

            try
            {
                object finalValue;

                if (prop.PropertyType == typeof(LayerMask))
                {
                    if (value is int intVal)
                        finalValue = (LayerMask)intVal;
                    else if (value is string strVal && int.TryParse(strVal, out int parsed))
                        finalValue = (LayerMask)parsed;
                    else
                    {
                        Debug.LogError($"Cannot convert {value?.GetType().Name} to LayerMask");
                        return;
                    }
                }
                else if (prop.PropertyType == typeof(bool))
                {
                    if (value is bool b)
                        finalValue = b;
                    else if (value is string strBool)
                    {
                        strBool = strBool.Trim().ToLower();
                        finalValue = strBool == "true" || strBool == "1";
                    }
                    else if (value is IConvertible conv)
                    {
                        try
                        {
                            finalValue = Convert.ToBoolean(conv);
                        }
                        catch
                        {
                            finalValue = System.Convert.ToInt64(conv) != 0;
                        }
                    }
                    else
                    {
                        finalValue = System.Convert.ToBoolean(value);
                    }
                }
                else if (prop.PropertyType.IsEnum)
                {
                    string strValue = value.ToString();
                    foreach (var field in prop.PropertyType.GetFields(BindingFlags.Public | BindingFlags.Static))
                    {
                        var enumMember = field.GetCustomAttribute<EnumMemberAttribute>();
                        if (enumMember != null && string.Equals(enumMember.Value, strValue, StringComparison.OrdinalIgnoreCase))
                        {
                            finalValue = field.GetValue(null);
                            prop.SetValue(target, finalValue);
                            return;
                        }
                    }
                    finalValue = Enum.Parse(prop.PropertyType, strValue, ignoreCase: true);
                }
                else if (value is IConvertible && prop.PropertyType != typeof(T))
                {
                    finalValue = Convert.ChangeType(value, prop.PropertyType);
                }
                else
                {
                    finalValue = value;
                }

                prop.SetValue(target, finalValue);
            }
            catch (Exception e)
            {
                Debug.LogError($"[BaseParser] Failed to set '{propertyName}' on {target.GetType().Name}. Value: {value}. Error: {e.Message}");
            }
        }

        /// <summary>
        /// Парсит Vector2 из строки "x,y" или JArray [x, y].
        /// </summary>
        protected Vector2 GetVector2(JToken jToken, string key, char separator = ',')
        {
            var token = jToken[key];
            if (token == null || token.Type == JTokenType.Null)
                return Vector2.zero;

            if (token is JArray jArray)
            {
                if (jArray.Count >= 2)
                {
                    float x = jArray[0].Value<float>();
                    float y = jArray[1].Value<float>();
                    return new Vector2(x, y);
                }
                Debug.LogError($"GetVector2: JArray имеет {jArray.Count} элементов, ожидалось 2. Key: {key}");
                return Vector2.zero;
            }

            if (token.Type == JTokenType.String)
            {
                string raw = token.Value<string>();
                string[] parts = raw.Split(new[] { separator }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 2)
                {
                    Debug.LogError($"GetVector2: не удалось разобрать '{raw}' как Vector2. Ожидался формат 'x,y'. Key: {key}");
                    return Vector2.zero;
                }

                if (!float.TryParse(parts[0].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out float xVal) ||
                    !float.TryParse(parts[1].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out float yVal))
                {
                    Debug.LogError($"GetVector2: некорректные числа в '{raw}' для ключа '{key}'");
                    return Vector2.zero;
                }
                return new Vector2(xVal, yVal);
            }

            Debug.LogError($"GetVector2: неожиданный тип токена {token.Type} для ключа '{key}'");
            return Vector2.zero;
        }

        /// <summary>
        /// Устанавливает значение списка в свойство (включая унаследованные).
        /// </summary>
        protected void SetPropertyListValue<TItem>(object target, string propertyName, List<TItem> list)
        {
            PropertyInfo prop = null;
            Type currentType = target.GetType();

            while (currentType != null)
            {
                prop = currentType.GetProperty(propertyName,
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.IgnoreCase);
                if (prop != null) 
                    break;
                currentType = currentType.BaseType;
            }

            if (prop == null || !prop.CanWrite)
            {
                Debug.LogError($"[SetPropertyListValue] Property '{propertyName}' not found or read-only in {target.GetType().Name}");
                return;
            }

            if (!prop.PropertyType.IsGenericType ||
                prop.PropertyType.GetGenericTypeDefinition() != typeof(List<>))
            {
                Debug.LogError($"[SetPropertyListValue] Property '{propertyName}' is not a List<T>");
                return;
            }

            Type itemType = prop.PropertyType.GetGenericArguments()[0];

            IList finalList;
            if (itemType == typeof(TItem))
            {
                finalList = list;
            }
            else
            {
                Type listType = typeof(List<>).MakeGenericType(itemType);
                finalList = (IList)Activator.CreateInstance(listType);
                foreach (var item in list)
                {
                    if (item is IConvertible)
                        finalList.Add(Convert.ChangeType(item, itemType));
                    else
                        finalList.Add(item);
                }
            }

            try
            {
                prop.SetValue(target, finalList);
            }
            catch (Exception e)
            {
                Debug.LogError($"[SetPropertyListValue] Failed to set '{propertyName}': {e.Message}");
            }
        }

        protected void SetPropertyArrayValue<TItem>(object target, string propertyName, TItem[] array)
        {
            PropertyInfo prop = null;
            Type currentType = target.GetType();

            while (currentType != null)
            {
                prop = currentType.GetProperty(propertyName,
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.IgnoreCase);

                if (prop != null) 
                    break;
                currentType = currentType.BaseType;
            }

            if (prop == null || !prop.CanWrite)
            {
                Debug.LogError($"[SetPropertyArrayValue] Property '{propertyName}' not found or read-only in {target.GetType().Name}");
                return;
            }

            if (!prop.PropertyType.IsArray)
            {
                Debug.LogError($"[SetPropertyArrayValue] Property '{propertyName}' is not an array");
                return;
            }

            Type elementType = prop.PropertyType.GetElementType();

            if (elementType == typeof(TItem))
            {
                prop.SetValue(target, array);
            }
            else
            {
                Array convertedArray = Array.CreateInstance(elementType, array.Length);
                for (int i = 0; i < array.Length; i++)
                {
                    if (array[i] is IConvertible)
                        convertedArray.SetValue(Convert.ChangeType(array[i], elementType), i);
                    else
                        convertedArray.SetValue(array[i], i);
                }
                prop.SetValue(target, convertedArray);
            }
        }

        protected void SetNestedPropertyValue(object target, string propertyPath, object value)
        {
            if (string.IsNullOrEmpty(propertyPath)) 
                return;

            string[] parts = propertyPath.Split('.');
            object currentObj = target;
            Type currentType = currentObj.GetType();

            for (int i = 0; i < parts.Length - 1; i++)
            {
                string propName = parts[i];
                PropertyInfo prop = FindProperty(currentType, propName);
                if (prop == null)
                {
                    Debug.LogError($"[SetNestedPropertyValue] Property '{propName}' not found on {currentType.Name}");
                    return;
                }

                object nestedObj = prop.GetValue(currentObj);
                if (nestedObj == null)
                {
                    if (prop.PropertyType.IsSubclassOf(typeof(ScriptableObject)))
                    {
                        nestedObj = ScriptableObject.CreateInstance(prop.PropertyType);
                        prop.SetValue(currentObj, nestedObj);
                    }
                    else if (prop.PropertyType.IsClass)
                    {
                        nestedObj = Activator.CreateInstance(prop.PropertyType);
                        prop.SetValue(currentObj, nestedObj);
                    }
                    else
                    {
                        Debug.LogError($"[SetNestedPropertyValue] Property '{propName}' is null and cannot be auto-created (value type)");
                        return;
                    }
                }
                currentObj = nestedObj;
                currentType = currentObj.GetType();
            }

            string finalPropName = parts[parts.Length - 1];
            SetPropertyValue(currentObj, finalPropName, value);
        }
        protected void SetFieldValue(object target, string fieldName, object value)
        {
            Type type = target.GetType();
            FieldInfo field = null;
            while (type != null)
            {
                field = type.GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.IgnoreCase);
                if (field != null) 
                    break;
                type = type.BaseType;
            }

            if (field == null)
            {
                Debug.LogError($"[SetFieldValue] Поле '{fieldName}' не найдено в {target.GetType().Name}");
                return;
            }

            try
            {
                object finalValue = value;

                if (field.FieldType == typeof(LayerMask) && value is int intVal)
                    finalValue = (LayerMask)intVal;
                else if (field.FieldType == typeof(bool) && value is string strBool)
                    finalValue = bool.Parse(strBool);

                field.SetValue(target, finalValue);
            }
            catch (Exception e)
            {
                Debug.LogError($"[SetFieldValue] Ошибка установки поля '{fieldName}': {e.Message}");
            }
        }

        private void AssertFalse(bool condition, string message)
        {
            if (!condition)
                return;
            Debug.LogError(message);
        }

        private PropertyInfo FindProperty(Type type, string propertyName)
        {
            Type currentType = type;
            while (currentType != null)
            {
                var prop = currentType.GetProperty(propertyName,
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.IgnoreCase);
                if (prop != null) 
                    return prop;

                currentType = currentType.BaseType;
            }
            return null;
        }

        private TEnum GetEnumFromToken<TEnum>(JToken token) where TEnum : struct, Enum
        {
            if (token.Type == JTokenType.String)
                return GetEnumFromString<TEnum>(token.Value<string>());
            if (token.Type == JTokenType.Integer)
            {
                int val = token.Value<int>();
                if (Enum.IsDefined(typeof(TEnum), val))
                    return (TEnum)(object)val;
                Debug.LogError($"Integer {val} is not defined in {typeof(TEnum).Name}");
                return default;
            }
            Debug.LogError($"Cannot parse enum from token type {token.Type}: {token}");
            return default;
        }

        private T GetEnumFromString<T>(string stringValue, bool ignoreCase = true) where T : struct, Enum
        {
            foreach (var field in typeof(T).GetFields(BindingFlags.Public | BindingFlags.Static))
            {
                var enumMember = field.GetCustomAttribute<EnumMemberAttribute>();
                if (enumMember != null && string.Equals(enumMember.Value, stringValue,
                    ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal))
                {
                    return (T)field.GetValue(null);
                }
            }

            if (Enum.TryParse<T>(stringValue, ignoreCase, out var result))
                return result;

            Debug.LogError($"Failed to parse enum '{stringValue}' for type {typeof(T).Name}. Valid values: {string.Join(", ", Enum.GetNames(typeof(T)))}");
            return default;
        }

        private bool TryParseEnumString<TEnum>(string s, out TEnum value) where TEnum : struct, Enum
        {
            value = default;
            if (Enum.TryParse<TEnum>(s, true, out value))
                return true;
            Debug.LogError($"GetEnumList: failed to parse enum string '{s}' for {typeof(TEnum).Name}");
            return false;
        }

        private bool TryParseEnumToken<TEnum>(JToken token, out TEnum value) where TEnum : struct, Enum
        {
            value = default;
            if (token.Type == JTokenType.String)
                return TryParseEnumString<TEnum>(token.Value<string>(), out value);
            if (token.Type == JTokenType.Integer)
            {
                int val = token.Value<int>();
                if (Enum.IsDefined(typeof(TEnum), val))
                {
                    value = (TEnum)(object)val;
                    return true;
                }
                Debug.LogError($"GetEnumList: integer {val} not defined in {typeof(TEnum).Name}");
                return false;
            }
            Debug.LogError($"GetEnumList: cannot parse enum from token type {token.Type}");
            return false;
        }
    }
}