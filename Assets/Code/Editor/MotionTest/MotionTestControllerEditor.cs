using CodeBase.MotionTest;
using UnityEditor;
using UnityEngine;

namespace CodeBase.EditorTools
{
    [CustomEditor(typeof(MotionTestController))]
    public sealed class MotionTestControllerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            var controller = (MotionTestController)target;
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Связывание групп", EditorStyles.boldLabel);
            if (controller.CombatRules == null || controller.EnemySpawns == null)
            {
                EditorGUILayout.HelpBox("Назначьте MotionCombatRulesConfig для проверки связей.", MessageType.Warning);
                return;
            }
            var radius = controller.CombatRules.GroupLinkRadius;
            for (var i = 0; i < controller.EnemySpawns.Length; ++i)
            {
                var spawn = controller.EnemySpawns[i];
                if (spawn == null) continue;
                var linked = 0;
                foreach (var other in controller.EnemySpawns)
                {
                    if (other == null || other == spawn || other.GroupId != spawn.GroupId) continue;
                    if (Vector3.Distance(spawn.Position, other.Position) <= radius) ++linked;
                }
                EditorGUILayout.LabelField(
                    $"Скелет {i + 1}: {spawn.GroupId} · союзников в радиусе {linked}");
            }
        }

        private void OnSceneGUI()
        {
            var controller = (MotionTestController)target;
            if (controller.CombatRules == null || controller.EnemySpawns == null) return;
            var radius = controller.CombatRules.GroupLinkRadius;
            for (var i = 0; i < controller.EnemySpawns.Length; ++i)
            {
                var spawn = controller.EnemySpawns[i];
                if (spawn == null) continue;
                Handles.color = GroupColor(spawn.GroupId);
                Handles.DrawWireDisc(spawn.Position, Vector3.up, radius);
                Handles.Label(spawn.Position + Vector3.up * 4,
                    $"Скелет {i + 1}\n{spawn.GroupId}");
                foreach (var other in controller.EnemySpawns)
                {
                    if (other == null || other == spawn || other.GroupId != spawn.GroupId ||
                        Vector3.Distance(spawn.Position, other.Position) > radius) continue;
                    Handles.DrawLine(spawn.Position, other.Position);
                }
            }
        }

        private static Color GroupColor(string groupId)
        {
            var hash = string.IsNullOrEmpty(groupId) ? 0 : groupId.GetHashCode();
            return Color.HSVToRGB(Mathf.Repeat((hash & 1023) / 1024f, 1), .65f, 1);
        }
    }
}
