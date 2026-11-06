using Cysharp.Threading.Tasks;
using PetProject.Tactics;
using UnityEngine;

namespace CodeBase.MotionTest
{
    public sealed class SkeletonCombatDirector : MonoBehaviour
    {
        private enum State { Exploration, Following, Aligning, Combat, Defeat }

        public bool Active { get; private set; }
        public bool Pending { get; private set; }
        public bool PresentationBusy { get; private set; }
        public bool IsDefeated => _state == State.Defeat;
        public bool CanReset => IsDefeated;
        public bool PlayerTurnReady => _state == State.Combat && !PresentationBusy &&
            Snapshot?.CurrentActorId == MotionTestController.HeroActorId;
        public EncounterSnapshot Snapshot
        {
            get
            {
                if (_encounter == null) return null;
                if (_snapshot == null || _snapshot.Revision != _encounter.Revision)
                    _snapshot = _encounter.GetSnapshot();
                return _snapshot;
            }
        }
        public long Revision => Snapshot?.Revision ?? -1;
        public GridPosition HeroPosition =>
            Snapshot?.GetActor(MotionTestController.HeroActorId).Position ?? default;
        public int HeroActions =>
            Snapshot?.GetActor(MotionTestController.HeroActorId).ActionPoints ?? 0;

        private IMotionEncounterContext _host;
        private State _state = State.Exploration;
        private bool _enabled = true;
        private SkeletonEnemyActor _enemy;
        private SkeletonCombatPresenter _presenter;
        private Encounter _encounter;
        private EncounterSnapshot _snapshot;
        private GridPosition _heroCell;
        private GridPosition _enemyCell;
        private float _alignmentElapsed;
        private int _command;
        private int _presentationVersion;
        private GridPosition? _hover;
        private long _hoverRevision = -1;

        public void Bind(IMotionEncounterContext host)
        {
            _host = host;
            if (_host.EnemyPrefab != null) SpawnEnemy();
        }

        public void SetEnabled(bool value)
        {
            _enabled = value;
            ResetEncounterState();
            _enemy?.SetEnabled(value);
            if (value && _enemy == null && _host.EnemyPrefab != null) SpawnEnemy();
        }

        public void ResetEncounterState()
        {
            ++_presentationVersion;
            Pending = false;
            Active = false;
            PresentationBusy = false;
            _state = State.Exploration;
            _encounter = null;
            _snapshot = null;
            _hover = null;
            _hoverRevision = -1;
            if (_enemy != null) _enemy.Detection.enabled = _enabled;
        }

        public void ResetCombat()
        {
            ResetEncounterState();
            _host.ResetHero();
            if (_enemy != null) _enemy.Dispose();
            _enemy = null;
            if (_host.EnemyPrefab != null) SpawnEnemy();
            _host.SetNotice("Тестовый бой сброшен");
        }

        public bool HandleExplorationClick(Collider collider)
        {
            if (!_enabled || Active || Pending || _host.Combat || _enemy == null) return false;
            if (collider.GetComponentInParent<MotionEnemyTarget>() == null) return false;
            var offset = _host.Hero.transform.position - _enemy.Instance.transform.position;
            offset.y = 0;
            var destination = _enemy.Instance.transform.position - offset.normalized * 1.25f;
            if (_host.MoveNormally(destination))
            {
                _state = State.Following;
                _host.SetNotice("Следование к скелету");
            }
            return true;
        }

        public void EndTurnFromHud()
        {
            if (!Active || _state != State.Combat || PresentationBusy || _encounter == null) return;
            var snapshot = _encounter.GetSnapshot();
            if (snapshot.CurrentActorId != MotionTestController.HeroActorId) return;
            var hero = snapshot.GetActor(MotionTestController.HeroActorId);
            if (hero.ActionPoints > 0) Execute(TacticalCommand.EndTurn(NextId(), MotionTestController.HeroActorId));
        }

        public GridPath PreviewHeroMove(GridPosition destination) =>
            _encounter?.PreviewMove(MotionTestController.HeroActorId, destination) ?? GridPath.Unreachable;

        public void TryHeroMove(GridPosition destination)
        {
            if (!PlayerTurnReady) return;
            Execute(TacticalCommand.Move(NextId(), MotionTestController.HeroActorId, destination),
                PreviewHeroMove(destination));
        }

