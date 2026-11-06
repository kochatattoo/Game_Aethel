using System;
using CodeBase.Hero;
using PetProject.Tactics;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

namespace CodeBase.MotionTest
{
    public sealed class MotionTestController : MonoBehaviour, IMotionCameraInput, IMotionGridContext,
        IMotionHudContext, IMotionEncounterContext
    {
        public MotionTestWorld World;
        public GameObject HeroPrefab;
        public GameObject EnemyPrefab;
        public Vector3 EnemySpawn = new Vector3(-4.5f, 0, -15.5f);
        public Camera GameCamera;
        public MotionCameraController CameraRig;
        public InputActionAsset InputAsset;
        public bool StartInCombat;
        public event Action Changed;
        // Player builds allow input directly; the editor bridge gates the real Game View.
        public Func<bool> PointerInputFilter { get; set; }
        public Func<bool> KeyboardInputFilter { get; set; }
        public bool PointerInputAllowed => MotionInput?.PointerAllowed ?? true;
        public bool KeyboardInputAllowed => MotionInput?.KeyboardAllowed ?? true;
        public bool ShowAllCells;
        public int TestBudget = 6;
        public MotionPointerInput MotionInput { get; private set; }
        public MotionHero HeroController { get; private set; }
        public SurfaceGridMap Map { get; private set; }
        public MotionTestSession Session => _tacticalMode?.Session;
        public WorldTurnClock WorldTurn { get; private set; }
        public WorldActionBudgets WorldActions { get; private set; }
        public MotionTestHud Hud { get; private set; }
        public SkeletonCombatDirector EncounterDirector { get; private set; }
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
        public bool PointerOverUi => Hud != null && Hud.PointerOver;
        public bool MovePressed => MotionInput.MovePressed;
        public string Error { get; private set; }
        public string Notice { get; private set; } = "Обычное движение";
        public GridPath Preview => _previewState.Path;
        public GridPosition? Hover => _previewState.Hover;
        public double PreviewMilliseconds => _previewState.Milliseconds;
        public int AcceptedMoves => _tacticalMode?.AcceptedMoves ?? 0;
        public bool IsAnimating => HeroController != null && HeroController.IsAnimating;
        public bool NormalMoving => HeroController != null && HeroController.NormalMoving;
        public string Status => Error != null ? "Error: " + Error :
            _tacticalMode != null && _tacticalMode.Alignment ? "Aligning" : Busy ? "Moving" : "Ready";
        public const string HeroActorId = "hero";

        private readonly MotionPreviewState _previewState = new();
        private MotionTacticalMode _tacticalMode;
        private MotionGridView _view;
        private int _lastWorldTenths = -1;

        MotionTestWorld IMotionGridContext.World => World;
        bool IMotionGridContext.ShowAllCells => ShowAllCells;
        int IMotionHudContext.TestBudget => TestBudget;
        MotionTestWorld IMotionEncounterContext.World => World;
        MotionCameraController IMotionEncounterContext.CameraRig => CameraRig;
        GameObject IMotionEncounterContext.EnemyPrefab => EnemyPrefab;
        Camera IMotionEncounterContext.GameCamera => GameCamera;
        Vector3 IMotionEncounterContext.EnemySpawn => EnemySpawn;
        int IMotionEncounterContext.TestBudget => TestBudget;

        private void Awake()
        {
            try
            {
                ValidateWorld();
                WorldTurn = new WorldTurnClock();
                WorldActions = new WorldActionBudgets();
                HeroController = MotionHero.Create(HeroPrefab, World.Config);
                Physics.SyncTransforms();
                Map = new SurfaceGridMap(World.Config);
                _tacticalMode = new MotionTacticalMode(HeroController, Map, World.Config, WorldActions,
                    () => TestBudget, SetNotice, Fail, InvalidatePreview, RefreshTacticalView);
                MotionInput = new MotionPointerInput(InputAsset,
                    () => PointerInputFilter?.Invoke() ?? true, () => KeyboardInputFilter?.Invoke() ?? true);
                CameraRig.Bind(this, Hero.transform, MotionInput.Actions);
                _view = gameObject.AddComponent<MotionGridView>();
                _view.Bind(this);
                Hud = gameObject.AddComponent<MotionTestHud>();
                Hud.Bind(this);
                EncounterDirector = gameObject.AddComponent<SkeletonCombatDirector>();
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
            if (EncounterActive) return;
            _tacticalMode.Tick();
            if (EncounterPending) return;
            if (!PointerInputAllowed || PointerOverUi || CameraRig.Orbiting) return;
            if (!MotionInput.TryRaycast(GameCamera, out var hit))
            {
                ClearHover();
                return;
            }
            if (EncounterDirector != null && EncounterDirector.HandleExplorationClick(hit.collider)) return;
            var surface = hit.collider.GetComponent<MotionSurface>();
            if (surface == null || !surface.Walkable || !Map.Cell(hit.point, out var cell))
            {
                ClearHover();
                return;
            }
            if (Combat && !Busy) SetPreview(cell);
            if (!MovePressed || Busy) return;
            if (Combat) TryMove(cell);
            else MoveNormally(hit.point);
        }

        public bool MoveNormally(Vector3 point)
        {
            if (!Ready || Combat || Busy || EncounterActive || EncounterPending) return false;
            if (!HeroController.MoveNormally(point, Map, out var error)) return Reject(error);
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
            return _previewState.SetPreview(Session, destination, SetNotice, RefreshTacticalView);
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
            MotionInput?.Dispose();
            HeroController?.Dispose();
        }
    }
}
