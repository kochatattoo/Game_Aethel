using System;
using CodeBase.Hero;
using CodeBase.Infrastructure.Services.AIServices.BehaviourTree.Configuration;
using PetProject.EnemyArchetypes;
using UnityEngine;
using UnityEngine.AI;
using Object = UnityEngine.Object;
using PetProject.Encounter;
using PetProject.Navigation;
using CodeBase.UI.Encounter;

namespace CodeBase.Enemies
{
    public sealed class EnemyActor : IDisposable
    {
        public event Action Defeated;

        public string Id { get; }
        public string GroupId { get; }
        public EnemyArchetypeConfig Archetype { get; }
        public IEnemyArchetype ArchetypeDefinition { get; }
        public MeleeTacticsAIConfig Behavior { get; }
        public bool IsAlive { get; private set; } = true;
        public GameObject Instance { get; }
        public EnemyAnimator Animator { get; }
        public NavMeshAgent Agent { get; }
        public EnemyDetection Detection { get; }
        public TargetHighlightView Highlighter { get; }
        public WorldHealthBarView HealthBar { get; private set; }

        private readonly CapsuleCollider _selectionCollider;

        private EnemyActor(GameObject instance, string id, string groupId,
            EnemyArchetypeConfig archetype, IEnemyArchetype archetypeDefinition, MeleeTacticsAIConfig behavior)
        {
            Instance = instance;
            Id = id;
            GroupId = groupId;
            Archetype = archetype;
            ArchetypeDefinition = archetypeDefinition;
            Behavior = behavior;
            Animator = instance.GetComponent<EnemyAnimator>();
            Agent = instance.GetComponent<NavMeshAgent>();
            Detection = instance.GetComponent<EnemyDetection>();
            Highlighter = instance.GetComponent<TargetHighlightView>();
            _selectionCollider = instance.GetComponent<CapsuleCollider>();
        }

        public static EnemyActor Create(GameObject fallbackPrefab, MeleeTacticsAIConfig fallbackAI,
            EnemySpawnDefinition spawn, IEnemyArchetype archetypeDefinition, string id, HeroMotor hero,
            NavigationConfig config, Camera gameCamera, Transform parent, Action spotted)
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
            foreach (var oldCollider in instance.GetComponentsInChildren<Collider>(true))
                oldCollider.enabled = false;
            var selectionCollider = instance.AddComponent<CapsuleCollider>();
            selectionCollider.isTrigger = true;
            selectionCollider.center = new Vector3(0, config.AgentHeight * .5f, 0);
            selectionCollider.height = config.AgentHeight;
            selectionCollider.radius = Mathf.Max(config.AgentRadius, .45f);

            instance.AddComponent<EnemyTarget>().Initialize(id, spawn.GroupId, archetype);
            instance.AddComponent<TargetHighlightView>().Initialize(config);
            var detection = instance.AddComponent<EnemyDetection>();
            var enemy = new EnemyActor(instance, id, spawn.GroupId, archetype, archetypeDefinition,
                archetype != null && archetype.AI != null ? archetype.AI : fallbackAI);
            foreach (var loot in instance.GetComponentsInChildren<LootSpawner>(true)) loot.Initialize(enemy);
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
            enemy.HealthBar = healthObject.AddComponent<WorldHealthBarView>();
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
            Defeated?.Invoke();
        }

        public void Dispose()
        {
            if (Instance != null) Object.Destroy(Instance);
        }
    }
}
