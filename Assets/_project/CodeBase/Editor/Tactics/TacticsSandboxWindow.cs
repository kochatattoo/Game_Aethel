using System;
using System.Linq;
using CodeBase.TacticsSandbox;
using PetProject.Tactics;
using UnityEditor;
using UnityEngine;

namespace CodeBase.EditorTools
{
    public sealed class TacticsSandboxWindow : EditorWindow
    {
        private TacticsSandboxBehaviour _host;
        private GridPosition _destination = new(2, 1);
        private GridPosition _previewDestination;
        private long _previewRevision = -1;
        private GridPath _preview;
        private Vector2 _scroll;

        [MenuItem("Tools/PetProject/Tactics/Open Command Window")]
        public static void Open()
        {
            var window = GetWindow<TacticsSandboxWindow>("Aethel: тактика");
            window.minSize = new Vector2(460, 640);
            if (!window.docked)
            {
                // Unity may remember a floating window on a disconnected display.
                var main = EditorGUIUtility.GetMainWindowPosition();
                window.position = new Rect(main.x + (main.width - 500) / 2,
                    main.y + Math.Max(30, (main.height - 720) / 2), 500, 720);
            }
            window.Show();
            window.Focus();
        }

        private void OnInspectorUpdate()
        {
            if (_host == null) _host = FindFirstObjectByType<TacticsSandboxBehaviour>();
            Repaint();
        }

        private void OnGUI()
        {
            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            EditorGUILayout.LabelField("D1 — технический полигон", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Временные правила: ОД и цена атаки задаются на компоненте, движение — по цене клеток. " +
                "Ход завершается вручную. Здесь разработчик управляет обеими сторонами.", MessageType.Info);
            using (new EditorGUI.DisabledScope(EditorApplication.isPlaying))
                if (GUILayout.Button("Создать / открыть сцену полигона")) TacticsSandboxTools.OpenScene();
            if (!EditorApplication.isPlaying || _host == null || !_host.IsInitialized)
            {
                EditorGUILayout.HelpBox("Откройте полигон и включите Play. Основной игровой проект и сохранения не подключаются.", MessageType.Info);
                EditorGUILayout.EndScrollView();
                return;
            }
            var snapshot = _host.Snapshot;
            EditorGUILayout.LabelField($"Раунд {snapshot.Round} · ревизия {snapshot.Revision}");
            EditorGUILayout.LabelField(snapshot.IsCompleted ? "Победа: " + snapshot.WinningFactionId : "Активный: " + snapshot.CurrentActorId);
            foreach (var actor in snapshot.Actors)
                EditorGUILayout.LabelField($"{actor.Id}  HP {actor.Health}/{actor.Definition.MaxHealth}  ОД {actor.ActionPoints}  " +
                    $"клетка {actor.Position}  инициатива {actor.Initiative}");
            GUILayout.Space(8);
            DrawBoard(snapshot);
            _destination = new GridPosition(EditorGUILayout.IntField("Клетка X", _destination.X), EditorGUILayout.IntField("Клетка Y", _destination.Y));
            if (_previewRevision != snapshot.Revision || _previewDestination != _destination)
            {
                _preview = _host.PreviewMove(_destination);
                _previewRevision = snapshot.Revision;
                _previewDestination = _destination;
            }
            EditorGUILayout.LabelField(_preview?.Found == true ? "Цена пути: " + _preview.Cost + " ОД" : "Путь недоступен");
            using (new EditorGUI.DisabledScope(snapshot.IsCompleted))
            {
                if (GUILayout.Button("Переместить активного")) Execute(TacticalCommand.Move(NewId(), snapshot.CurrentActorId, _destination));
                var actor = snapshot.Actors.FirstOrDefault(a => a.Id == snapshot.CurrentActorId);
                var target = snapshot.Actors.FirstOrDefault(a => a.IsAlive && a.FactionId != actor?.FactionId);
                using (new EditorGUI.DisabledScope(target == null))
                    if (GUILayout.Button("Атаковать противника")) Execute(TacticalCommand.Attack(NewId(), snapshot.CurrentActorId, target.Id));
                if (GUILayout.Button("Завершить ход")) Execute(TacticalCommand.EndTurn(NewId(), snapshot.CurrentActorId));
            }
            if (GUILayout.Button("Сбросить прототип")) { _host.ResetEncounter(); _previewRevision = -1; }
            var result = _host.LastResult;
            if (result != null)
            {
                EditorGUILayout.HelpBox(result.Accepted ? "Команда принята" : "Отказ: " + result.Error,
                    result.Accepted ? MessageType.Info : MessageType.Warning);
                foreach (var item in result.Events)
                    EditorGUILayout.LabelField($"{item.Kind}: {item.ActorId} → {item.TargetId}, значение {item.Amount}, d20 {item.Roll}", EditorStyles.wordWrappedMiniLabel);
            }
            EditorGUILayout.EndScrollView();
        }

        private void DrawBoard(EncounterSnapshot snapshot)
        {
            var oldColor = GUI.backgroundColor;
            for (var y = _host.Board.Height - 1; y >= 0; --y)
            {
                EditorGUILayout.BeginHorizontal();
                for (var x = 0; x < _host.Board.Width; ++x)
                {
                    var cell = new GridPosition(x, y);
                    var unit = snapshot.Actors.FirstOrDefault(a => a.IsAlive && a.Position == cell);
                    var tile = _host.Board.GetCell(cell);
                    GUI.backgroundColor = cell == _destination ? Color.yellow : unit?.FactionId == "allies" ? Color.green : unit != null ? Color.red : oldColor;
                    var label = unit != null ? unit.Id : !tile.Walkable ? "#" : tile.MoveCost.ToString();
                    if (GUILayout.Button(new GUIContent(label, $"{cell}, высота {tile.Height}"), GUILayout.Height(26))) _destination = cell;
                }
                EditorGUILayout.EndHorizontal();
            }
            GUI.backgroundColor = oldColor;
        }

        private void Execute(TacticalCommand command) => _host.Execute(command);
        private static string NewId() => Guid.NewGuid().ToString("N");
    }
}
