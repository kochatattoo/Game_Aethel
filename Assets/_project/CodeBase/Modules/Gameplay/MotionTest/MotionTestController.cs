using System;
using System.Collections.Generic;
using CodeBase.Hero;
using PetProject.Tactics;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

namespace CodeBase.MotionTest
{
    public sealed class MotionTestController : MonoBehaviour
    {
        public MotionTestWorld World;
        public GameObject HeroPrefab;
        public Camera GameCamera;
        public MotionCameraController CameraRig;
        public InputActionAsset InputAsset;
        public bool StartInCombat;
        // Player builds allow input directly; the editor bridge gates the real Game View.
        public Func<bool> PointerInputFilter { get; set; }
        public Func<bool> KeyboardInputFilter { get; set; }
        public bool PointerInputAllowed => PointerInputFilter?.Invoke() ?? true;
        public bool KeyboardInputAllowed => KeyboardInputFilter?.Invoke() ?? true;
        public bool ShowAllCells;
        public int TestBudget = 6;
        public SurfaceGridMap Map { get; private set; }
        public MotionTestSession Session { get; private set; }
        public GameObject Hero { get; private set; }
        public bool Combat { get; private set; }
        public bool Busy => _alignment || _route != null;
        public bool Ready => Map != null && Hero != null && Error == null;
        public string Error { get; private set; }
        public string Notice { get; private set; } = "Обычное движение";
        public GridPath Preview { get; private set; } = GridPath.Unreachable;
        public GridPosition? Hover { get; private set; }
        public double PreviewMilliseconds { get; private set; }
        public int AcceptedMoves { get; private set; }
        public bool IsAnimating => _animation != null && _animation.Animator.GetBool("Move");
        public bool NormalMoving => _agent != null && _agent.enabled && _agent.isOnNavMesh &&
            (_agent.pathPending || _agent.remainingDistance > _agent.stoppingDistance);
        public string Status => Error != null ? "Error: " + Error : _alignment ? "Aligning" : _route != null ? "Moving" : "Ready";
        private InputActionAsset _actions;
        private InputActionMap _input;
        private NavMeshAgent _agent;
        private HeroPathFollower _follower;
        private HeroAnimator _animation;
        private MotionGridView _view;
        private IReadOnlyList<GridPosition> _route;
        private int _routeIndex;
        private bool _alignment, _exitAfterMove;
        private GridPosition _alignedCell;
        private long _hoverRevision = -1;
        private int _command;
        private float _alignmentElapsed;

        private void Awake()
        {
            try
            {
                if (World == null || World.Config == null || World.Surface == null || World.Surface.navMeshData == null)
                    throw new InvalidOperationException("Generate and bake the motion scene before Play");
                if (World.BakedSignature != World.Signature) throw new InvalidOperationException("Configuration changed: rebuild the motion world in Edit Mode");
                var settings = NavMesh.GetSettingsByID(World.Config.AgentTypeId);
                if (Mathf.Abs(settings.agentRadius - World.Config.AgentRadius) > .001f || Mathf.Abs(settings.agentHeight - World.Config.AgentHeight) > .001f)
                    throw new InvalidOperationException("Motion config dimensions must match its existing Navigation agent type");
                Hero = Instantiate(HeroPrefab, World.Config.Spawn, Quaternion.identity);
                Hero.name = "Motion test hero";
                foreach (var child in Hero.GetComponentsInChildren<Transform>(true)) child.gameObject.layer = 2;
                _agent = Hero.GetComponent<NavMeshAgent>();
                _follower = Hero.GetComponent<HeroPathFollower>();
                _animation = Hero.GetComponent<HeroAnimator>();
                _animation.Animator.applyRootMotion = false;
                _animation.Animator.fireEvents = false;
                _animation.Animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
                _agent.speed = World.Config.Speed; _agent.angularSpeed = 540; _agent.acceleration = 30;
                _agent.stoppingDistance = .04f;
                _follower.Construct();
                // Construct uses the prefab's movement settings; keep this scene's speed/precision explicit.
                _agent.speed = World.Config.Speed; _agent.stoppingDistance = .04f;
                Physics.SyncTransforms();
                Map = new SurfaceGridMap(World.Config);
                _actions = Instantiate(InputAsset); _input = _actions.FindActionMap("Motion", true); _input.Enable();
                CameraRig.Bind(this, Hero.transform, _input);
                _view = gameObject.AddComponent<MotionGridView>(); _view.Bind(this);
                if (StartInCombat) SetCombat(true);
            }
            catch (Exception error) { Fail(error.Message); Debug.LogException(error, this); }
        }

