using System;

namespace PetProject.Tactics
{
    /// <summary>Prototype rules, not a complete D&D implementation. Basic attacks are adjacent only.</summary>
    public sealed class EncounterRules
    {
        public int ActionsPerTurn { get; }
        public int AttackCost { get; }
        public int MaxHeightStep { get; }

        public EncounterRules(int actionsPerTurn = 3, int attackCost = 1, int maxHeightStep = 1)
        {
            if (actionsPerTurn < 1) throw new ArgumentOutOfRangeException(nameof(actionsPerTurn));
            if (attackCost < 1 || attackCost > actionsPerTurn) throw new ArgumentOutOfRangeException(nameof(attackCost));
            if (maxHeightStep < 0) throw new ArgumentOutOfRangeException(nameof(maxHeightStep));
            ActionsPerTurn = actionsPerTurn;
            AttackCost = attackCost;
            MaxHeightStep = maxHeightStep;
        }
    }
}
