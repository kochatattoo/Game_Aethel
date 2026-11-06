using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using PetProject.Tactics;
using UnityEngine;

namespace CodeBase.MotionTest
{
    internal sealed class SkeletonCombatPresenter
    {
        private readonly IMotionEncounterContext _host;
        private readonly SkeletonEnemyActor _enemy;

        public SkeletonCombatPresenter(IMotionEncounterContext host, SkeletonEnemyActor enemy)
        {
            _host = host;
            _enemy = enemy;
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
            var actorTransform = actorId == MotionTestController.HeroActorId
                ? _host.Hero.transform
                : _enemy.Instance.transform;
            SetMoving(actorId, true);
            for (var i = 1; i < path.Cells.Count; ++i)
            {
                var destination = _host.Map.World(path.Cells[i]);
                while (!GridPathMovement.HasArrived(actorTransform, destination))
                {
                    if (version != currentVersion()) return;
                    if (!GridPathMovement.MoveStep(actorTransform, _host.Map, destination,
                        _host.World.Config.Speed))
                    {
                        _host.SetNotice(
                            "Представление потеряло опору; выполните Reset");
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
            var attackerTransform = attacker == MotionTestController.HeroActorId
                ? _host.Hero.transform
                : _enemy.Instance.transform;
            var targetTransform = target == MotionTestController.HeroActorId
                ? _host.Hero.transform
                : _enemy.Instance.transform;
            var direction = targetTransform.position - attackerTransform.position;
            direction.y = 0;
            if (direction.sqrMagnitude > .001f) attackerTransform.rotation = Quaternion.LookRotation(direction);
            if (attacker == MotionTestController.HeroActorId) _host.HeroAnimator.PlayAttack();
            else _enemy.Animator.PlayAttack1();
            await UniTask.Delay(TimeSpan.FromSeconds(.25));
            if (version != currentVersion()) return;
            if (attack.Kind == TacticalEventKind.AttackHit)
            {
                if (target == MotionTestController.HeroActorId) _host.HeroAnimator.PlayHit();
                else _enemy.Animator.PlayHit();
            }
            _host.SetNotice(attack.Kind == TacticalEventKind.AttackHit
                ? $"{attacker}: d20={attack.Roll}, урон {attack.Amount}"
                : $"{attacker}: d20={attack.Roll}, промах");
            await UniTask.Delay(TimeSpan.FromSeconds(.35));
            if (version != currentVersion()) return;
            foreach (var defeated in result.Events.Where(e => e.Kind == TacticalEventKind.ActorDefeated))
            {
                if (defeated.ActorId == MotionTestController.HeroActorId) _host.HeroAnimator.PlayDeath();
                else _enemy.Animator.PlayDeath();
                await UniTask.Delay(TimeSpan.FromSeconds(.7));
                if (version != currentVersion()) return;
                if (defeated.ActorId != MotionTestController.HeroActorId) _enemy.Instance.SetActive(false);
            }
        }

        private void SetMoving(string actorId, bool moving)
        {
            if (actorId == MotionTestController.HeroActorId)
            {
                if (moving) _host.HeroAnimator.Move();
                else _host.HeroAnimator.StopMoving();
            }
            else
            {
                if (moving) _enemy.Animator.Move(_host.World.Config.Speed);
                else _enemy.Animator.StopMoving();
            }
        }
    }
}
