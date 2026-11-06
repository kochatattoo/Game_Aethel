using CodeBase.Infrastructure.Services.AIServices.BehaviourTree.Configuration;
using UnityEditor;
using UnityEngine;

namespace CodeBase.EditorTools.AI
{
    [CustomEditor(typeof(MeleeTacticsAIConfig))]
    public sealed class MeleeTacticsAIConfigEditor : UnityEditor.Editor
    {
        private SerializedProperty _attackPriority;
        private SerializedProperty _movePriority;
        private SerializedProperty _endTurnPriority;
        private SerializedProperty _maxMovementCostPerCommand;

        private void OnEnable()
        {
            _attackPriority = serializedObject.FindProperty("_attackPriority");
            _movePriority = serializedObject.FindProperty("_movePriority");
            _endTurnPriority = serializedObject.FindProperty("_endTurnPriority");
            _maxMovementCostPerCommand = serializedObject.FindProperty("_maxMovementCostPerCommand");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.HelpBox(
                "Дерево выбирает первую доступную ветку: атака → движение → завершение хода.", MessageType.Info);
            EditorGUILayout.LabelField("Приоритеты веток", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_attackPriority, new GUIContent("Атака"));
            EditorGUILayout.PropertyField(_movePriority, new GUIContent("Движение"));
            EditorGUILayout.PropertyField(_endTurnPriority, new GUIContent("Завершение хода"));
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(_maxMovementCostPerCommand,
                new GUIContent("Максимум ОД на команду движения"));
            if (_maxMovementCostPerCommand.intValue < 1)
                EditorGUILayout.HelpBox("Минимальная стоимость команды движения — 1 ОД.", MessageType.Warning);
            serializedObject.ApplyModifiedProperties();
        }
    }
}
