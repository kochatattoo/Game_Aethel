using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace CodeBase.MotionTest
{
    public sealed class MotionTestHud : MonoBehaviour
    {
        public Button ModeButton { get; private set; }
        public Button EndTurnButton { get; private set; }
        public bool PointerOver => EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();

        private IMotionHudContext _host;
        private Text _modeText;
        private Text _modeButtonText;
        private Text _actionsText;
        private Text _noticeText;
        private Font _font;

        public void Bind(IMotionHudContext host)
        {
            _host = host;
            _host.Changed += Refresh;
            _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            EnsureEventSystem();
            var canvas = CreateCanvas();
            var panel = CreatePanel(canvas.transform);
            _modeText = CreateText("Mode indicator", panel, new Vector2(12, -10),
                new Vector2(436, 28), 20, TextAnchor.MiddleLeft);
            ModeButton = CreateButton("Mode switch", panel, new Vector2(12, -44),
                new Vector2(436, 40), out _modeButtonText);
            EndTurnButton = CreateButton("End turn", panel, new Vector2(12, -91),
                new Vector2(436, 36), out var endTurnText);
            endTurnText.text = "Завершить ход — обновить ОД";
            _actionsText = CreateText("Action points", panel, new Vector2(12, -133),
                new Vector2(436, 25), 18, TextAnchor.MiddleLeft);
            _noticeText = CreateText("Notice", panel, new Vector2(12, -161),
                new Vector2(436, 30), 15, TextAnchor.MiddleLeft);
            ModeButton.onClick.AddListener(() =>
            {
                if (_host.EncounterDirector != null && _host.EncounterDirector.CanReset)
                    _host.EncounterDirector.ResetCombat();
                else
                    _host.SetCombat(!_host.Combat);
            });
            EndTurnButton.onClick.AddListener(() =>
            {
                if (_host.EncounterActive) _host.EncounterDirector.EndTurnFromHud();
                else _host.NewTurn();
            });
            Refresh();
        }

        private void OnDestroy()
        {
            if (_host != null) _host.Changed -= Refresh;
        }

        private void Refresh()
        {
            if (_host == null || ModeButton == null) return;
            if (_host.EncounterActive)
            {
                var director = _host.EncounterDirector;
                var snapshot = director.Snapshot;
                if (snapshot != null)
                {
                    RefreshEncounter(director, snapshot);
                    return;
                }
            }
            RefreshExploration();
        }

        private void RefreshEncounter(SkeletonCombatDirector director, PetProject.Tactics.EncounterSnapshot snapshot)
        {
            var hero = snapshot.GetActor(MotionTestController.HeroActorId);
            var enemy = snapshot.GetActor(MotionEnemyTarget.SkeletonActorId);
            _modeText.text = director.IsDefeated
                ? "Поражение"
                : $"Бой · {snapshot.CurrentActorId} · раунд {snapshot.Round}";
            _modeButtonText.text = director.CanReset ? "Сбросить бой" : "Бой до завершения";
            ModeButton.interactable = director.CanReset;
            EndTurnButton.gameObject.SetActive(!director.IsDefeated);
            EndTurnButton.interactable = director.PlayerTurnReady && hero.ActionPoints > 0;
            _actionsText.gameObject.SetActive(true);
            _actionsText.text = $"HP {hero.Health}/{hero.Definition.MaxHealth} · " +
                $"Враг {enemy.Health}/{enemy.Definition.MaxHealth} · ОД {hero.ActionPoints}";
            _actionsText.color = Color.white;
            _noticeText.text = _host.Notice ?? string.Empty;
        }

        private void RefreshExploration()
        {
            var aligning = _host.Status == "Aligning";
            _modeText.text = _host.Combat
                ? "Тактический режим"
                : aligning ? "Вход в тактический режим…" : "Обычное движение";
            _modeButtonText.text = _host.Combat ? "Выйти из тактики" : "Войти в тактику";
            ModeButton.interactable = _host.CanSwitchMode;
            EndTurnButton.gameObject.SetActive(_host.Combat);
            EndTurnButton.interactable = _host.Ready && _host.Combat && !_host.Busy &&
                _host.Session != null && _host.Session.Actions > 0;
            _actionsText.gameObject.SetActive(_host.WorldActions != null);
            if (_host.Session != null)
            {
                _actionsText.text = $"ОД: {_host.Session.Actions} / {_host.Session.Budget}";
                _actionsText.color = _host.Session.Actions == 0 ? new Color(1f, .72f, .2f) : Color.white;
            }
            else if (_host.WorldActions != null)
            {
                var budget = Mathf.Max(1, _host.TestBudget);
                _actionsText.text =
                    $"ОД для тактики: {_host.WorldActions.Get(MotionTestController.HeroActorId, budget)} / {budget} · " +
                    $"новый мировой ход: {_host.WorldTurn.SecondsToNextRound:F1} c";
                _actionsText.color = Color.white;
            }
            _noticeText.text = _host.Notice ?? string.Empty;
        }

        private Transform CreateCanvas()
        {
            var instance = new GameObject("Motion HUD", typeof(RectTransform), typeof(Canvas),
                typeof(CanvasScaler), typeof(GraphicRaycaster));
            instance.transform.SetParent(transform, false);
            var canvas = instance.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            var scaler = instance.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280, 800);
            scaler.matchWidthOrHeight = .5f;
            return instance.transform;
        }

        private void EnsureEventSystem()
        {
            if (EventSystem.current != null) return;
            var instance = new GameObject("Motion EventSystem", typeof(EventSystem),
                typeof(InputSystemUIInputModule));
            instance.transform.SetParent(transform, false);
        }

        private static RectTransform CreatePanel(Transform parent)
        {
            var panel = CreateRect("Mode panel", parent, new Vector2(16, -16), new Vector2(460, 202));
            var image = panel.gameObject.AddComponent<Image>();
            image.color = new Color(.025f, .035f, .05f, .9f);
            return panel;
        }

        private Text CreateText(string name, Transform parent, Vector2 position, Vector2 size,
            int fontSize, TextAnchor alignment)
        {
            var rect = CreateRect(name, parent, position, size);
            var text = rect.gameObject.AddComponent<Text>();
            text.font = _font;
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.color = Color.white;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            text.raycastTarget = false;
            return text;
        }

        private Button CreateButton(string name, Transform parent, Vector2 position, Vector2 size, out Text label)
        {
            var rect = CreateRect(name, parent, position, size);
            var image = rect.gameObject.AddComponent<Image>();
            image.color = new Color(.16f, .24f, .32f, 1f);
            var button = rect.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            var colors = button.colors;
            colors.highlightedColor = new Color(.25f, .39f, .5f);
            colors.pressedColor = new Color(.1f, .5f, .36f);
            colors.disabledColor = new Color(.12f, .14f, .17f, .65f);
            button.colors = colors;
            label = CreateText("Label", rect, Vector2.zero, size, 18, TextAnchor.MiddleCenter);
            StretchToParent(label.rectTransform);
            return button;
        }

        private static RectTransform CreateRect(string name, Transform parent, Vector2 position, Vector2 size)
        {
            var instance = new GameObject(name, typeof(RectTransform));
            var rect = instance.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = rect.anchorMax = new Vector2(0, 1);
            rect.pivot = new Vector2(0, 1);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            return rect;
        }

        private static void StretchToParent(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(.5f, .5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = Vector2.zero;
        }
    }
}
