using CodeBase.MotionTest;
using UnityEditor;
using UnityEngine;

namespace CodeBase.EditorTools
{
    public sealed class MotionTestWindow : EditorWindow
    {
        private MotionTestController _host;
        private Vector2 _scroll;
        private double _nextRefresh;
        [MenuItem("Tools/PetProject/Motion/Test Motion Window")]
        public static void Open() => GetWindow<MotionTestWindow>("Test Motion").Show();
        private void OnInspectorUpdate()
        {
            if (_host == null) _host = FindFirstObjectByType<MotionTestController>();
            if (EditorApplication.timeSinceStartup < _nextRefresh) return;
            _nextRefresh = EditorApplication.timeSinceStartup + .1; Repaint();
        }
        private void OnGUI()
        {
            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            EditorGUILayout.LabelField("Test_MotionScene • движение", EditorStyles.boldLabel);
            if (_host == null)
            {
                EditorGUILayout.HelpBox("Откройте тестовую сцену. Управление персонажем — в Game View.", MessageType.Info);
                using (new EditorGUI.DisabledScope(EditorApplication.isPlaying))
                    if (GUILayout.Button("Открыть Test_MotionScene")) MotionTestSceneTools.Open();
                EditorGUILayout.EndScrollView(); return;
            }
            var playing = EditorApplication.isPlaying && !EditorApplication.isPaused;
            using (new EditorGUI.DisabledScope(EditorApplication.isPaused || EditorApplication.isCompiling))
            {
                var current = EditorApplication.isPlaying ? _host.Combat : _host.StartInCombat;
                var next = GUILayout.Toolbar(current ? 1 : 0, new[] { "Обычное", "В бою" });
                if ((next == 1) != current)
                {
                    if (playing) _host.SetCombat(next == 1);
                    else { Undo.RecordObject(_host, "Initial motion mode"); _host.StartInCombat = next == 1; EditorUtility.SetDirty(_host); }
                }
            }
            EditorGUILayout.Space();
            if (EditorApplication.isPlaying)
            {
                EditorGUILayout.LabelField("Состояние", _host.Status);
                EditorGUILayout.LabelField("Камера", $"yaw {_host.CameraRig.Yaw:F0}° / zoom {_host.CameraRig.Zoom:F1}");
                EditorGUILayout.HelpBox(_host.Notice ?? "", _host.Error == null ? MessageType.Info : MessageType.Error);
                if (_host.Hero != null) EditorGUILayout.LabelField("Мировая позиция", _host.Hero.transform.position.ToString("F2"));
                if (_host.Session != null)
                {
                    EditorGUILayout.LabelField("Логическая клетка", _host.Session.Position.ToString());
                    EditorGUILayout.LabelField("Очки действий", $"{_host.Session.Actions} / {_host.Session.Budget}");
                    EditorGUILayout.LabelField("Ревизия", _host.Session.Revision.ToString());
                }
                if (_host.Hover.HasValue) EditorGUILayout.LabelField("Цель / цена", $"{_host.Hover} / {(_host.Preview.Found ? _host.Preview.Cost.ToString() : "нет пути")}");
                using (new EditorGUI.DisabledScope(!playing || !_host.Ready || _host.Busy))
                {
                    if (GUILayout.Button("Новый тестовый ход")) _host.NewTurn();
                }
                using (new EditorGUI.DisabledScope(!playing))
                {
                    if (GUILayout.Button("Сброс героя")) _host.ResetHero();
                    if (GUILayout.Button("Фокус камеры")) _host.CameraRig.Focus();
                }
                _host.ShowAllCells = EditorGUILayout.Toggle("Показать всё поле", _host.ShowAllCells);
                if (_host.Map != null)
                {
                    EditorGUILayout.LabelField("Поле", $"{_host.World.Config.Dimension} × {_host.World.Config.Dimension}; доступно {_host.Map.WalkableCount}");
                    EditorGUILayout.LabelField("Сборка сетки", $"{_host.Map.BuildMilliseconds:F1} ms; blocked edges {_host.Map.BlockedEdgeCount}");
                    EditorGUILayout.LabelField("Последний preview", $"{_host.PreviewMilliseconds:F2} ms");
                }
            }
            else
            {
                EditorGUILayout.HelpBox("Play → фокус Game View → ЛКМ по земле. В бою: наведение показывает цену, ЛКМ выполняет путь.", MessageType.Info);
                var serialized = new SerializedObject(_host); serialized.Update();
                EditorGUILayout.PropertyField(serialized.FindProperty("TestBudget"), new GUIContent("ОД на тестовый ход"));
                serialized.ApplyModifiedProperties();
                EditorGUILayout.BeginHorizontal();
                foreach (var budget in new[] { 3, 6, 12 })
                    if (GUILayout.Button(budget + " ОД")) { Undo.RecordObject(_host, "Motion budget"); _host.TestBudget = budget; EditorUtility.SetDirty(_host); }
                EditorGUILayout.EndHorizontal();
                if (_host.World != null && _host.World.Config != null)
                {
                    if (_host.World.BakedSignature != _host.World.Signature)
                        EditorGUILayout.HelpBox("Геометрия или настройки изменились: выполните Generate + Bake перед Play.", MessageType.Warning);
                    var config = new SerializedObject(_host.World.Config); config.Update();
                    foreach (var name in new[] { "Seed", "CellSize", "VisibleRadius", "Speed" }) EditorGUILayout.PropertyField(config.FindProperty(name));
                    config.ApplyModifiedProperties();
                    if (GUILayout.Button("Generate + Bake NavMesh (Undo)")) MotionTestSceneTools.Rebuild(_host.World);
                    EditorGUILayout.LabelField("Габариты", "100 × 100 units");
                }
            }
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("ЛКМ — движение\nПКМ + мышь — вращение\nQ / E — поворот 45°\nКолесо — масштаб\nF — исходный ракурс\nСетка: зелёный путь доступен, красный — не хватает ОД.", MessageType.None);
            EditorGUILayout.EndScrollView();
        }
    }
}
