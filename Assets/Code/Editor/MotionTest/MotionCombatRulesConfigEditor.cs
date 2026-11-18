using CodeBase.MotionTest;
using UnityEditor;
using UnityEngine;

namespace CodeBase.EditorTools
{
    [CustomEditor(typeof(MotionCombatRulesConfig))]
    public sealed class MotionCombatRulesConfigEditor : UnityEditor.Editor
    {
        [MenuItem("Tools/PetProject/Motion/Combat Rules Settings")]
        private static void Open()
        {
            var config = Selection.activeObject as MotionCombatRulesConfig;
            if (config == null)
                config = AssetDatabase.LoadAssetAtPath<MotionCombatRulesConfig>(
                    MotionTestSceneTools.DataRoot + "/MotionCombatRulesConfig.asset");
            Selection.activeObject = config;
            EditorGUIUtility.PingObject(config);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.HelpBox(
                "Общие правила столкновения и параметры героя. Архетипы противников и их AI настраиваются отдельно.",
                MessageType.Info);
            EditorGUILayout.LabelField("Очки действий и группы", EditorStyles.boldLabel);
            Draw("_actionsPerTurn", "ОД на ход");
            Draw("_attackCost", "Стоимость атаки, ОД");
            Draw("_groupLinkRadius", "Радиус связывания группы, units");
            Draw("_enemyDecisionDelay", "Пауза перед действием противника, с");

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Характеристики героя", EditorStyles.boldLabel);
            DrawCharacteristic("_heroStrength", "Сила");
            DrawCharacteristic("_heroDexterity", "Ловкость");
            DrawCharacteristic("_heroConstitution", "Телосложение");
            DrawCharacteristic("_heroIntelligence", "Интеллект");
            DrawCharacteristic("_heroWisdom", "Мудрость");
            DrawCharacteristic("_heroCharisma", "Харизма");
            Draw("_heroBaseMaxHealth", "Базовый максимум HP");
            Draw("_heroHealthPerConstitutionModifier", "HP за модификатор телосложения");
            Draw("_heroProficiencyBonus", "Бонус мастерства");
            Draw("_heroBaseDefense", "Базовая броня");
            Draw("_heroArmorBonus", "Бонус доспеха");
            Draw("_heroDamage", "Фиксированный урон");
            serializedObject.ApplyModifiedProperties();

            var config = (MotionCombatRulesConfig)target;
            EditorGUILayout.HelpBox(
                $"HP {config.HeroMaxHealth} · броня {config.HeroDefense} · атака +{config.HeroAttackBonus} · " +
                $"урон {config.HeroDamage}", MessageType.None);
        }

        private void Draw(string propertyName, string label) =>
            EditorGUILayout.PropertyField(serializedObject.FindProperty(propertyName), new GUIContent(label));

        private void DrawCharacteristic(string propertyName, string label)
        {
            var property = serializedObject.FindProperty(propertyName);
            EditorGUILayout.PropertyField(property, new GUIContent(label));
            EditorGUILayout.LabelField($"Модификатор: {MotionCombatRulesConfig.Modifier(property.intValue):+#;-#;0}",
                EditorStyles.miniLabel);
        }
    }
}
