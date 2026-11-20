using System;
using UnityEngine;
using UnityEngine.AI;
using Object = UnityEngine.Object;
using PetProject.Navigation;

namespace CodeBase.Hero
{
    public sealed class HeroMotor : IDisposable
    {
        private readonly NavigationConfig _config;
        private readonly HeroPathFollower _follower;

        public GameObject Instance { get; }
        public NavMeshAgent Agent { get; }
        public HeroAnimator Animator { get; }
        public HeroHealth Health { get; private set; }
        public bool IsAnimating => Animator.Animator.GetBool("Move");
        public bool NormalMoving => Agent.enabled && Agent.isOnNavMesh &&
            (Agent.pathPending || Agent.remainingDistance > Agent.stoppingDistance);
        public bool AlignmentFailed => !Agent.pathPending && Agent.pathStatus != NavMeshPathStatus.PathComplete;
        public bool AlignmentCompleted => !Agent.pathPending && Agent.remainingDistance <= .06f;

        private HeroMotor(GameObject instance, NavigationConfig config)
        {
            Instance = instance;
            _config = config;
            Agent = instance.GetComponent<NavMeshAgent>();
            _follower = instance.GetComponent<HeroPathFollower>();
            Animator = instance.GetComponent<HeroAnimator>();
        }

        public static HeroMotor Create(GameObject prefab, NavigationConfig config)
        {
            var instance = Object.Instantiate(prefab, config.Spawn, Quaternion.identity);
            instance.name = "Hero";
            foreach (var child in instance.GetComponentsInChildren<Transform>(true)) child.gameObject.layer = 2;
            if (instance.GetComponent<Collider>() == null)
            {
                var body = instance.AddComponent<CapsuleCollider>();
                body.isTrigger = false;
                body.center = new Vector3(0, config.AgentHeight * .5f, 0);
                body.height = config.AgentHeight;
                body.radius = config.AgentRadius;
            }
            if (instance.GetComponent<Rigidbody>() == null)
            {
                var body = instance.AddComponent<Rigidbody>();
                body.isKinematic = true;
                body.useGravity = false;
                body.constraints = RigidbodyConstraints.FreezeRotation;
                body.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
            }

            var hero = new HeroMotor(instance, config);
            hero.Health = instance.GetComponent<HeroHealth>();
            hero.Animator.Animator.applyRootMotion = false;
            hero.Animator.Animator.fireEvents = false;
            hero.Animator.Animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            hero.Agent.speed = config.Speed;
            hero.Agent.angularSpeed = 540;
            hero.Agent.acceleration = 30;
            hero.Agent.stoppingDistance = .04f;
            hero._follower.Construct();
            hero.Agent.speed = config.Speed;
            hero.Agent.stoppingDistance = .04f;
            return hero;
        }

        public bool MoveNormally(Vector3 point, NavMeshGridMap map, out string error)
        {
            error = null;
            if (!map.Cell(point, out _) || !map.Support(point, out var target, out _))
            {
                error = "Недопустимая поверхность";
                return false;
            }

            var path = new NavMeshPath();
            if (!NavMesh.SamplePosition(target, out var nav, .22f, Agent.areaMask) ||
                Vector3.Distance(nav.position, target) > .3f || !Agent.CalculatePath(nav.position, path) ||
                path.status != NavMeshPathStatus.PathComplete)
            {
                error = "Нет полного пути";
                return false;
            }

            Agent.isStopped = false;
            Agent.SetPath(path);
            return true;
        }

        public bool BeginAlignment(Vector3 destination)
        {
            Agent.isStopped = false;
            return Agent.SetDestination(destination);
        }

        public void CompleteAlignment(Vector3 destination)
        {
            Agent.ResetPath();
            Agent.Warp(destination);
            EnterTactical();
        }

        public void Stop() => _follower.Stop();

        public void EnterTactical()
        {
            _follower.Stop();
            _follower.enabled = false;
            Agent.enabled = false;
            Animator.StopMoving();
        }

        public void PrepareForEncounter()
        {
            _follower.Stop();
            _follower.enabled = false;
            if (Agent.enabled) Agent.ResetPath();
            Agent.enabled = false;
            Animator.StopMoving();
        }

        public bool RestoreExploration(bool resetAnimation)
        {
            Agent.enabled = true;
            if (!Agent.Warp(Instance.transform.position)) return false;
            Agent.ResetPath();
            Agent.isStopped = true;
            _follower.enabled = true;
            if (resetAnimation) Animator.ResetToIdle();
            else Animator.StopMoving();
            return true;
        }

        public void Reset()
        {
            Agent.enabled = true;
            Agent.Warp(_config.Spawn);
            Agent.ResetPath();
            Agent.isStopped = true;
            _follower.enabled = true;
            Animator.StopMoving();
            Animator.ResetToIdle();
        }

        public void Dispose()
        {
            if (Instance != null) Object.Destroy(Instance);
        }
    }
}
