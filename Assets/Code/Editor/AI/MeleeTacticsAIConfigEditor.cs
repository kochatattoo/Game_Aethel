using CodeBase.Infrastructure.Services.AIServices.BehaviourTree.Configuration;
using UnityEditor;
using UnityEngine;

namespace CodeBase.EditorTools.AI
{
    [CustomEditor(typeof(MeleeTacticsAIConfig))]
    public sealed class MeleeTacticsAIConfigEditor : UnityEditor.Editor
    {
        [MenuItem("Tools/PetProject/AI/Melee Tactics Settings")]
        private static void Open()
        {
            var config = Selection.activeObject as MeleeTacticsAIConfig;
            if (config == null)
                config = AssetDatabase.LoadAssetAtPath<MeleeTacticsAIConfig>(
                    "Assets/Configs/AI/MeleeTacticsAIConfig.asset");
            Selection.activeObject = config;
            EditorGUIUtility.PingObject(config);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.HelpBox(
                "Только поведение AI. Характеристики, ранг и кубики настраиваются в EnemyArchetypeConfig.",
                MessageType.Info);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_attackPriority"),
                new GUIContent("Приоритет атаки"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_movePriority"),
                new GUIContent("Приоритет сближения"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_endTurnPriority"),
                new GUIContent("Приоритет завершения хода"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_maxMovementCostPerCommand"),
                new GUIContent("Максимум ОД на одно перемещение"));
            serializedObject.ApplyModifiedProperties();
        }
    }
}
