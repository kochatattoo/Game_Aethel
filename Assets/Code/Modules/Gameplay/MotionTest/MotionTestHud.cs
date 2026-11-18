using System.Collections.Generic;
using PetProject.Tactics;
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
        public bool TargetHealthVisible => _targetHealthText != null && _targetHealthText.gameObject.activeSelf;
        public string HeroHealthValue => _heroHealthText != null ? _heroHealthText.text : string.Empty;
        public string TargetHealthValue => _targetHealthText != null ? _targetHealthText.text : string.Empty;
        public string HistoryValue => _historyText != null ? _historyText.text : string.Empty;
        public string PlayerStatsValue => _playerStatsText != null ? _playerStatsText.text : string.Empty;
        public float HeroHealthFillAmount => _heroHealthFill != null ? _heroHealthFill.fillAmount : 0;

        private IMotionHudContext _host;
        private Text _modeText;
        private Text _modeButtonText;
        private Text _actionsText;
        private Text _heroHealthText;
        private Text _targetHealthText;
        private Text _noticeText;
        private Text _historyText;
        private Text _playerStatsText;
        private Image _heroHealthFill;
        private Font _font;
        private int _historyCount = -1;
        private string _historyLast;
        private bool _playerStatsInitialized;
        private RectTransform _turnQueuePanel;
        private Text _roundText;
        private readonly List<TurnQueueEntry> _turnQueueEntries = new List<TurnQueueEntry>();

        private sealed class TurnQueueEntry
        {
            public Image Background;
            public Text Label;
        }

        public void Bind(IMotionHudContext host)
        {
            _host = host;
            _host.Changed += Refresh;
            _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            EnsureEventSystem();
            var canvas = CreateCanvas();
            CreateTurnQueue(canvas);
            CreateHistory(canvas.transform);
            var panel = CreatePanel(canvas.transform);
            CreatePlayerStats(canvas.transform);
            _modeText = CreateText("Mode indicator", panel, new Vector2(10, -8),
                new Vector2(296, 22), 17, TextAnchor.MiddleLeft);
            ModeButton = CreateButton("Mode switch", panel, new Vector2(10, -36),
                new Vector2(296, 32), out _modeButtonText);
            EndTurnButton = CreateButton("End turn", panel, new Vector2(10, -72),
                new Vector2(296, 30), out var endTurnText);
            endTurnText.text = "Завершить ход";
            _actionsText = CreateText("Action points", panel, new Vector2(10, -108),
                new Vector2(296, 22), 15, TextAnchor.MiddleLeft);
            _heroHealthText = CreateText("Hero health", panel, new Vector2(10, -131),
                new Vector2(296, 18), 14, TextAnchor.MiddleLeft);
            CreateHeroHealthFill(panel);
            _targetHealthText = CreateText("Target health", panel, new Vector2(10, -162),
                new Vector2(296, 18), 14, TextAnchor.MiddleLeft);
            _noticeText = CreateText("Notice", panel, new Vector2(10, -184),
                new Vector2(296, 28), 13, TextAnchor.MiddleLeft);
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
            RefreshHistory();
            RefreshPlayerStats();
            if (_host.EncounterActive)
            {
                var director = _host.EncounterDirector;
                var snapshot = director.Snapshot;
                if (snapshot != null)
                {
                    RefreshEncounter(director, snapshot);
                    RefreshTurnQueue(snapshot);
                    return;
                }
            }
            RefreshExploration();
        }

        private void RefreshEncounter(SkeletonCombatDirector director, PetProject.Tactics.EncounterSnapshot snapshot)
        {
            var hero = snapshot.GetActor(MotionTestController.HeroActorId);
            _modeText.text = director.IsDefeated
                ? "Поражение"
                : $"Бой · {snapshot.CurrentActorId} · раунд {snapshot.Round}";
            _modeButtonText.text = director.CanReset ? "Сбросить бой" : "Бой до завершения";
            ModeButton.interactable = director.CanReset;
            EndTurnButton.gameObject.SetActive(!director.IsDefeated);
            EndTurnButton.interactable = director.PlayerTurnReady && hero.ActionPoints > 0;
            _actionsText.gameObject.SetActive(true);
            _actionsText.text = $"ОД {hero.ActionPoints}";
            _actionsText.color = Color.white;
            RefreshHealth();
            _noticeText.text = _host.Notice ?? string.Empty;
        }

        private void RefreshExploration()
        {
            _turnQueuePanel.gameObject.SetActive(false);
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
            RefreshHealth();
            _noticeText.text = _host.Notice ?? string.Empty;
        }

        private void CreatePlayerStats(Transform canvas)
        {
            var panel = CreateRect("Player characteristics", canvas, new Vector2(12, -240), new Vector2(316, 156));
            var background = panel.gameObject.AddComponent<Image>();
            background.color = new Color(.025f, .035f, .05f, .9f);
            background.raycastTarget = false;
            CreateText("Title", panel, new Vector2(10, -6), new Vector2(296, 22),
                16, TextAnchor.MiddleLeft).text = "Характеристики героя";
            _playerStatsText = CreateText("Values", panel, new Vector2(10, -32),
                new Vector2(296, 116), 14, TextAnchor.UpperLeft);
        }

        private void CreateHeroHealthFill(Transform panel)
        {
            var background = CreateSolidImage("Hero HP background", panel, new Vector2(10, -151),
                new Vector2(296, 8));
            background.color = new Color(.1f, .02f, .02f, .9f);
            _heroHealthFill = CreateSolidImage("Hero HP fill", background.rectTransform,
                new Vector2(1, -1), new Vector2(294, 6));
            _heroHealthFill.type = Image.Type.Filled;
            _heroHealthFill.fillMethod = Image.FillMethod.Horizontal;
            _heroHealthFill.fillOrigin = 0;
        }

        private static Image CreateSolidImage(string name, Transform parent, Vector2 position, Vector2 size)
        {
            var rect = CreateRect(name, parent, position, size);
            var image = rect.gameObject.AddComponent<Image>();
            image.raycastTarget = false;
            return image;
        }

        private void RefreshPlayerStats()
        {
            if (_playerStatsInitialized || _host.CombatRules == null || _playerStatsText == null) return;
            _playerStatsInitialized = true;
            var rules = _host.CombatRules;
            _playerStatsText.text =
                $"СИЛ {rules.HeroStrength} ({SignedStat(rules.HeroStrength)})   " +
                $"ЛОВ {rules.HeroDexterity} ({SignedStat(rules.HeroDexterity)})   " +
                $"ТЕЛ {rules.HeroConstitution} ({SignedStat(rules.HeroConstitution)})\n" +
                $"ИНТ {rules.HeroIntelligence} ({SignedStat(rules.HeroIntelligence)})   " +
                $"МУД {rules.HeroWisdom} ({SignedStat(rules.HeroWisdom)})   " +
                $"ХАР {rules.HeroCharisma} ({SignedStat(rules.HeroCharisma)})\n" +
                $"HP {rules.HeroMaxHealth} · Броня {rules.HeroDefense}\n" +
                $"Атака +{rules.HeroAttackBonus} · Урон {rules.HeroDamage}\n" +
                $"Инициатива +{rules.HeroInitiativeBonus}";
        }

        private static string SignedStat(int characteristic)
        {
            var modifier = MotionCombatRulesConfig.Modifier(characteristic);
            return modifier >= 0 ? "+" + modifier : modifier.ToString();
        }

        private void CreateHistory(Transform canvas)
        {
            var panel = CreateRect("Action history", canvas, new Vector2(-12, 12), new Vector2(420, 190));
            panel.anchorMin = panel.anchorMax = new Vector2(1, 0);
            panel.pivot = new Vector2(1, 0);
            var background = panel.gameObject.AddComponent<Image>();
            background.color = new Color(.025f, .035f, .05f, .9f);
            background.raycastTarget = false;
            CreateText("History title", panel, new Vector2(8, -6), new Vector2(404, 22),
                15, TextAnchor.MiddleLeft).text = "История действий";
            _historyText = CreateText("History entries", panel, new Vector2(8, -32),
                new Vector2(404, 150), 13, TextAnchor.UpperLeft);
        }

        private void RefreshHistory()
        {
            var history = _host.History;
            if (history == null)
            {
                _historyText.text = string.Empty;
                return;
            }
            var last = history.Count > 0 ? history[history.Count - 1] : null;
            if (_historyCount == history.Count && _historyLast == last) return;
            _historyCount = history.Count;
            _historyLast = last;
            if (history.Count == 0)
            {
                _historyText.text = "Действий пока нет";
                return;
            }
            var first = System.Math.Max(0, history.Count - 6);
            _historyText.text = string.Empty;
            for (var i = first; i < history.Count; ++i)
                _historyText.text += (i > first ? "\n" : string.Empty) + "• " + history[i];
        }

        private void CreateTurnQueue(Transform canvas)
        {
            _turnQueuePanel = CreateRect("Turn queue", canvas, new Vector2(0, -12), new Vector2(484, 88));
            _turnQueuePanel.anchorMin = _turnQueuePanel.anchorMax = new Vector2(.5f, 1);
            _turnQueuePanel.pivot = new Vector2(.5f, 1);
            var background = _turnQueuePanel.gameObject.AddComponent<Image>();
            background.color = new Color(.025f, .035f, .05f, .92f);
            background.raycastTarget = false;
            _roundText = CreateText("Round", _turnQueuePanel, new Vector2(8, -4),
                new Vector2(468, 24), 16, TextAnchor.MiddleCenter);
            _turnQueuePanel.gameObject.SetActive(false);
        }

        private void RefreshTurnQueue(EncounterSnapshot snapshot)
        {
            _turnQueuePanel.gameObject.SetActive(true);
            _turnQueuePanel.sizeDelta = new Vector2(Mathf.Max(180, snapshot.Actors.Count * 156 + 16), 88);
            _roundText.text = $"Раунд {snapshot.Round}";
            _roundText.rectTransform.sizeDelta = new Vector2(_turnQueuePanel.sizeDelta.x - 16, 24);
            EnsureTurnQueueEntries(snapshot.Actors.Count);
            var entryIndex = 0;
            for (var completedPass = 0; completedPass < 2; ++completedPass)
            foreach (var actor in snapshot.Actors)
            {
                if (actor.HasCompletedTurn != (completedPass == 1)) continue;
                var entry = _turnQueueEntries[entryIndex++];
                entry.Label.text = actor.IsAlive
                    ? $"{ActorDisplayName(actor.Id)}\nОД {actor.ActionPoints}"
                    : $"{ActorDisplayName(actor.Id)}\nПобеждён";
                entry.Label.color = actor.HasCompletedTurn ? new Color(.55f, .58f, .62f) : Color.white;
                entry.Background.color = actor.HasCompletedTurn
                    ? new Color(.16f, .17f, .19f, .9f)
                    : actor.Id == snapshot.CurrentActorId
                        ? new Color(.12f, .42f, .32f, 1f)
                        : new Color(.16f, .24f, .32f, 1f);
            }
        }

        private void EnsureTurnQueueEntries(int count)
        {
            while (_turnQueueEntries.Count > count)
            {
                var last = _turnQueueEntries.Count - 1;
                Destroy(_turnQueueEntries[last].Background.gameObject);
                _turnQueueEntries.RemoveAt(last);
            }
            while (_turnQueueEntries.Count < count)
            {
                var index = _turnQueueEntries.Count;
                var rect = CreateRect("Turn " + (index + 1), _turnQueuePanel,
                    new Vector2(8 + index * 156, -32), new Vector2(148, 48));
                var image = rect.gameObject.AddComponent<Image>();
                image.raycastTarget = false;
                var label = CreateText("Actor", rect, Vector2.zero, new Vector2(148, 48),
                    15, TextAnchor.MiddleCenter);
                StretchToParent(label.rectTransform);
                _turnQueueEntries.Add(new TurnQueueEntry { Background = image, Label = label });
            }
        }

        private static string ActorDisplayName(string actorId)
        {
            if (actorId == MotionTestController.HeroActorId) return "Герой";
            if (actorId == MotionEnemyTarget.SkeletonActorId) return "Скелет 1";
            if (actorId != null && actorId.StartsWith("skeleton-"))
                return "Скелет " + actorId.Substring("skeleton-".Length);
            return actorId;
        }

        private void RefreshHealth()
        {
            var director = _host.EncounterDirector;
            if (director == null) return;
            _heroHealthText.text = $"HP героя: {director.HeroHealth}/{director.HeroMaxHealth}";
            _heroHealthFill.fillAmount = director.HeroMaxHealth > 0
                ? Mathf.Clamp01((float)director.HeroHealth / director.HeroMaxHealth)
                : 0;
            _heroHealthFill.color = _heroHealthFill.fillAmount > .5f ? new Color(.2f, .8f, .35f) :
                _heroHealthFill.fillAmount > .25f ? new Color(1f, .65f, .15f) : new Color(.9f, .18f, .15f);
            _targetHealthText.gameObject.SetActive(director.IsEnemySelected);
            if (director.IsEnemySelected)
                _targetHealthText.text =
                    $"{director.SelectedEnemyName} · HP {director.EnemyHealth}/{director.EnemyMaxHealth}";
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
            var panel = CreateRect("Mode panel", parent, new Vector2(12, -12), new Vector2(316, 220));
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