        private void Update()
        {
            if (!Ready || Time.timeScale == 0) return;
            if (_alignment)
            {
                _alignmentElapsed += Time.deltaTime;
                if (_alignmentElapsed > 5 || (!_agent.pathPending && _agent.pathStatus != NavMeshPathStatus.PathComplete))
                { _follower.Stop(); _alignment = false; Reject("Не удалось безопасно выровнять героя; обычный режим сохранён"); }
            }
            if (_alignment && !_agent.pathPending && _agent.remainingDistance <= .06f)
            {
                _agent.ResetPath(); _agent.Warp(Map.World(_alignedCell));
                _follower.Stop(); _follower.enabled = false; _agent.enabled = false; _animation.StopMoving();
                Session = new MotionTestSession(Map.Board, _alignedCell, Math.Max(1, TestBudget), World.Config.MaxHeightStep);
                Combat = true; _alignment = false; Notice = "Тактическое перемещение"; InvalidatePreview();
            }
            if (_route != null) AdvanceRoute();
            if (!Busy && _exitAfterMove) { _exitAfterMove = false; SetCombat(false); }
            if (!PointerInputAllowed || CameraRig.Orbiting) return;
            var cursor = _input["Point"].ReadValue<Vector2>();
            if (cursor.x < 0 || cursor.y < 0 || cursor.x >= Screen.width || cursor.y >= Screen.height) return;
            if (!Physics.Raycast(GameCamera.ScreenPointToRay(cursor), out var hit, 300, ~(1 << 2), QueryTriggerInteraction.Ignore))
            { ClearHover(); return; }
            var surface = hit.collider.GetComponent<MotionSurface>();
            if (surface == null || !surface.Walkable || !Map.Cell(hit.point, out var cell)) { ClearHover(); return; }
            if (Combat && !Busy) SetPreview(cell);
            if (!_input["Move"].WasPressedThisFrame() || Busy) return;
            if (Combat) TryMove(cell);
            else MoveNormally(hit.point);
        }

        public bool MoveNormally(Vector3 point)
        {
            if (!Ready || Combat || Busy) return false;
            if (!Map.Cell(point, out _) || !Map.Support(point, out var target, out _)) return Reject("Недопустимая поверхность");
            var path = new NavMeshPath();
            if (!NavMesh.SamplePosition(target, out var nav, .22f, _agent.areaMask) || Vector3.Distance(nav.position, target) > .3f ||
                !_agent.CalculatePath(nav.position, path) || path.status != NavMeshPathStatus.PathComplete) return Reject("Нет полного пути");
            _agent.isStopped = false; _agent.SetPath(path); Notice = "Движение к " + point.ToString("F1"); return true;
        }

        public bool SetCombat(bool requested)
        {
            if (!Ready) return false;
            if (requested == Combat && !_alignment) return true;
            if (_route != null) { if (!requested) { _exitAfterMove = true; Notice = "Выход после завершения пути"; } return false; }
            if (_alignment) return Reject("Подождите выравнивание по клетке");
            if (!requested)
            {
                Combat = false; Session = null; _agent.enabled = true;
                if (!_agent.Warp(Hero.transform.position)) { Fail("Не удалось вернуть героя на NavMesh"); return false; }
                _agent.ResetPath(); _agent.isStopped = true; _follower.enabled = true; _animation.StopMoving();
                ClearHover(); Notice = "Обычное движение"; return true;
            }
            _follower.Stop();
            if (!Map.NearestReachable(Hero.transform.position, out _alignedCell)) return Reject("Рядом нет безопасного центра клетки");
            _agent.isStopped = false;
            if (!_agent.SetDestination(Map.World(_alignedCell))) return Reject("Не удалось задать путь к центру клетки");
            _alignmentElapsed = 0; _alignment = true;
            Notice = "Выравнивание по клетке"; return true;
        }