        public void TryHeroAttack()
        {
            if (!PlayerTurnReady) return;
            Execute(TacticalCommand.Attack(NextId(), MotionTestController.HeroActorId,
                MotionEnemyTarget.SkeletonActorId));
        }

        private void Update()
        {
            if (!_enabled || _host == null || !_host.Ready) return;
            if (Pending && !Active)
            {
                if (_host.Busy) return;
                BeginCombat();
            }
            if (_state == State.Aligning) UpdateAlignment();
            if (_state != State.Combat || PresentationBusy || _encounter == null) return;
            var snapshot = Snapshot;
            if (snapshot.IsCompleted || snapshot.CurrentActorId == null) return;
            var current = snapshot.GetActor(snapshot.CurrentActorId);
            if (current.ActionPoints == 0)
            {
                Execute(TacticalCommand.EndTurn(NextId(), current.Id));
                return;
            }
            if (current.Id == MotionEnemyTarget.SkeletonActorId) RunEnemyTurn();
            else HandlePlayerInput();
        }

        private void BeginCombat()
        {
            Pending = false;
            if (!EncounterPlacement.FindCell(_host.Map, _host.World.Config, _host.Hero.transform.position, null, out _heroCell) ||
                !EncounterPlacement.FindCell(_host.Map, _host.World.Config, _enemy.Instance.transform.position, _heroCell, out _enemyCell))
            {
                _host.SetNotice("Не удалось выбрать разные клетки для боя");
                _enemy.Detection.enabled = true;
                return;
            }

            _host.PrepareForEncounter();
            var budget = Mathf.Max(1, _host.TestBudget);
            var actors = new[]
            {
                new ActorDefinition(MotionTestController.HeroActorId, "allies", _heroCell, 12, 0, 3, 12, 4,
                    _host.WorldActions.Get(MotionTestController.HeroActorId, budget)),
                new ActorDefinition(MotionEnemyTarget.SkeletonActorId, "enemies", _enemyCell, 5, 0, 3, 12, 5,
                    _host.WorldActions.Get(MotionEnemyTarget.SkeletonActorId, budget))
            };
            _encounter = new Encounter(_host.Map.Board, actors, new DeterministicRandom(718),
                new EncounterRules(budget, 1, _host.World.Config.MaxHeightStep));
            _snapshot = null;
            Active = true;
            _state = State.Aligning;
            _alignmentElapsed = 0;
            if (!_host.HeroAgent.enabled) _host.HeroAgent.enabled = true;
            _enemy.Agent.enabled = true;
            if (!_host.HeroAgent.SetDestination(_host.Map.World(_heroCell)) ||
                !_enemy.Agent.SetDestination(_host.Map.World(_enemyCell)))
            {
                AbortAlignment("Не удалось начать выравнивание для боя");
                return;
            }
            _host.SetNotice("Скелет заметил героя — выравнивание для боя");
        }

        private void UpdateAlignment()
        {
            _alignmentElapsed += Time.deltaTime;
            if (_alignmentElapsed > 6)
            {
                AbortAlignment("Выравнивание для боя не удалось");
                return;
            }
            if (_enemy.Agent.velocity.magnitude > .1f) _enemy.Animator.Move(_enemy.Agent.velocity.magnitude);
            else _enemy.Animator.StopMoving();
            if (!EncounterPlacement.HasArrived(_host.HeroAgent, _host.Map.World(_heroCell)) ||
                !EncounterPlacement.HasArrived(_enemy.Agent, _host.Map.World(_enemyCell))) return;
            _host.HeroAgent.Warp(_host.Map.World(_heroCell));
            _enemy.Agent.Warp(_host.Map.World(_enemyCell));
            _host.FinishEncounterAlignment();
            _enemy.Agent.enabled = false;
            _enemy.Animator.StopMoving();
            _state = State.Combat;
            _host.SetNotice("Бой начался");
            _host.RefreshTacticalView();
        }

        private void AbortAlignment(string message)
        {
            _encounter = null;
            _snapshot = null;
            Active = false;
            _state = State.Exploration;
            _host.RestoreExploration();
            _enemy.Agent.enabled = true;
            _enemy.Detection.enabled = true;
            _host.SetNotice(message);
        }

