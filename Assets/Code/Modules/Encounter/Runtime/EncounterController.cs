using System;
using System.Collections.Generic;
using CodeBase.Hero;
using CodeBase.Infrastructure.Services.AIServices.BehaviourTree.Configuration;
using PetProject.Encounter;
using PetProject.Navigation;
using PetProject.Tactics;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using CodeBase.AI.Tactics;
using CodeBase.Enemies;
using CodeBase.Infrastructure.Services;
using CodeBase.Infrastructure.Utils;
using CodeBase.UI.Encounter;

namespace PetProject.Encounter
{
    public sealed class EncounterController : MonoBehaviour, ICameraInput, ITacticalGridContext,
        IEncounterHudContext, IEncounterContext
    {
        public EncounterWorld World;
        public GameObject HeroPrefab;
        public GameObject EnemyPrefab;
        public GameObject MoveMarkerPrefab;
        public MeleeTacticsAIConfig EnemyAI;
        public EncounterRulesConfig CombatRules;
        public EnemySpawnDefinition[] EnemySpawns =
        {
            new EnemySpawnDefinition(new Vector3(-6, 0, -18), "skeleton-patrol"),
            new EnemySpawnDefinition(new Vector3(0, 0, -20), "skeleton-patrol"),
            new EnemySpawnDefinition(new Vector3(4, 0, -12), "skeleton-patrol"),
            new EnemySpawnDefinition(new Vector3(-22, 0, 12), "skeleton-crypt"),
            new EnemySpawnDefinition(new Vector3(-16, 0, 14), "skeleton-crypt"),
            new EnemySpawnDefinition(new Vector3(22, 0, 22), "skeleton-general")
        };
        public Camera GameCamera;
        public IsometricCameraController CameraRig;
        public InputActionAsset InputAsset;
        public bool StartInCombat;
        public bool UseLegacyHud;
        public event Action Changed;
        // Player builds allow input directly; the editor bridge gates the real Game View.
        public Func<bool> PointerInputFilter { get; set; }
        public Func<bool> KeyboardInputFilter { get; set; }
        public bool PointerInputAllowed => PointerInput?.PointerAllowed ?? true;
        public bool KeyboardInputAllowed => PointerInput?.KeyboardAllowed ?? true;
        public bool ShowAllCells;
        public int ActionBudget = 6;
        public PointerInputProvider PointerInput { get; private set; }
        public HeroMotor HeroController { get; private set; }
        public NavMeshGridMap Map { get; private set; }
        public TacticalMovementSession Session => _tacticalMode?.Session;
        public WorldTurnClock WorldTurn { get; private set; }
        public WorldActionBudgets WorldActions { get; private set; }
        public EncounterHudView Hud { get; private set; }
        public EncounterDirector EncounterDirector { get; private set; }
        public GameObject Hero => HeroController?.Instance;
        public NavMeshAgent HeroAgent => HeroController?.Agent;
        public HeroAnimator HeroAnimator => HeroController?.Animator;
        public bool Combat => _tacticalMode?.Active ?? false;
        public bool EncounterActive => EncounterDirector != null && EncounterDirector.Active;
        public bool EncounterPending => EncounterDirector != null && EncounterDirector.Pending;
        public bool TacticalActive => Combat || EncounterActive;
        public long TacticalRevision => Session?.Revision ?? EncounterDirector?.Revision ?? -1;
        public GridPosition TacticalPosition => Session?.Position ?? EncounterDirector?.HeroPosition ?? default;
        public int TacticalActions => Session?.Actions ?? EncounterDirector?.HeroActions ?? 0;
        public bool Busy => _tacticalMode?.Busy ?? false;
        public bool Ready => Map != null && HeroController != null && Error == null;
        public bool CanSwitchMode => Ready && _tacticalMode.CanSwitch && !EncounterActive && !EncounterPending;
        public bool PointerOverUi => Hud != null ? Hud.PointerOver :
            EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
        public bool MovePressed => PointerInput.MovePressed;
        public string Error { get; private set; }
        public string Notice { get; private set; } = "Обычное движение";
        public IReadOnlyList<string> History => _history;
        public GridPath Preview => _previewState.Path;
        public GridPosition? Hover => _previewState.Hover;
        public double PreviewMilliseconds => _previewState.Milliseconds;
        public int AcceptedMoves => _tacticalMode?.AcceptedMoves ?? 0;
        public bool IsAnimating => HeroController != null && HeroController.IsAnimating;
        public bool NormalMoving => HeroController != null && HeroController.NormalMoving;
        public string Status => Error != null ? "Error: " + Error :
            _tacticalMode != null && _tacticalMode.Alignment ? "Aligning" : Busy ? "Moving" : "Ready";

        private readonly EncounterPreviewState _previewState = new();
        private readonly List<string> _history = new List<string>();
        private TacticalMovementController _tacticalMode;
        private TacticalGridView _view;
        private NavigationPathView _navigationFeedback;
        private ICharacterMovement _characterMovement;
        private int _lastWorldTenths = -1;

        NavigationConfig ITacticalGridContext.Config => World.Config;
        bool ITacticalGridContext.ShowAllCells => ShowAllCells;
        int IEncounterHudContext.ActionBudget => ActionBudget;
        IEncounterCombatContext IEncounterHudContext.EncounterDirector => EncounterDirector;
        EncounterWorld IEncounterContext.World => World;
        IEncounterRules IEncounterHudContext.CombatRules => CombatRules;
        IsometricCameraController IEncounterContext.CameraRig => CameraRig;
        GameObject IEncounterContext.EnemyPrefab => EnemyPrefab;
        MeleeTacticsAIConfig IEncounterContext.EnemyAI => EnemyAI;
        EncounterRulesConfig IEncounterContext.CombatRules => CombatRules;
        Camera IEncounterContext.GameCamera => GameCamera;
        System.Collections.Generic.IReadOnlyList<EnemySpawnDefinition> IEncounterContext.EnemySpawns => EnemySpawns;
        int IEncounterContext.ActionBudget => ActionBudget;

        private void Awake()
        {
            try
            {
                ValidateWorld();
                WorldTurn = new WorldTurnClock();
                WorldActions = new WorldActionBudgets();
                HeroController = HeroMotor.Create(HeroPrefab, World.Config);
                Physics.SyncTransforms();
                Map = new NavMeshGridMap(World.Config);
                _characterMovement = new HeroNavigationAdapter(HeroController, Map);
                _tacticalMode = new TacticalMovementController(HeroController, Map, World.Config, WorldActions,
                    () => ActionBudget, SetNotice, Fail, InvalidatePreview, RefreshTacticalView);
                PointerInput = new PointerInputProvider(InputAsset,
                    () => PointerInputFilter?.Invoke() ?? true, () => KeyboardInputFilter?.Invoke() ?? true);
                CameraRig.Bind(this, Hero.transform, PointerInput.Actions);
                _view = gameObject.AddComponent<TacticalGridView>();
                _view.Bind(this);
                _navigationFeedback = gameObject.AddComponent<NavigationPathView>();
                _navigationFeedback.Bind(World.Config, MoveMarkerPrefab, new NavigationLifecycle());
                if (!UseLegacyHud)
                {
                    Hud = gameObject.AddComponent<EncounterHudView>();
                    Hud.Bind(this);
                }
                EncounterDirector = gameObject.AddComponent<EncounterDirector>();
                EncounterDirector.Bind(this);
                if (StartInCombat) SetCombat(true);
            }
            catch (Exception error)
            {
                Fail(error.Message);
                Debug.LogException(error, this);
            }
        }

        private void Update()
        {
            if (!Ready || Time.timeScale == 0) return;
            if (!Combat && !_tacticalMode.Alignment && !EncounterActive && !EncounterPending &&
                WorldTurn.Advance(Time.deltaTime) > 0)
            {
                WorldActions.ResetAll();
                NotifyChanged();
            }
            NotifyWorldClock();
            _navigationFeedback.Tick(HeroAgent, NormalMoving);
            if (EncounterActive) return;
            _tacticalMode.Tick();
            if (EncounterPending) return;
            if (!PointerInputAllowed || PointerOverUi || CameraRig.Orbiting) return;
            if (!PointerInput.TryRaycast(GameCamera, out var hit))
            {
                ClearHover();
                return;
            }
            if (MovePressed && EncounterDirector != null && EncounterDirector.HandleExplorationClick(hit.collider)) return;
            var surface = hit.collider.GetComponent<NavigationSurface>();
            if (surface == null || !surface.Walkable || !Map.Cell(hit.point, out var cell))
            {
                ClearHover();
                return;
            }
            if (Combat && !Busy) SetPreview(cell);
            if (!MovePressed || Busy) return;
            EncounterDirector?.ClearTargetSelection();
            if (Combat) TryMove(cell);
            else MoveNormally(hit.point);
        }

        public bool MoveNormally(Vector3 point)
        {
            if (!Ready || Combat || Busy || EncounterActive || EncounterPending) return false;
            var destination = new NavigationPoint(point.x, point.y, point.z);
            if (!_characterMovement.TryMove(destination, out var error)) return Reject(error);
            _navigationFeedback.ShowPath(HeroAgent.path, HeroAgent.pathEndPosition);
            AddHistory("Герой: маршрут к " + point.ToString("F1"));
            SetNotice("Движение к " + point.ToString("F1"));
            return true;
        }

        public bool SetCombat(bool requested)
        {
            if (!Ready || EncounterActive || EncounterPending) return false;
            return _tacticalMode.SetActive(requested);
        }

        public GridPath SetPreview(GridPosition destination)
        {
            if (!Ready || !Combat || Busy) return GridPath.Unreachable;
            var path = _previewState.SetPreview(Session, destination);
            SetNotice(!path.Found ? "Нет пути" : path.Cost > Session.Actions
                ? $"Нужно {path.Cost} ОД; не хватает {path.Cost - Session.Actions}"
                : $"Путь: {path.Cost} ОД; останется {Session.Actions - path.Cost}");
            RefreshTacticalView();
            return path;
        }

        public bool TryMove(GridPosition destination)
        {
            if (!Ready || !Combat || Busy) return false;
            var result = _tacticalMode.Move(destination);
            if (!result.Accepted) return Reject(result.Error);
            _previewState.SetAcceptedPath(result.Path, destination);
            SetNotice($"Выполнение пути: {result.Path.Cost} ОД");
            RefreshTacticalView();
            return true;
        }

        public void PrepareForEncounter()
        {
            _navigationFeedback.Clear();
            _tacticalMode.PrepareForEncounter();
            ClearHover();
        }

        public void FinishEncounterAlignment() => _tacticalMode.FinishEncounterAlignment();

        public void RestoreExploration()
        {
            if (!_tacticalMode.RestoreExploration()) Fail("Не удалось вернуть героя на NavMesh");
            ClearHover();
        }

        public void SetNotice(string message)
        {
            Notice = message;
            NotifyChanged();
        }

        public void AddHistory(string message)
        {
            if (string.IsNullOrWhiteSpace(message)) return;
            _history.Add(message);
            if (_history.Count > 7) _history.RemoveAt(0);
            NotifyChanged();
        }

        public void RefreshTacticalView()
        {
            _view?.Refresh();
            NotifyChanged();
        }

        private void NotifyWorldClock()
        {
            var tenths = (int)Math.Ceiling(WorldTurn.SecondsToNextRound * 10);
            if (_lastWorldTenths == tenths) return;
            _lastWorldTenths = tenths;
            NotifyChanged();
        }

        private void NotifyChanged() => Changed?.Invoke();

        public void SetEncounterPreview(GridPath path, GridPosition cell)
        {
            _previewState.SetEncounterPreview(path, cell);
            RefreshTacticalView();
        }

        public void ClearPreview()
        {
            InvalidatePreview();
            RefreshTacticalView();
        }

        public void NewTurn()
        {
            if (Ready) _tacticalMode.NewTurn();
        }

        public void ResetHero()
        {
            if (HeroController == null) return;
            _navigationFeedback.Clear();
            EncounterDirector?.ResetEncounterState();
            Error = null;
            WorldTurn.Reset();
            WorldActions.ResetAll();
            _tacticalMode.Reset();
            ClearHover();
            CameraRig.Focus();
            SetNotice("Герой сброшен");
        }

        private void ValidateWorld()
        {
            if (World == null || World.Config == null || World.Surface == null || World.Surface.navMeshData == null)
                throw new InvalidOperationException("Generate and bake the motion scene before Play");
            if (World.BakedSignature != World.Signature)
                throw new InvalidOperationException("Configuration changed: rebuild the motion world in Edit Mode");
            var settings = NavMesh.GetSettingsByID(World.Config.AgentTypeId);
            if (Mathf.Abs(settings.agentRadius - World.Config.AgentRadius) > .001f ||
                Mathf.Abs(settings.agentHeight - World.Config.AgentHeight) > .001f)
                throw new InvalidOperationException("Motion config dimensions must match its existing Navigation agent type");
        }

        private bool Reject(string message)
        {
            SetNotice(message switch
            {
                "NotEnoughActions" => "Недостаточно очков действий",
                "AlreadyAtDestination" => "Герой уже находится в этой клетке",
                "NoPath" => "Нет пути к выбранной клетке",
                "OutsideBoard" => "Цель вне поля",
                "StalePreview" => "Состояние изменилось — выберите путь заново",
                _ => message
            });
            return false;
        }

        private void Fail(string message)
        {
            Error = message;
            HeroController?.Animator.StopMoving();
            NotifyChanged();
        }

        private void InvalidatePreview() => _previewState.Clear();

        private void ClearHover()
        {
            if (Hover == null) return;
            InvalidatePreview();
            RefreshTacticalView();
        }

        private void OnDestroy()
        {
            PointerInput?.Dispose();
            HeroController?.Dispose();
        }
    }
}
