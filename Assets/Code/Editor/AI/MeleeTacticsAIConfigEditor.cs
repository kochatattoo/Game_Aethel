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
        private SerializedProperty _actionsPerTurn;
        private SerializedProperty _attackCost;
        private SerializedProperty _maxHealth;
        private SerializedProperty _initiativeBonus;
        private SerializedProperty _attackBonus;
        private SerializedProperty _defense;
        private SerializedProperty _damage;

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

        private void OnEnable()
        {
            _attackPriority = serializedObject.FindProperty("_attackPriority");
            _movePriority = serializedObject.FindProperty("_movePriority");
            _endTurnPriority = serializedObject.FindProperty("_endTurnPriority");
            _maxMovementCostPerCommand = serializedObject.FindProperty("_maxMovementCostPerCommand");
            _actionsPerTurn = serializedObject.FindProperty("_actionsPerTurn");
            _attackCost = serializedObject.FindProperty("_attackCost");
            _maxHealth = serializedObject.FindProperty("_maxHealth");
            _initiativeBonus = serializedObject.FindProperty("_initiativeBonus");
            _attackBonus = serializedObject.FindProperty("_attackBonus");
            _defense = serializedObject.FindProperty("_defense");
            _damage = serializedObject.FindProperty("_damage");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.HelpBox(
                "Дерево выбирает первую доступную ветку: атака → движение → завершение хода. " +
                "Больший приоритет означает более раннюю проверку ветки.", MessageType.Info);

            EditorGUILayout.LabelField("Поведение", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_attackPriority, new GUIContent("Приоритет атаки"));
            EditorGUILayout.PropertyField(_movePriority, new GUIContent("Приоритет сближения"));
            EditorGUILayout.PropertyField(_endTurnPriority, new GUIContent("Приоритет завершения хода"));
            EditorGUILayout.PropertyField(_maxMovementCostPerCommand,
                new GUIContent("Максимум ОД на одно перемещение"));

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Очки действий", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_actionsPerTurn, new GUIContent("ОД на ход"));
            EditorGUILayout.PropertyField(_attackCost, new GUIContent("Стоимость атаки, ОД"));
            if (_attackCost.intValue > _actionsPerTurn.intValue)
                EditorGUILayout.HelpBox("Стоимость атаки не может превышать ОД на ход.", MessageType.Error);
            if (_maxMovementCostPerCommand.intValue < 1)
                EditorGUILayout.HelpBox("Минимальная стоимость команды движения — 1 ОД.", MessageType.Warning);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Боевые параметры скелета", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_maxHealth, new GUIContent("Максимум HP"));
            EditorGUILayout.PropertyField(_initiativeBonus, new GUIContent("Бонус инициативы"));
            EditorGUILayout.PropertyField(_attackBonus, new GUIContent("Бонус атаки"));
            EditorGUILayout.PropertyField(_defense, new GUIContent("Защита"));
            EditorGUILayout.PropertyField(_damage, new GUIContent("Урон"));

            serializedObject.ApplyModifiedProperties();
            var config = (MeleeTacticsAIConfig)target;
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox(
                $"За полный ход доступно атак: {config.ActionsPerTurn / config.AttackCost}. " +
                $"Атака тратит {config.AttackCost} ОД; остаток можно потратить на перемещение.",
                MessageType.None);
        }
    }
}
