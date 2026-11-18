using System;
using CodeBase.Enemies;
using CodeBase.Infrastructure.Services.AIServices.BehaviourTree.Configuration;
using UnityEngine;
using UnityEngine.AI;
using Object = UnityEngine.Object;

namespace CodeBase.MotionTest
{
    internal sealed class SkeletonEnemyActor : IDisposable
    {
        public string Id { get; }
        public string GroupId { get; }
        public EnemyArchetypeConfig Archetype { get; }
        public MeleeTacticsAIConfig Behavior { get; }
        public bool IsAlive { get; private set; } = true;
        public GameObject Instance { get; }
        public EnemyAnimator Animator { get; }
        public NavMeshAgent Agent { get; }
        public SkeletonDetectionAdapter Detection { get; }
        public MotionTargetHighlighter Highlighter { get; }
        public MotionWorldHealthBar HealthBar { get; private set; }

        private readonly CapsuleCollider _selectionCollider;

        private SkeletonEnemyActor(GameObject instance, string id, string groupId,
            EnemyArchetypeConfig archetype, MeleeTacticsAIConfig behavior)
        {
            Instance = instance;
            Id = id;
            GroupId = groupId;
            Archetype = archetype;
            Behavior = behavior;
            Animator = instance.GetComponent<EnemyAnimator>();
            Agent = instance.GetComponent<NavMeshAgent>();
            Detection = instance.GetComponent<SkeletonDetectionAdapter>();
            Highlighter = instance.GetComponent<MotionTargetHighlighter>();
            _selectionCollider = instance.GetComponent<CapsuleCollider>();
        }

        public static SkeletonEnemyActor Create(GameObject fallbackPrefab, MeleeTacticsAIConfig fallbackAI,
            SkeletonEnemySpawn spawn, string id, MotionHero hero, MotionTestConfig config, Camera gameCamera,
            Transform parent, Action spotted)
        {
            var archetype = spawn.Archetype;
            var prefab = archetype != null && archetype.Prefab != null ? archetype.Prefab : fallbackPrefab;
            var instance = Object.Instantiate(prefab, spawn.Position, Quaternion.identity, parent);
            instance.name = (archetype != null ? archetype.DisplayName : "Enemy_Skeleton") + " — " + id;
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

            instance.AddComponent<MotionEnemyTarget>().Initialize(id, spawn.GroupId, archetype);
            instance.AddComponent<MotionTargetHighlighter>().Initialize(config);
            var detection = instance.AddComponent<SkeletonDetectionAdapter>();
            var enemy = new SkeletonEnemyActor(instance, id, spawn.GroupId, archetype,
                archetype != null && archetype.AI != null ? archetype.AI : fallbackAI);
            enemy.Agent.agentTypeID = config.AgentTypeId;
            enemy.Agent.radius = config.AgentRadius;
            enemy.Agent.height = config.AgentHeight;
            enemy.Agent.speed = config.Speed;
            enemy.Agent.stoppingDistance = .04f;
            if (NavMesh.SamplePosition(instance.transform.position, out var navigationPoint, 3, NavMesh.AllAreas))
                enemy.Agent.Warp(navigationPoint.position);
            detection.Construct(hero.Instance.transform, spotted);

            var healthObject = new GameObject("HUD health", typeof(RectTransform));
            healthObject.transform.SetParent(instance.transform, false);
            healthObject.transform.localPosition = Vector3.up * (config.AgentHeight + .45f);
            enemy.HealthBar = healthObject.AddComponent<MotionWorldHealthBar>();
            var maxHealth = archetype != null ? archetype.MaxHealth : 5;
            enemy.HealthBar.Initialize(gameCamera, maxHealth,
                archetype != null ? archetype.HealthBarColor : new Color(.9f, .22f, .18f));
            return enemy;
        }

        public void SetEnabled(bool value)
        {
            Detection.enabled = value;
            Instance.SetActive(value);
            HealthBar.SetVisible(value && IsAlive);
        }

        public void SetSelected(bool value) => Highlighter.SetVisible(value);

        public void MarkDead()
        {
            IsAlive = false;
            Detection.enabled = false;
            _selectionCollider.enabled = false;
            Highlighter.SetVisible(false);
            HealthBar.SetVisible(false);
        }

        public void Dispose()
        {
            if (Instance != null) Object.Destroy(Instance);
        }
    }
}
