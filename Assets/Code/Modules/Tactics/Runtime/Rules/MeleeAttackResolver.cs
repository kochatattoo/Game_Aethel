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
        private readonly Func<int, int, int> _rollDice;

        public MeleeAttackResolver(GridBoard board, EncounterRules rules, EncounterRoster roster,
            Func<int, int, int> rollDice)
        {
            _board = board;
            _rules = rules;
            _roster = roster;
            _rollDice = rollDice;
        }

        public CommandError TryAttack(ActorState actor, string targetId, List<TacticalEvent> events)
        {
            if (!_roster.TryGet(targetId, out var target)) return CommandError.UnknownTarget;
            if (!target.IsAlive) return CommandError.TargetDefeated;
            if (target.Definition.FactionId == actor.Definition.FactionId) return CommandError.FriendlyTarget;
            if (!GridAdjacency.IsAdjacent(actor.Position, target.Position) ||
                Math.Abs((long)_board.GetCell(actor.Position).Height - _board.GetCell(target.Position).Height) > _rules.MaxHeightStep)
                return CommandError.TargetOutOfRange;
            if (actor.ActionPoints < _rules.AttackCost) return CommandError.NotEnoughActions;
            var roll = _rollDice(1, actor.Definition.AttackDiceSides);
            var hit = (long)roll + actor.Definition.AttackBonus > target.Definition.Defense;
            actor.SpendActions(_rules.AttackCost);
            var damageRoll = 0;
            var damage = 0;
            if (hit)
            {
                damageRoll = actor.Definition.UsesDamageDice
                    ? _rollDice(actor.Definition.DamageDiceCount, actor.Definition.DamageDiceSides)
                    : actor.Definition.Damage;
                damage = target.TakeDamage(Math.Max(0, damageRoll + actor.Definition.DamageBonus));
            }
            events.Add(new TacticalEvent(hit ? TacticalEventKind.AttackHit : TacticalEventKind.AttackMiss,
                actor.Definition.Id, targetId, target.Position, damage, roll, damageRoll));
            if (!target.IsAlive) _roster.RecordDefeat(target, actor.Definition.Id, events);
            return CommandError.None;
        }
    }
}
