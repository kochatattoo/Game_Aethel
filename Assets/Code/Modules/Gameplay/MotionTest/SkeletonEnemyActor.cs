using System;
using CodeBase.Enemies;
using UnityEngine;
using UnityEngine.AI;
using Object = UnityEngine.Object;

namespace CodeBase.MotionTest
{
    internal sealed class SkeletonEnemyActor : IDisposable
    {
        public string Id { get; }
        public string GroupId { get; }
        public bool IsAlive { get; private set; } = true;
        public GameObject Instance { get; }
        public EnemyAnimator Animator { get; }
        public NavMeshAgent Agent { get; }
        public SkeletonDetectionAdapter Detection { get; }
        public MotionTargetHighlighter Highlighter { get; }

        private readonly CapsuleCollider _selectionCollider;

        private SkeletonEnemyActor(GameObject instance, string id, string groupId)
        {
            Instance = instance;
            Id = id;
            GroupId = groupId;
            Animator = instance.GetComponent<EnemyAnimator>();
            Agent = instance.GetComponent<NavMeshAgent>();
            Detection = instance.GetComponent<SkeletonDetectionAdapter>();
            Highlighter = instance.GetComponent<MotionTargetHighlighter>();
            _selectionCollider = instance.GetComponent<CapsuleCollider>();
        }

        public static SkeletonEnemyActor Create(GameObject prefab, SkeletonEnemySpawn spawn, string id,
            MotionHero hero, MotionTestConfig config, Transform parent, Action spotted)
        {
            var instance = Object.Instantiate(prefab, spawn.Position, Quaternion.identity, parent);
            instance.name = "Enemy_Skeleton — " + id;
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
                    component is CodeBase.Sensors.BladeSensor || component is EnemyVision ||
                    component is EnemyHealth || component is EnemyDeath || component is LootSpawner)
                    component.enabled = false;
            }
            foreach (var oldCollider in instance.GetComponentsInChildren<Collider>(true))
                oldCollider.enabled = false;
            var selectionCollider = instance.AddComponent<CapsuleCollider>();
            selectionCollider.isTrigger = true;
            selectionCollider.center = new Vector3(0, config.AgentHeight * .5f, 0);
            selectionCollider.height = config.AgentHeight;
            selectionCollider.radius = Mathf.Max(config.AgentRadius, .45f);

            instance.AddComponent<MotionEnemyTarget>().Initialize(id, spawn.GroupId);
            instance.AddComponent<MotionTargetHighlighter>().Initialize(config);
            var detection = instance.AddComponent<SkeletonDetectionAdapter>();
            var enemy = new SkeletonEnemyActor(instance, id, spawn.GroupId);
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

        public void SetSelected(bool value) => Highlighter.SetVisible(value);

        public void MarkDead()
        {
            IsAlive = false;
            Detection.enabled = false;
            _selectionCollider.enabled = false;
            Highlighter.SetVisible(false);
        }

        public void Dispose()
        {
            if (Instance != null) Object.Destroy(Instance);
        }
    }
}
