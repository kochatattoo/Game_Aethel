using System;
using System.Collections.Generic;

namespace PetProject.Tactics
{
    /// <summary>Adjacent melee rules. Rejections happen before AP, HP or RNG changes.</summary>
    internal sealed class MeleeAttackResolver
    {
        private readonly GridBoard _board;
        private readonly EncounterRules _rules;
        private readonly EncounterRoster _roster;
        private readonly Func<int> _rollD20;

        public MeleeAttackResolver(GridBoard board, EncounterRules rules, EncounterRoster roster, Func<int> rollD20)
        {
            _board = board;
            _rules = rules;
            _roster = roster;
            _rollD20 = rollD20;
        }

        public CommandError TryAttack(ActorState actor, string targetId, List<TacticalEvent> events)
        {
            if (!_roster.TryGet(targetId, out var target)) return CommandError.UnknownTarget;
            if (!target.IsAlive) return CommandError.TargetDefeated;
            if (target.Definition.FactionId == actor.Definition.FactionId) return CommandError.FriendlyTarget;
            if (actor.Position.DistanceTo(target.Position) != 1 ||
                Math.Abs((long)_board.GetCell(actor.Position).Height - _board.GetCell(target.Position).Height) > _rules.MaxHeightStep)
                return CommandError.TargetOutOfRange;
            if (actor.ActionPoints < _rules.AttackCost) return CommandError.NotEnoughActions;
            var roll = _rollD20();
            var hit = (long)roll + actor.Definition.AttackBonus >= target.Definition.Defense;
            actor.SpendActions(_rules.AttackCost);
            var damage = hit ? target.TakeDamage(actor.Definition.Damage) : 0;
            events.Add(new TacticalEvent(hit ? TacticalEventKind.AttackHit : TacticalEventKind.AttackMiss,
                actor.Definition.Id, targetId, target.Position, damage, roll));
            if (!target.IsAlive) _roster.RecordDefeat(target, actor.Definition.Id, events);
            return CommandError.None;
        }
    }
}
