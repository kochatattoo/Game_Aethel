using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using PetProject.Tactics;
using UnityEngine;
using CodeBase.AI.Tactics;
using CodeBase.Enemies;
using CodeBase.Hero;
using CodeBase.Infrastructure.Services;
using CodeBase.Infrastructure.Utils;
using CodeBase.UI.Encounter;
using PetProject.Navigation;

namespace PetProject.Encounter
{
    internal sealed class EncounterPresenter
    {
        private readonly IEncounterContext _host;
        private readonly IReadOnlyDictionary<string, EnemyActor> _enemies;

        public EncounterPresenter(IEncounterContext host,
            IReadOnlyDictionary<string, EnemyActor> enemies)
        {
            _host = host;
            _enemies = enemies;
        }

        public async UniTask PlayAsync(TacticalCommand command, CommandResult result, GridPath path,
            int version, Func<int> currentVersion)
        {
            if (command.Kind == TacticalCommandKind.Move)
                await AnimateMove(command.ActorId, path, version, currentVersion);
            else if (command.Kind == TacticalCommandKind.Attack)
                await AnimateAttack(result, version, currentVersion);
        }

        private async UniTask AnimateMove(string actorId, GridPath path, int version, Func<int> currentVersion)
        {
            if (path == null || !path.Found || path.Cells.Count < 2) return;
            var actorTransform = actorId == EncounterActorIds.Hero
                ? _host.Hero.transform
                : _enemies[actorId].Instance.transform;
            SetMoving(actorId, true);
            for (var i = 1; i < path.Cells.Count; ++i)
            {
                var destination = _host.Map.World(path.Cells[i]);
                while (!GridMotor.HasArrived(actorTransform, destination))
                {
                    if (version != currentVersion()) return;
                    if (!GridMotor.MoveStep(actorTransform, _host.Map, destination,
                        _host.World.Config.Speed))
                    {
                        _host.SetNotice("Представление потеряло опору; выполните Reset");
                        return;
                    }
                    await UniTask.Yield();
                }
                actorTransform.position = destination;
            }
            SetMoving(actorId, false);
        }

        private async UniTask AnimateAttack(CommandResult result, int version, Func<int> currentVersion)
        {
            var attack = result.Events.FirstOrDefault(e =>
                e.Kind == TacticalEventKind.AttackHit || e.Kind == TacticalEventKind.AttackMiss);
            if (attack.Kind != TacticalEventKind.AttackHit && attack.Kind != TacticalEventKind.AttackMiss) return;
            var attacker = attack.ActorId;
            var target = attack.TargetId;
            var attackerTransform = attacker == EncounterActorIds.Hero
                ? _host.Hero.transform
                : _enemies[attacker].Instance.transform;
            var targetTransform = target == EncounterActorIds.Hero
                ? _host.Hero.transform
                : _enemies[target].Instance.transform;
            var direction = targetTransform.position - attackerTransform.position;
            direction.y = 0;
            if (direction.sqrMagnitude > .001f) attackerTransform.rotation = Quaternion.LookRotation(direction);
            if (attacker == EncounterActorIds.Hero) _host.HeroAnimator.PlayAttack();
            else _enemies[attacker].Animator.PlayAttack1();
            await UniTask.Delay(TimeSpan.FromSeconds(.25));
            if (version != currentVersion()) return;
            if (attack.Kind == TacticalEventKind.AttackHit)
            {
                if (target == EncounterActorIds.Hero) _host.HeroAnimator.PlayHit();
                else _enemies[target].Animator.PlayHit();
            }
            _host.SetNotice(attack.Kind == TacticalEventKind.AttackHit
                ? $"{attacker}: d20={attack.Roll}, урон {attack.Amount}"
                : $"{attacker}: d20={attack.Roll}, промах");
            await UniTask.Delay(TimeSpan.FromSeconds(.35));
            if (version != currentVersion()) return;
            foreach (var defeated in result.Events.Where(e => e.Kind == TacticalEventKind.ActorDefeated))
            {
                if (defeated.ActorId == EncounterActorIds.Hero)
                {
                    _host.HeroAnimator.PlayDeath();
                    await UniTask.Delay(TimeSpan.FromSeconds(.7));
                    if (version != currentVersion()) return;
                    continue;
                }
                var enemy = _enemies[defeated.ActorId];
                enemy.Animator.PlayDeath();
                var elapsed = 0f;
                while (!enemy.Animator.DeathAnimationCompleted && elapsed < 10f)
                {
                    if (version != currentVersion()) return;
                    elapsed += Time.deltaTime;
                    await UniTask.Yield();
                }
                enemy.MarkDead();
                if (!enemy.Animator.DeathAnimationCompleted)
                    _host.SetNotice("Анимация смерти скелета не завершилась; труп сохранён");
            }
        }

        private void SetMoving(string actorId, bool moving)
        {
            if (actorId == EncounterActorIds.Hero)
            {
                if (moving) _host.HeroAnimator.Move();
                else _host.HeroAnimator.StopMoving();
            }
            else
            {
                if (moving) _enemies[actorId].Animator.Move(_host.World.Config.Speed);
                else _enemies[actorId].Animator.StopMoving();
            }
        }
    }
}
