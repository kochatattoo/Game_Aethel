using System;
using System.Collections.Generic;

namespace PetProject.Tactics
{
    public sealed class WorldActionBudgets
    {
        private readonly Dictionary<string, int> _remaining = new Dictionary<string, int>(StringComparer.Ordinal);

        public int Get(string actorId, int budget)
        {
            Validate(actorId, budget);
            return _remaining.TryGetValue(actorId, out var actions) ? actions : budget;
        }

        public void Set(string actorId, int actions, int budget)
        {
            Validate(actorId, budget);
            if (actions < 0 || actions > budget) throw new ArgumentOutOfRangeException(nameof(actions));
            _remaining[actorId] = actions;
        }

        public void ResetAll() => _remaining.Clear();

        private static void Validate(string actorId, int budget)
        {
            if (string.IsNullOrWhiteSpace(actorId)) throw new ArgumentException("Actor ID is required", nameof(actorId));
            if (budget < 1) throw new ArgumentOutOfRangeException(nameof(budget));
        }
    }
}
