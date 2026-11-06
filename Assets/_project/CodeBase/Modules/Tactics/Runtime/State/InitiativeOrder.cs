using System;
using System.Collections.Generic;
using System.Linq;

namespace PetProject.Tactics
{
    /// <summary>Stable initiative order, AP reset and round advancement; defeated participants are skipped.</summary>
    internal sealed class InitiativeOrder
    {
        private readonly List<ActorState> _actors;
        private readonly int _actionsPerTurn;
        private readonly HashSet<string> _started = new HashSet<string>(StringComparer.Ordinal);
        private readonly HashSet<string> _completedThisRound = new HashSet<string>(StringComparer.Ordinal);
        private int _index;
        public int Round { get; private set; } = 1;
        public ActorState Current => _actors[_index];

        public InitiativeOrder(IEnumerable<ActorState> actors, int actionsPerTurn, Func<int> rollD20,
            string openingActorId = null)
        {
            _actionsPerTurn = actionsPerTurn;
            // Броски назначаются в каноническом порядке ID: перечисление сцены не влияет на RNG.
            _actors = actors.OrderBy(a => a.Definition.Id, StringComparer.Ordinal).ToList();
            foreach (var actor in _actors)
                actor.AssignInitiative((long)rollD20() + actor.Definition.InitiativeBonus);
            _actors = _actors.OrderByDescending(a => a.Initiative)
                .ThenBy(a => a.Definition.Id, StringComparer.Ordinal).ToList();
            if (openingActorId != null)
            {
                _index = _actors.FindIndex(a => a.Definition.Id == openingActorId);
                if (_index < 0) throw new ArgumentException("Opening actor must belong to the roster", nameof(openingActorId));
            }
            BeginTurn();
        }

        public ActorSnapshot[] CaptureActors() => _actors.Select(a => a.Snapshot()).ToArray();

        public void EndTurn(List<TacticalEvent> events)
        {
            Current.EndTurn();
            _completedThisRound.Add(Current.Definition.Id);
            events.Add(new TacticalEvent(TacticalEventKind.TurnEnded, Current.Definition.Id));
            var next = FindNext(false);
            if (next < 0)
            {
                _completedThisRound.Clear();
                ++Round;
                next = FindNext(true);
                if (next < 0) throw new InvalidOperationException("Encounter cannot advance a turn without living actors");
            }
            _index = next;
            BeginTurn();
            events.Add(new TacticalEvent(TacticalEventKind.TurnStarted, Current.Definition.Id, amount: Current.ActionPoints));
        }

        private int FindNext(bool ignoreCompleted)
        {
            for (var step = 1; step <= _actors.Count; ++step)
            {
                var index = (_index + step) % _actors.Count;
                var actor = _actors[index];
                if (actor.IsAlive && (ignoreCompleted || !_completedThisRound.Contains(actor.Definition.Id)))
                    return index;
            }
            return -1;
        }

        private void BeginTurn()
        {
            var isFirstTurn = _started.Add(Current.Definition.Id);
            Current.BeginTurn(isFirstTurn
                ? Current.Definition.InitialActionPoints ?? _actionsPerTurn
                : _actionsPerTurn);
        }
    }
}