        private void HandlePlayerInput()
        {
            if (!_host.PointerInputAllowed || _host.PointerOverUi || _host.CameraRig.Orbiting) return;
            if (!_host.MotionInput.TryRaycast(_host.GameCamera, out var hit)) return;
            if (_host.MovePressed && hit.collider.GetComponentInParent<MotionEnemyTarget>() != null)
            {
                Execute(TacticalCommand.Attack(NextId(), MotionTestController.HeroActorId,
                    MotionEnemyTarget.SkeletonActorId));
                return;
            }
            var surface = hit.collider.GetComponent<MotionSurface>();
            if (surface == null || !surface.Walkable || !_host.Map.Cell(hit.point, out var cell)) return;
            if (_hover != cell || _hoverRevision != Snapshot.Revision)
            {
                _hover = cell;
                _hoverRevision = Snapshot.Revision;
                _host.SetEncounterPreview(_encounter.PreviewMove(MotionTestController.HeroActorId, cell), cell);
            }
            if (_host.MovePressed)
                Execute(TacticalCommand.Move(NextId(), MotionTestController.HeroActorId, cell), _host.Preview);
        }

        private void RunEnemyTurn() =>
            Execute(SkeletonTurnPolicy.CreateCommand(_encounter, Snapshot, NextId));

        private void Execute(TacticalCommand command, GridPath path = null)
        {
            if (PresentationBusy || _encounter == null) return;
            if (command.Kind == TacticalCommandKind.Move && path == null)
                path = _encounter.PreviewMove(command.ActorId, command.Destination);
            var result = _encounter.Execute(command);
            if (!result.Accepted)
            {
                _host.SetNotice(ErrorText(result.Error));
                return;
            }
            if (result.IsReplay) return;
            PresentationBusy = true;
            RunPresentationAsync(command, result, path).Forget(Debug.LogException);
        }

        private async UniTask RunPresentationAsync(TacticalCommand command, CommandResult result, GridPath path)
        {
            var version = _presentationVersion;
            try
            {
                await _presenter.PlayAsync(command, result, path, version, () => _presentationVersion);
                if (version != _presentationVersion) return;
                PresentationBusy = false;
                _hover = null;
                _hoverRevision = -1;
                _host.ClearPreview();
                var snapshot = _encounter.GetSnapshot();
                if (snapshot.IsCompleted)
                {
                    if (snapshot.WinningFactionId == "allies") FinishVictory(snapshot);
                    else FinishDefeat();
                    return;
                }
                _host.SetNotice(snapshot.CurrentActorId == MotionTestController.HeroActorId
                    ? "Ход героя"
                    : "Ход скелета");
            }
            finally
            {
                if (version == _presentationVersion) PresentationBusy = false;
            }
        }

        private void FinishVictory(EncounterSnapshot snapshot)
        {
            var hero = snapshot.GetActor(MotionTestController.HeroActorId);
            var budget = Mathf.Max(1, _host.TestBudget);
            _host.WorldActions.Set(MotionTestController.HeroActorId, hero.ActionPoints, budget);
            Active = false;
            _state = State.Exploration;
            _encounter = null;
            _snapshot = null;
            _host.RestoreExploration();
            _host.SetNotice("Победа — обычное движение восстановлено");
        }

        private void FinishDefeat()
        {
            _state = State.Defeat;
            _host.SetNotice("Поражение — нажмите «Сбросить бой»");
        }

        private void SpawnEnemy()
        {
            _enemy = SkeletonEnemyActor.Create(_host.EnemyPrefab, _host.EnemySpawn, _host.HeroController,
                _host.World.Config, transform, () =>
                {
                    if (_enabled && !Active) Pending = true;
                });
            _enemy.SetEnabled(_enabled);
            _presenter = new SkeletonCombatPresenter(_host, _enemy);
        }

        private string NextId() => "skeleton-combat-" + ++_command;

        private static string ErrorText(CommandError error) => error switch
        {
            CommandError.TargetOutOfRange => "Скелет не находится в соседней клетке",
            CommandError.NotEnoughActions => "Недостаточно очков действий",
            CommandError.NoPath => "Нет пути к выбранной клетке",
            CommandError.WrongTurn => "Сейчас ход другого участника",
            CommandError.AlreadyAtDestination => "Участник уже находится в этой клетке",
            CommandError.EncounterCompleted => "Бой уже завершён",
            _ => error.ToString()
        };
    }
}
