using System;
using CodeBase.Enemies;
using UnityEngine;
using UnityEngine.AI;
using Object = UnityEngine.Object;

namespace CodeBase.MotionTest
{
    internal sealed class SkeletonEnemyActor : IDisposable
    {
        public GameObject Instance { get; }
        public EnemyAnimator Animator { get; }
        public NavMeshAgent Agent { get; }
        public SkeletonDetectionAdapter Detection { get; }

        private SkeletonEnemyActor(GameObject instance)
        {
            Instance = instance;
            Animator = instance.GetComponent<EnemyAnimator>();
            Agent = instance.GetComponent<NavMeshAgent>();
            Detection = instance.GetComponent<SkeletonDetectionAdapter>();
        }

        public static SkeletonEnemyActor Create(GameObject prefab, Vector3 spawn, MotionHero hero,
            MotionTestConfig config, Transform parent, Action spotted)
        {
            var instance = Object.Instantiate(prefab, spawn, Quaternion.identity, parent);
            instance.name = "Enemy_Skeleton — Motion combat";
            var direction = hero.Instance.transform.position - instance.transform.position;
            direction.y = 0;
            if (direction.sqrMagnitude > .001f) instance.transform.rotation = Quaternion.LookRotation(direction);
            foreach (var child in instance.GetComponentsInChildren<Transform>(true))
            {
                child.gameObject.layer = 8;
                if (child.name == "Canvas") child.gameObject.SetActive(false);
            }
            foreach (var component in instance.GetComponentsInChildren<Behaviour>(true))
            {
                if (component is AgentMoveToHero || component is RotateToHero || component is AnimateAlongAgent ||
                    component is Attack || component is AttackRuntime || component is CheckAttackRange ||
                    component is EnemyVision || component is EnemyHealth || component is EnemyDeath ||
                    component is LootSpawner) component.enabled = false;
            }

            instance.AddComponent<MotionEnemyTarget>();
            var detection = instance.AddComponent<SkeletonDetectionAdapter>();
            var enemy = new SkeletonEnemyActor(instance);
            enemy.Agent.agentTypeID = config.AgentTypeId;
            enemy.Agent.radius = config.AgentRadius;
            enemy.Agent.height = config.AgentHeight;
            enemy.Agent.speed = config.Speed;
            enemy.Agent.stoppingDistance = .04f;
            if (NavMesh.SamplePosition(instance.transform.position, out var navigationPoint, 3, NavMesh.AllAreas))
                enemy.Agent.Warp(navigationPoint.position);
            detection.Construct(hero.Instance.transform, spotted);
            return enemy;
        }

        public void SetEnabled(bool value)
        {
            Detection.enabled = value;
            Instance.SetActive(value);
        }

        public void Dispose()
        {
            if (Instance != null) Object.Destroy(Instance);
        }
    }
}
