using System;
using System.Collections.Generic;
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
        public int HeroHealth => Snapshot?.GetActor(MotionTestController.HeroActorId).Health ?? HeroMaxHealth;
        public int HeroMaxHealth => _host != null && _host.CombatRules != null ? _host.CombatRules.HeroMaxHealth : 12;
        public int EnemyHealth
        {
            get
            {
                var enemy = _selectedEnemy ?? FirstLivingEnemy();
                if (enemy == null) return 0;
                return Snapshot != null ? Snapshot.GetActor(enemy.Id).Health : EnemyMaxHealth;
            }
        }
        public int EnemyMaxHealth
        {
            get
            {
                var enemy = _selectedEnemy ?? FirstLivingEnemy();
                return enemy?.Archetype != null ? enemy.Archetype.MaxHealth : 5;
            }
        }
        public string SelectedEnemyName => _selectedEnemy?.Archetype != null
            ? _selectedEnemy.Archetype.DisplayName
            : "Скелет";
        public bool IsEnemySelected => _selectedEnemy != null;

        private IMotionEncounterContext _host;
        private State _state = State.Exploration;
        private bool _enabled = true;
        private readonly List<SkeletonEnemyActor> _enemies = new List<SkeletonEnemyActor>();
        private readonly Dictionary<string, SkeletonEnemyActor> _enemiesById =
            new Dictionary<string, SkeletonEnemyActor>();
        private readonly List<SkeletonEnemyActor> _engagedEnemies = new List<SkeletonEnemyActor>();
        private readonly List<GridPosition> _enemyCells = new List<GridPosition>();
        private SkeletonEnemyActor _selectedEnemy;
        private SkeletonEnemyActor _followingEnemy;
        private SkeletonCombatPresenter _presenter;
        private Encounter _encounter;
        private EncounterSnapshot _snapshot;
        private SkeletonTurnPolicy _turnPolicy;
        private GridPosition _heroCell;
        private string _pendingGroupId;
        private string _engagedGroupId;
        private Vector3 _pendingGroupOrigin;
        private bool _enemyThinking;
        private float _alignmentElapsed;
        private int _command;
        private int _presentationVersion;
        private bool _openingAttack;
        private GridPosition? _hover;
        private long _hoverRevision = -1;

        public void Bind(IMotionEncounterContext host)
        {
            _host = host;
            _turnPolicy = new SkeletonTurnPolicy(_host.EnemyAI);
            if (_host.EnemyPrefab != null) SpawnEnemies();
        }

        public void SetEnabled(bool value)
        {
            _enabled = value;
            ResetEncounterState();
            foreach (var enemy in _enemies) enemy.SetEnabled(value);
            if (value && _enemies.Count == 0 && _host.EnemyPrefab != null) SpawnEnemies();
        }

        public void ResetEncounterState()
        {
            ++_presentationVersion;
            ClearTargetSelection();
            Pending = false;
            Active = false;
            PresentationBusy = false;
            _openingAttack = false;
            _pendingGroupId = null;
            _engagedGroupId = null;
            _followingEnemy = null;
            _enemyThinking = false;
            _state = State.Exploration;
            _encounter = null;
            _snapshot = null;
            _hover = null;
            _hoverRevision = -1;
            _engagedEnemies.Clear();
            _enemyCells.Clear();
            foreach (var enemy in _enemies) enemy.Detection.enabled = _enabled && enemy.IsAlive;
        }

        public void ResetCombat()
        {
            ResetEncounterState();
            _host.ResetHero();
            foreach (var enemy in _enemies) enemy.Dispose();
            _enemies.Clear();
            _enemiesById.Clear();
            if (_host.EnemyPrefab != null) SpawnEnemies();
            _host.SetNotice("Тестовый бой сброшен");
        }

        public void ClearTargetSelection()
        {
            if (_selectedEnemy == null) return;
            _selectedEnemy.SetSelected(false);
            _selectedEnemy = null;
            _host.RefreshTacticalView();
        }

        private void SelectEnemy(SkeletonEnemyActor enemy)
        {
            if (_selectedEnemy == enemy) return;
            _selectedEnemy?.SetSelected(false);
            _selectedEnemy = enemy;
            _selectedEnemy.SetSelected(true);
            _host.RefreshTacticalView();
        }

        public bool HandleExplorationClick(Collider collider)
        {
            if (!_enabled || Active || Pending || _host.Combat || _enemies.Count == 0) return false;
            var target = collider.GetComponentInParent<MotionEnemyTarget>();
            if (target == null || !_enemiesById.TryGetValue(target.ActorId, out var enemy) || !enemy.IsAlive)
                return false;
            SelectEnemy(enemy);
            if (AreAdjacent(enemy))
            {
                RequestOpeningAttack();
                return true;
            }
            if (TryMoveToSkeleton(enemy))
            {
                _followingEnemy = enemy;
                _state = State.Following;
                _host.SetNotice("Следование к скелету");
            }
            else
            {
                _host.SetNotice("Нет полного пути к свободной соседней клетке скелета");
            }
            return true;
        }

        private bool TryMoveToSkeleton(SkeletonEnemyActor enemy)
        {
            if (!_host.Map.Cell(enemy.Instance.transform.position, out var enemyCell)) return false;
            var candidates = new List<GridPosition>(GridAdjacency.GetNeighbors(enemyCell)).ToArray();
            var heroPosition = _host.Hero.transform.position;
            var attempted = new bool[candidates.Length];
            for (var attempt = 0; attempt < candidates.Length; ++attempt)
            {
                var nearest = -1;
                var nearestDistance = float.MaxValue;
                for (var i = 0; i < candidates.Length; ++i)
                {
                    if (attempted[i] || !_host.Map.Board.Contains(candidates[i]) ||
                        !_host.Map.Board.GetCell(candidates[i]).Walkable) continue;
                    var distance = (_host.Map.World(candidates[i]) - heroPosition).sqrMagnitude;
                    if (distance >= nearestDistance) continue;
                    nearest = i;
                    nearestDistance = distance;
                }
                if (nearest < 0) return false;
                attempted[nearest] = true;
                if (_host.MoveNormally(_host.Map.World(candidates[nearest]))) return true;
            }
            return false;
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

        public bool SelectTarget(string actorId)
        {
            if (!_enemiesById.TryGetValue(actorId, out var enemy) || !enemy.IsAlive) return false;
            SelectEnemy(enemy);
            return true;
        }

        public void TryHeroAttack()
        {
            if (!PlayerTurnReady || _selectedEnemy == null) return;
            Execute(TacticalCommand.Attack(NextId(), MotionTestController.HeroActorId, _selectedEnemy.Id));
        }

        private void Update()
        {
            if (!_enabled || _host == null || !_host.Ready) return;
            if (_state == State.Following && !Pending && _followingEnemy != null && AreAdjacent(_followingEnemy))
                RequestOpeningAttack();
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
            if (current.Id == MotionTestController.HeroActorId) HandlePlayerInput();
            else RunEnemyTurn();
        }

        private static bool AreAdjacent(IMotionEncounterContext host, SkeletonEnemyActor enemy) =>
            host.Map.Cell(host.Hero.transform.position, out var heroCell) &&
            host.Map.Cell(enemy.Instance.transform.position, out var enemyCell) &&
            GridAdjacency.IsAdjacent(heroCell, enemyCell);

        private bool AreAdjacent(SkeletonEnemyActor enemy) => AreAdjacent(_host, enemy);

        private void RequestOpeningAttack()
        {
            var budget = ActionsPerTurn;
            if (_host.WorldActions.Get(MotionTestController.HeroActorId, budget) < AttackCost)
            {
                _state = State.Exploration;
                _host.SetNotice($"Для внезапной атаки требуется {AttackCost} ОД");
                return;
            }
            _pendingGroupId = _selectedEnemy.GroupId;
            _pendingGroupOrigin = _selectedEnemy.Instance.transform.position;
            _openingAttack = true;
            Pending = true;
            _host.SetNotice("Внезапная атака — группа скелетов вступает в бой");
        }

        private void BeginCombat()
        {
            Pending = false;
            if (!EncounterPlacement.FindCell(_host.Map, _host.World.Config, _host.Hero.transform.position,
                    null, out _heroCell))
            {
                AbortPlacement("Не удалось выбрать клетку героя для боя");
                return;
            }

            var groupId = _pendingGroupId ?? _selectedEnemy?.GroupId;
            _engagedGroupId = groupId;
            _engagedEnemies.Clear();
            foreach (var enemy in _enemies)
            {
                if (!enemy.IsAlive || enemy.GroupId != groupId) continue;
                var distance = Vector3.Distance(_pendingGroupOrigin, enemy.Instance.transform.position);
                if (distance <= GroupLinkRadius) _engagedEnemies.Add(enemy);
            }
            if (_engagedEnemies.Count == 0)
            {
                AbortPlacement("В группе нет живых противников");
                return;
            }

            var occupied = new HashSet<GridPosition> { _heroCell };
            _enemyCells.Clear();
            foreach (var enemy in _engagedEnemies)
            {
                if (!EncounterPlacement.FindCell(_host.Map, _host.World.Config, enemy.Instance.transform.position,
                        occupied, out var enemyCell))
                {
                    AbortPlacement("Не удалось выбрать разные клетки для группы");
                    return;
                }
                occupied.Add(enemyCell);
                _enemyCells.Add(enemyCell);
            }

            _host.PrepareForEncounter();
            var rules = _host.CombatRules;
            var actors = new List<ActorDefinition>
            {
                new ActorDefinition(MotionTestController.HeroActorId, "allies", _heroCell,
                    rules != null ? rules.HeroMaxHealth : 12,
                    rules != null ? rules.HeroInitiativeBonus : 0,
                    rules != null ? rules.HeroAttackBonus : 3,
                    rules != null ? rules.HeroDefense : 12,
                    rules != null ? rules.HeroDamage : 4,
                    _host.WorldActions.Get(MotionTestController.HeroActorId, ActionsPerTurn))
            };
            for (var i = 0; i < _engagedEnemies.Count; ++i)
            {
                var enemy = _engagedEnemies[i];
                var archetype = enemy.Archetype;
                actors.Add(new ActorDefinition(enemy.Id, "enemies", _enemyCells[i],
                    archetype != null ? archetype.MaxHealth : 5,
                    archetype != null ? archetype.InitiativeBonus : 0,
                    archetype != null ? archetype.AttackBonus : 2,
                    archetype != null ? archetype.Defense : 12,
                    4,
                    _host.WorldActions.Get(enemy.Id, ActionsPerTurn),
                    archetype != null ? archetype.AttackDiceSides : 20,
                    archetype != null ? archetype.DamageDiceCount : 1,
                    archetype != null ? archetype.DamageDiceSides : 4,
                    archetype != null ? archetype.DamageBonus : 0));
            }
            _encounter = new Encounter(_host.Map.Board, actors, new DeterministicRandom(718),
                new EncounterRules(ActionsPerTurn, AttackCost, _host.World.Config.MaxHeightStep),
                _openingAttack ? MotionTestController.HeroActorId : null);
            _snapshot = null;
            Active = true;
            _state = State.Aligning;
            _alignmentElapsed = 0;
            if (!_host.HeroAgent.enabled) _host.HeroAgent.enabled = true;
            foreach (var enemy in _engagedEnemies) enemy.Agent.enabled = true;
            if (_openingAttack)
            {
                CompleteAlignment();
                return;
            }
            if (!_host.HeroAgent.SetDestination(_host.Map.World(_heroCell)))
            {
                AbortAlignment("Не удалось начать выравнивание героя для боя");
                return;
            }
            for (var i = 0; i < _engagedEnemies.Count; ++i)
                if (!_engagedEnemies[i].Agent.SetDestination(_host.Map.World(_enemyCells[i])))
                {
                    AbortAlignment("Не удалось начать выравнивание группы для боя");
                    return;
                }
            _host.SetNotice($"Скелеты группы {groupId} заметили героя — выравнивание для боя");
        }

        private void UpdateAlignment()
        {
            _alignmentElapsed += Time.deltaTime;
            if (_alignmentElapsed > 6)
            {
                AbortAlignment("Выравнивание для боя не удалось");
                return;
            }
            foreach (var enemy in _engagedEnemies)
            {
                if (enemy.Agent.velocity.magnitude > .1f) enemy.Animator.Move(enemy.Agent.velocity.magnitude);
                else enemy.Animator.StopMoving();
            }
            if (!EncounterPlacement.HasArrived(_host.HeroAgent, _host.Map.World(_heroCell))) return;
            for (var i = 0; i < _engagedEnemies.Count; ++i)
                if (!EncounterPlacement.HasArrived(_engagedEnemies[i].Agent, _host.Map.World(_enemyCells[i])))
                    return;
            CompleteAlignment();
        }

        private void CompleteAlignment()
        {
            _host.HeroAgent.Warp(_host.Map.World(_heroCell));
            for (var i = 0; i < _engagedEnemies.Count; ++i)
                _engagedEnemies[i].Agent.Warp(_host.Map.World(_enemyCells[i]));
            _host.FinishEncounterAlignment();
            foreach (var enemy in _engagedEnemies)
            {
                enemy.Agent.enabled = false;
                enemy.Animator.StopMoving();
            }
            _state = State.Combat;
            _host.SetNotice($"Бой начался: противников {_engagedEnemies.Count}");
            _host.AddHistory($"Группа {_engagedGroupId}: в бою скелетов {_engagedEnemies.Count}");
            _host.RefreshTacticalView();
            if (_openingAttack)
            {
                _openingAttack = false;
                TryHeroAttack();
            }
        }

        private void AbortPlacement(string message)
        {
            _openingAttack = false;
            _pendingGroupId = null;
            _state = State.Exploration;
            _engagedEnemies.Clear();
            _enemyCells.Clear();
            foreach (var enemy in _enemies) enemy.Detection.enabled = enemy.IsAlive;
            _host.SetNotice(message);
        }

        private void AbortAlignment(string message)
        {
            _encounter = null;
            _snapshot = null;
            _openingAttack = false;
            _pendingGroupId = null;
            Active = false;
            _state = State.Exploration;
            _host.RestoreExploration();
            foreach (var enemy in _engagedEnemies)
            {
                enemy.Agent.enabled = true;
                enemy.Detection.enabled = enemy.IsAlive;
            }
            _engagedEnemies.Clear();
            _enemyCells.Clear();
            _host.SetNotice(message);
        }

        private void HandlePlayerInput()
        {
            if (!_host.PointerInputAllowed || _host.PointerOverUi || _host.CameraRig.Orbiting) return;
            if (!_host.MotionInput.TryRaycast(_host.GameCamera, out var hit)) return;
            var target = hit.collider.GetComponentInParent<MotionEnemyTarget>();
            if (_host.MovePressed && target != null && _enemiesById.TryGetValue(target.ActorId, out var enemy) &&
                _engagedEnemies.Contains(enemy))
            {
                SelectEnemy(enemy);
                Execute(TacticalCommand.Attack(NextId(), MotionTestController.HeroActorId, enemy.Id));
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
            {
                ClearTargetSelection();
                Execute(TacticalCommand.Move(NextId(), MotionTestController.HeroActorId, cell), _host.Preview);
            }
        }

        private void RunEnemyTurn()
        {
            if (_enemyThinking) return;
            _enemyThinking = true;
            var actorId = Snapshot.CurrentActorId;
            _host.SetNotice($"{ActorName(actorId)} обдумывает действие");
            _host.AddHistory($"{ActorName(actorId)}: пауза решения {EnemyDecisionDelay:0.##} с");
            ThinkAndExecuteAsync(_presentationVersion, actorId).Forget(Debug.LogException);
        }

        private async UniTask ThinkAndExecuteAsync(int version, string actorId)
        {
            try
            {
                await UniTask.Delay(TimeSpan.FromSeconds(EnemyDecisionDelay));
                if (version != _presentationVersion || _state != State.Combat || _encounter == null) return;
                if (Snapshot.CurrentActorId != actorId) return;
                _enemyThinking = false;
                Execute(_turnPolicy.CreateCommand(_encounter, Snapshot, NextId,
                    _enemiesById[actorId].Behavior));
            }
            finally
            {
                if (version == _presentationVersion) _enemyThinking = false;
            }
        }

        private void Execute(TacticalCommand command, GridPath path = null)
        {
            if (PresentationBusy || _encounter == null) return;
            if (command.Kind == TacticalCommandKind.Move && path == null)
                path = _encounter.PreviewMove(command.ActorId, command.Destination);
            var before = _encounter.GetSnapshot();
            var result = _encounter.Execute(command);
            if (!result.Accepted)
            {
                var error = ErrorText(result.Error);
                _host.SetNotice(error);
                _host.AddHistory("Отказ: " + error);
                return;
            }
            if (result.IsReplay) return;
            LogCommand(result, before);
            UpdateHealthBars();
            PresentationBusy = true;
            _host.RefreshTacticalView();
            RunPresentationAsync(command, result, path).Forget(Debug.LogException);
        }

        private void UpdateHealthBars()
        {
            if (Snapshot == null) return;
            foreach (var enemy in _engagedEnemies)
            {
                var actor = Snapshot.GetActor(enemy.Id);
                enemy.HealthBar.SetHealth(actor.Health, actor.Definition.MaxHealth);
            }
        }

        private void LogCommand(CommandResult result, EncounterSnapshot before)
        {
            var after = _encounter.GetSnapshot();
            foreach (var combatEvent in result.Events)
            {
                if (combatEvent.Kind == TacticalEventKind.Moved)
                {
                    _host.AddHistory($"{ActorName(combatEvent.ActorId)}: клетка {combatEvent.Position}, " +
                        $"-{combatEvent.Amount} ОД");
                }
                else if (combatEvent.Kind == TacticalEventKind.AttackHit ||
                    combatEvent.Kind == TacticalEventKind.AttackMiss)
                {
                    LogAttack(combatEvent, before, after);
                }
                else if (combatEvent.Kind == TacticalEventKind.ActorDefeated)
                {
                    _host.AddHistory($"{ActorName(combatEvent.ActorId)}: побеждён");
                }
                else if (combatEvent.Kind == TacticalEventKind.TurnEnded)
                {
                    _host.AddHistory($"{ActorName(combatEvent.ActorId)}: ход завершён");
                }
                else if (combatEvent.Kind == TacticalEventKind.TurnStarted)
                {
                    _host.AddHistory($"Раунд {after.Round}: ход {ActorName(combatEvent.ActorId)}, " +
                        $"{combatEvent.Amount} ОД");
                }
            }
        }

        private void LogAttack(TacticalEvent attack, EncounterSnapshot before, EncounterSnapshot after)
        {
            var attacker = before.GetActor(attack.ActorId);
            var targetBefore = before.GetActor(attack.TargetId);
            var targetAfter = after.GetActor(attack.TargetId);
            var definition = attacker.Definition;
            var attackTotal = attack.Roll + definition.AttackBonus;
            var hit = attack.Kind == TacticalEventKind.AttackHit;
            var message = $"{ActorName(attacker.Id)} → {ActorName(targetBefore.Id)}: " +
                $"d{definition.AttackDiceSides} {attack.Roll} {Signed(definition.AttackBonus)} = {attackTotal}; " +
                $"нужно > {targetBefore.Definition.Defense} — {(hit ? "попадание" : "промах")}";
            if (hit)
            {
                message += definition.UsesDamageDice
                    ? $"; {definition.DamageDiceCount}d{definition.DamageDiceSides} = {attack.DamageRoll} " +
                        $"{Signed(definition.DamageBonus)}, урон {attack.Amount}"
                    : $"; фиксированный урон {attack.Amount}";
                message += $"; HP {targetBefore.Health} → {targetAfter.Health}";
            }
            _host.AddHistory(message);
        }

        private static string Signed(int value) => value >= 0 ? "+" + value : value.ToString();

        private string ActorName(string actorId)
        {
            if (actorId == MotionTestController.HeroActorId) return "Герой";
            if (_enemiesById.TryGetValue(actorId, out var enemy) && enemy.Archetype != null)
                return enemy.Archetype.DisplayName;
            if (actorId == MotionEnemyTarget.SkeletonActorId) return "Скелет 1";
            if (actorId != null && actorId.StartsWith("skeleton-"))
                return "Скелет " + actorId.Substring("skeleton-".Length);
            return actorId;
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
                    : $"Ход: {snapshot.CurrentActorId}");
            }
            finally
            {
                if (version == _presentationVersion) PresentationBusy = false;
            }
        }

        private void FinishVictory(EncounterSnapshot snapshot)
        {
            var hero = snapshot.GetActor(MotionTestController.HeroActorId);
            _host.WorldActions.Set(MotionTestController.HeroActorId, hero.ActionPoints, ActionsPerTurn);
            ClearTargetSelection();
            Active = false;
            _state = State.Exploration;
            _encounter = null;
            _snapshot = null;
            _engagedEnemies.Clear();
            _enemyCells.Clear();
            _host.RestoreExploration();
            _host.AddHistory("Победа: группа противников уничтожена");
            _host.SetNotice("Победа — обычное движение восстановлено");
        }

        private void FinishDefeat()
        {
            _state = State.Defeat;
            _host.AddHistory("Поражение: герой побеждён");
            _host.SetNotice("Поражение — нажмите «Сбросить бой»");
        }

        private void SpawnEnemies()
        {
            if (_host.EnemySpawns == null) return;
            for (var i = 0; i < _host.EnemySpawns.Count; ++i)
            {
                var spawn = _host.EnemySpawns[i];
                if (spawn == null) continue;
                var id = i == 0 ? MotionEnemyTarget.SkeletonActorId : "skeleton-" + (i + 1);
                var enemy = SkeletonEnemyActor.Create(_host.EnemyPrefab, _host.EnemyAI, spawn, id,
                    _host.HeroController, _host.World.Config, _host.GameCamera, transform, () =>
                    {
                        if (!_enabled || Active || Pending) return;
                        _pendingGroupId = spawn.GroupId;
                        _pendingGroupOrigin = spawn.Position;
                        Pending = true;
                    });
                _enemies.Add(enemy);
                _enemiesById.Add(id, enemy);
                enemy.SetEnabled(_enabled);
            }
            _presenter = new SkeletonCombatPresenter(_host, _enemiesById);
        }

        private SkeletonEnemyActor FirstLivingEnemy()
        {
            foreach (var enemy in _enemies)
                if (enemy.IsAlive) return enemy;
            return null;
        }

        private int ActionsPerTurn => _host.CombatRules != null
            ? _host.CombatRules.ActionsPerTurn
            : Mathf.Max(1, _host.TestBudget);
        private int AttackCost => _host.CombatRules != null ? _host.CombatRules.AttackCost : 3;
        private float GroupLinkRadius => _host.CombatRules != null ? _host.CombatRules.GroupLinkRadius : 12;
        private float EnemyDecisionDelay => _host.CombatRules != null
            ? Mathf.Max(.5f, _host.CombatRules.EnemyDecisionDelay)
            : .75f;

        private string NextId() => "skeleton-combat-" + ++_command;

        private static string ErrorText(CommandError error) => error switch
        {
            CommandError.TargetOutOfRange => "Цель не находится в соседней клетке",
            CommandError.NotEnoughActions => "Недостаточно очков действий",
            CommandError.NoPath => "Нет пути к выбранной клетке",
            CommandError.WrongTurn => "Сейчас ход другого участника",
            CommandError.AlreadyAtDestination => "Участник уже находится в этой клетке",
            CommandError.EncounterCompleted => "Бой уже завершён",
            _ => error.ToString()
        };
    }
}
