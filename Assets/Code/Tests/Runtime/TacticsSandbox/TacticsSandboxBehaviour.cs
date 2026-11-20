using System;
using PetProject.Tactics;
using UnityEngine;

namespace CodeBase.TacticsSandbox
{
    /// <summary>Isolated developer prototype. It never resolves the campaign container or writes saves.</summary>
    public sealed class TacticsSandboxBehaviour : MonoBehaviour
    {
        [SerializeField, Min(1)] private int _actionsPerTurn = 3;
        [SerializeField, Min(1)] private int _attackCost = 1;
        [SerializeField, Min(1)] private int _seed = 718;
        [SerializeField] private int _attackBonus = 3;
        [SerializeField] private int _defense = 12;
        [SerializeField, Min(1)] private int _damage = 4;
        [SerializeField, Min(1)] private int _health = 12;

        private Encounter _encounter;
        private TacticsSandboxView _view;
        public EncounterSnapshot Snapshot { get; private set; }
        public CommandResult LastResult { get; private set; }
        public GridBoard Board => _encounter?.Board;
        public bool IsInitialized => _encounter != null;

        private void Awake() => ResetEncounter();

        public void ResetEncounter()
        {
            if (_seed < 1) throw new InvalidOperationException("Prototype seed must be positive");
            var board = new GridBoard(8, 5, new[]
            {
                new GridCell(new GridPosition(3, 2), false),
                new GridCell(new GridPosition(3, 3), false),
                new GridCell(new GridPosition(1, 3), moveCost: 2),
                new GridCell(new GridPosition(2, 3), height: 1)
            });
            var actors = new[]
            {
                new ActorDefinition("hero", "allies", new GridPosition(1, 1), _health, 20, _attackBonus, _defense, _damage),
                new ActorDefinition("enemy", "enemies", new GridPosition(5, 1), _health, 0, _attackBonus, _defense, _damage)
            };
            _encounter = new Encounter(board, actors, new DeterministicRandom((uint)_seed),
                new EncounterRules(_actionsPerTurn, _attackCost));
            _view ??= new TacticsSandboxView(transform, board);
            LastResult = null;
            SyncView();
        }

        public GridPath PreviewMove(GridPosition destination) => Snapshot?.CurrentActorId == null
            ? GridPath.Unreachable : _encounter.PreviewMove(Snapshot.CurrentActorId, destination);

        public CommandResult Execute(TacticalCommand command)
        {
            if (_encounter == null) throw new InvalidOperationException("Sandbox is not initialized");
            LastResult = _encounter.Execute(command);
            if (LastResult.Accepted && !LastResult.IsReplay) SyncView();
            return LastResult;
        }

        public Vector3 ViewPosition(string actorId) => _view.GetActorPosition(actorId);
        public int ViewActorCount => _view?.ActorCount ?? 0;
        public bool ViewActorIsActive(string actorId) => _view.IsActorActive(actorId);

        private void SyncView()
        {
            Snapshot = _encounter.GetSnapshot();
            _view.Sync(Snapshot);
        }

        private void OnDestroy() => _view?.Dispose();
    }
}
