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
        private int _index;
        public int Round { get; private set; } = 1;
        public ActorState Current => _actors[_index];

        public InitiativeOrder(IEnumerable<ActorState> actors, int actionsPerTurn, Func<int> rollD20)
        {
            _actionsPerTurn = actionsPerTurn;
            // Assign rolls in canonical ID order before sorting; scene enumeration must not affect RNG.
            _actors = actors.OrderBy(a => a.Definition.Id, StringComparer.Ordinal).ToList();
            foreach (var actor in _actors)
                actor.AssignInitiative((long)rollD20() + actor.Definition.InitiativeBonus);
            _actors = _actors.OrderByDescending(a => a.Initiative)
                .ThenBy(a => a.Definition.Id, StringComparer.Ordinal).ToList();
            Current.BeginTurn(_actionsPerTurn);
        }

        public ActorSnapshot[] CaptureActors() => _actors.Select(a => a.Snapshot()).ToArray();

        public void EndTurn(List<TacticalEvent> events)
        {
            Current.EndTurn();
            events.Add(new TacticalEvent(TacticalEventKind.TurnEnded, Current.Definition.Id));
            do
            {
                _index = (_index + 1) % _actors.Count;
                if (_index == 0) ++Round;
            } while (!Current.IsAlive);
            Current.BeginTurn(_actionsPerTurn);
            events.Add(new TacticalEvent(TacticalEventKind.TurnStarted, Current.Definition.Id, amount: Current.ActionPoints));
        }
    }
}