        public GridPath SetPreview(GridPosition destination)
        {
            if (!Ready || !Combat || Busy) return GridPath.Unreachable;
            if (Hover == destination && _hoverRevision == Session.Revision) return Preview;
            var timer = System.Diagnostics.Stopwatch.StartNew();
            Preview = Session.Preview(destination); PreviewMilliseconds = timer.Elapsed.TotalMilliseconds;
            Hover = destination; _hoverRevision = Session.Revision;
            Notice = !Preview.Found ? "Нет пути" : Preview.Cost > Session.Actions
                ? $"Нужно {Preview.Cost} ОД; не хватает {Preview.Cost - Session.Actions}"
                : $"Путь: {Preview.Cost} ОД; останется {Session.Actions - Preview.Cost}";
            _view?.Refresh(); return Preview;
        }
        public bool TryMove(GridPosition destination)
        {
            if (!Ready || !Combat || Busy) return false;
            var result = Session.Move("motion-" + ++_command, destination, Session.Revision);
            if (!result.Accepted) return Reject(result.Error);
            Preview = result.Path; Hover = destination;
            _route = result.Path.Cells; _routeIndex = 1; ++AcceptedMoves;
            _animation.Move(); Notice = $"Выполнение пути: {result.Path.Cost} ОД";
            _view.Refresh(); return true;
        }
        private void AdvanceRoute()
        {
            var destination = Map.World(_route[_routeIndex]);
            var current = Hero.transform.position;
            var step = Vector3.MoveTowards(new Vector3(current.x, 0, current.z), new Vector3(destination.x, 0, destination.z), World.Config.Speed * Time.deltaTime);
            if (!Map.Support(step, out var supported, out _)) { Fail("Исполняемый путь потерял опору; выполните Reset"); return; }
            Hero.transform.position = supported;
            var forward = new Vector3(destination.x - current.x, 0, destination.z - current.z);
            if (forward.sqrMagnitude > .0001f) Hero.transform.rotation = Quaternion.RotateTowards(Hero.transform.rotation, Quaternion.LookRotation(forward), 540 * Time.deltaTime);
            if (Vector2.Distance(new Vector2(supported.x, supported.z), new Vector2(destination.x, destination.z)) > .005f) return;
            Hero.transform.position = destination;
            if (++_routeIndex < _route.Count) return;
            _route = null; _animation.StopMoving(); Notice = "Путь завершён"; InvalidatePreview(); _view.Refresh();
        }
        public void NewTurn()
        {
            if (!Ready || !Combat || Busy) return;
            Session.NewTurn(); Notice = "Новый тестовый ход"; InvalidatePreview(); _view.Refresh();
        }
        public void ResetHero()
        {
            if (Hero == null) return;
            _route = null; _alignment = false; _exitAfterMove = false; Combat = false; Session = null; Error = null;
            _agent.enabled = true; _agent.Warp(World.Config.Spawn); _agent.ResetPath(); _agent.isStopped = true;
            _follower.enabled = true; _animation.StopMoving(); _animation.ResetToIdle(); AcceptedMoves = 0;
            ClearHover(); CameraRig.Focus(); Notice = "Герой сброшен";
        }
        private bool Reject(string message)
        {
            Notice = message switch
            {
                "NotEnoughActions" => "Недостаточно очков действий",
                "AlreadyAtDestination" => "Герой уже находится в этой клетке",
                "NoPath" => "Нет пути к выбранной клетке",
                "OutsideBoard" => "Цель вне поля",
                "StalePreview" => "Состояние изменилось — выберите путь заново",
                _ => message
            };
            return false;
        }
        private void Fail(string message) { Error = message; _route = null; _animation?.StopMoving(); }
        private void InvalidatePreview() { _hoverRevision = -1; Preview = GridPath.Unreachable; Hover = null; }
        private void ClearHover() { if (Hover == null) return; InvalidatePreview(); _view?.Refresh(); }
        private void OnDestroy()
        {
            if (_actions != null) { _actions.Disable(); Destroy(_actions); }
            if (Hero != null) Destroy(Hero);
        }
    }
}
