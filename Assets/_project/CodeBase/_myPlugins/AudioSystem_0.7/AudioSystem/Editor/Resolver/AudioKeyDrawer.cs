using Infrastructure.AudioSystem.WwiseSystem.WwiseKeys;
using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Infrastructure.AudioSystem.Utils;

namespace Audio.Editor.Resolver
{
    [CustomPropertyDrawer(typeof(AudioKeyAttribute))]
    public class AudioKeyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (!IsAttributeTypeConsistent(property, out AudioKeyType consistentType))
            {
                EditorGUI.LabelField(position, label.text, "Multiple Key Types Selected");
                return;
            }

            Type targetType = GetTargetType(consistentType);
            var keys = targetType
                .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                .Where(f => f.IsLiteral)
                .Select(f => f.GetRawConstantValue().ToString())
                .ToArray();

            if (keys.Length == 0)
            {
                EditorGUI.PropertyField(position, property, label);
                return;
            }

            EditorGUI.BeginProperty(position, label, property);
            EditorGUI.showMixedValue = property.hasMultipleDifferentValues;

            int currentIndex = Array.IndexOf(keys, property.stringValue);

            EditorGUI.BeginChangeCheck();
            int newIndex = EditorGUI.Popup(position, label.text, currentIndex < 0 ? 0 : currentIndex, keys);

            if (EditorGUI.EndChangeCheck())
            {
                property.stringValue = keys[newIndex];
            }

            EditorGUI.showMixedValue = false;
            EditorGUI.EndProperty();
        }

        private bool IsAttributeTypeConsistent(SerializedProperty property, out AudioKeyType type)
        {
            type = ((AudioKeyAttribute)attribute).Type;

            foreach (var targetObject in property.serializedObject.targetObjects)
            {
                var field = targetObject.GetType()
                    .GetField(property.name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (field != null)
                {
                    var attr = (AudioKeyAttribute)Attribute.GetCustomAttribute(field, typeof(AudioKeyAttribute));
                    if (attr != null && attr.Type != type) 
                        return false;
                }
            }
            return true;
        }

        private Type GetTargetType(AudioKeyType type) => type switch
        {
            AudioKeyType.Event => typeof(WwiseEventsKeys),
            AudioKeyType.Parameter => typeof(WwiseParameterKeys),
            AudioKeyType.Switch => typeof(WwiseSwitchKeys),
            AudioKeyType.State => typeof(WwiseStateKeys),
            AudioKeyType.AuxBus => typeof(WwiseAuxBusKeys),
            _ => typeof(WwiseEventsKeys)
        };
    }
}

