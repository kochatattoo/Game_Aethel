using CodeBase.MotionTest;
using UnityEditor;
using UnityEngine;

namespace CodeBase.EditorTools
{
    [CustomEditor(typeof(EnemyArchetypeConfig))]
    public sealed class EnemyArchetypeConfigEditor : UnityEditor.Editor
    {
        [MenuItem("Tools/PetProject/Enemies/Skeleton Archetype Settings")]
        private static void Open()
        {
            var config = AssetDatabase.LoadAssetAtPath<EnemyArchetypeConfig>(
                "Assets/Configs/Enemies/Archetypes/Skeleton.asset");
            Selection.activeObject = config;
            EditorGUIUtility.PingObject(config);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.HelpBox(
                "Архетип конкретного противника. Каждый спавн может ссылаться на свой актив. " +
                "AI вынесен в MeleeTacticsAIConfig, общие правила — в MotionCombatRulesConfig.", MessageType.Info);
            EditorGUILayout.LabelField("Идентификация и поведение", EditorStyles.boldLabel);
            Draw("_archetype", "Архетип");
            Draw("_rank", "Ранг");
            Draw("_displayName", "Отображаемое имя");
            Draw("_prefab", "Prefab");
            Draw("_ai", "AI поведения");

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Характеристики D&D", EditorStyles.boldLabel);
            DrawCharacteristic("_strength", "Сила");
            DrawCharacteristic("_dexterity", "Ловкость");
            DrawCharacteristic("_constitution", "Телосложение");
            DrawCharacteristic("_intelligence", "Интеллект");
            DrawCharacteristic("_wisdom", "Мудрость");
            DrawCharacteristic("_charisma", "Харизма");
            Draw("_attackAttribute", "Характеристика атаки");

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Производные параметры", EditorStyles.boldLabel);
            Draw("_baseMaxHealth", "Базовый максимум HP");
            Draw("_healthPerConstitutionModifier", "HP за модификатор телосложения");
            Draw("_proficiencyBonus", "Бонус мастерства");
            Draw("_baseDefense", "Базовая броня");
            Draw("_armorBonus", "Бонус доспеха");

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Кубики", EditorStyles.boldLabel);
            Draw("_attackDiceSides", "Граней кубика атаки");
            Draw("_damageDiceCount", "Количество кубиков урона");
            Draw("_damageDiceSides", "Граней кубика урона");
            Draw("_damageBonus", "Бонус урона");
            serializedObject.ApplyModifiedProperties();

            var config = (EnemyArchetypeConfig)target;
            EditorGUILayout.HelpBox(
                $"HP {config.MaxHealth} · инициатива {Signed(config.InitiativeBonus)} · " +
                $"атака {Signed(config.AttackBonus)} · броня {config.Defense}\n" +
                $"Атака: 1d{config.AttackDiceSides} {Signed(config.AttackBonus)}; " +
                $"урон: {config.DamageDiceCount}d{config.DamageDiceSides} {Signed(config.DamageBonus)}.",
                MessageType.None);
        }

        private void Draw(string propertyName, string label) =>
            EditorGUILayout.PropertyField(serializedObject.FindProperty(propertyName), new GUIContent(label));

        private void DrawCharacteristic(string propertyName, string label)
        {
            var property = serializedObject.FindProperty(propertyName);
            EditorGUILayout.PropertyField(property, new GUIContent(label));
            EditorGUILayout.LabelField($"Модификатор: {Signed(EnemyArchetypeConfig.Modifier(property.intValue))}",
                EditorStyles.miniLabel);
        }

        private static string Signed(int value) => value >= 0 ? "+" + value : value.ToString();
    }
}
