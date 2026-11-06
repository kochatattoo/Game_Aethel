using System;
using System.Collections.Generic;
using System.Linq;

namespace PetProject.Tactics
{
    /// <summary>Owns participants, spawn invariants, living occupancy and victory state.</summary>
    internal sealed class EncounterRoster
    {
        private readonly Dictionary<string, ActorState> _actors = new Dictionary<string, ActorState>(StringComparer.Ordinal);
        public IEnumerable<ActorState> Actors => _actors.Values;
        public string WinningFactionId { get; private set; }

        public EncounterRoster(GridBoard board, IEnumerable<ActorDefinition> actors)
        {
            if (actors == null) throw new ArgumentNullException(nameof(actors));
            var occupied = new HashSet<GridPosition>();
            var factions = new HashSet<string>(StringComparer.Ordinal);
            foreach (var definition in actors)
            {
                if (definition == null || _actors.ContainsKey(definition.Id) || !board.Contains(definition.Position) ||
                    !board.GetCell(definition.Position).Walkable || !occupied.Add(definition.Position))
                    throw new ArgumentException("Actors need unique IDs and distinct walkable spawn cells", nameof(actors));
                _actors.Add(definition.Id, new ActorState(definition));
                factions.Add(definition.FactionId);
            }
            if (factions.Count < 2) throw new ArgumentException("An encounter needs at least two factions", nameof(actors));
        }

        public bool TryGet(string id, out ActorState actor) => _actors.TryGetValue(id, out actor);
        public IEnumerable<GridPosition> OccupiedCells() => _actors.Values.Where(a => a.IsAlive).Select(a => a.Position);

        public void RecordDefeat(ActorState target, string attackerId, List<TacticalEvent> events)
        {
            target.EndTurn();
            events.Add(new TacticalEvent(TacticalEventKind.ActorDefeated, target.Definition.Id));
            var aliveFactions = _actors.Values.Where(a => a.IsAlive).Select(a => a.Definition.FactionId)
                .Distinct(StringComparer.Ordinal).ToArray();
            if (aliveFactions.Length != 1) return;
            WinningFactionId = aliveFactions[0];
            events.Add(new TacticalEvent(TacticalEventKind.EncounterCompleted, attackerId));
        }
    }
}
