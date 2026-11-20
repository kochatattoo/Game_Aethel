using CodeBase.Enemies;
using CodeBase.Hero;
using CodeBase.Infrastructure.Services;
using CodeBase.Infrastructure.Utils;
using CodeBase.UI.Encounter;
using PetProject.Encounter;
using PetProject.Navigation;
using UnityEditor;
using UnityEngine;

namespace CodeBase.EditorTools
{
    [CustomEditor(typeof(NavigationConfig))]
    public sealed class NavigationConfigEditor : UnityEditor.Editor
    {
        [MenuItem("Tools/PetProject/Motion/Field Settings")]
        private static void Open()
        {
            var config = Selection.activeObject as NavigationConfig;
            if (config == null)
            {
                var host = Object.FindFirstObjectByType<EncounterController>();
                if (host != null && host.World != null) config = host.World.Config;
            }
            if (config == null)
                config = AssetDatabase.LoadAssetAtPath<NavigationConfig>(
                    MotionTestSceneTools.DataRoot + "/NavigationConfig.asset");
            Selection.activeObject = config;
            EditorGUIUtility.PingObject(config);
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox(
                "Размер поля, клетки и параметры агента применяются через Generate + Bake NavMesh.",
                MessageType.Info);
            DrawDefaultInspector();
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Цвета материалов поля", EditorStyles.boldLabel);
            var config = (NavigationConfig)target;
            DrawMaterialColor("Пол", config.Floor);
            DrawMaterialColor("Препятствия", config.Obstacle);
            DrawMaterialColor("Пандусы", config.Ramp);
            DrawMaterialColor("Платформы", config.Platform);
            DrawMaterialColor("Дорогие клетки", config.Expensive);
            DrawMaterialColor("Декорации", config.Decoration);
        }

        private static void DrawMaterialColor(string label, Material material)
        {
            if (material == null || !material.HasProperty("_BaseColor")) return;
            EditorGUI.BeginChangeCheck();
            var color = EditorGUILayout.ColorField(label, material.GetColor("_BaseColor"));
            if (!EditorGUI.EndChangeCheck()) return;
            Undo.RecordObject(material, "Change motion material color");
            material.SetColor("_BaseColor", color);
            EditorUtility.SetDirty(material);
        }
    }
}
