using System;
using System.Collections;
using CodeBase.Data;
using CodeBase.Infrastructure.Services.ObjectPool;
using CodeBase.Infrastructure.Services.PersistentProgress;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CodeBase.Enemies
{
    public class LootPiece : MonoBehaviour, ISavedProgress, IPoolable<LootPiece>
    {
        [Header("View")]
        public GameObject Prefab;
        public GameObject PickupPopup;
        public TextMeshPro LootText;

        [Header("VFX")]
        public GameObject PickupFxPrefab;

        [SerializeField] private string _id;
        private Loot _loot;
        private bool _picked;
        private WorldData _worldData;
        private IProgressRegistry _registry;
        private IPool<LootPiece> _pool;
        private LootPieceView _view;

        private LootPieceView View => _view ??= new LootPieceView(Prefab, PickupPopup, LootText, PickupFxPrefab);

        public void Construct(WorldData worldData, IProgressRegistry registry)
        {
            _worldData = worldData;
            _registry = registry;
        }

        public void SetPool(IPool<LootPiece> pool) => _pool = pool;

        public void Initialize(Loot loot)
        {
            _loot = loot;
            SetId(GenerateId());
            View.Initialize();
        }

        public void SetId(string id) => _id = id;

        public void OnSpawned()
        {
            _picked = false;
            View.Spawn();
        }

        public void Release() => _pool.Despawn(this);

        public void OnDespawned()
        {
            StopAllCoroutines();
            _registry?.Unregister(this);
            View.Reset();
        }

        public void UpdateProgress(PlayerProgress progress) =>
            LootProgressStorage.Save(progress, _id, transform.position, _loot);

        public void LoadProgress(PlayerProgress progress)
        {
            _worldData = progress.WorldData;
            if (!LootProgressStorage.TryTake(progress, _id, out var loot)) return;
            _loot = loot.loot;
            transform.position = loot.positionData.AsUnityVector();
            _id = loot.id;
        }

        private void OnTriggerEnter(Collider other) => PickUp();

        private void PickUp()
        {
            if (_picked || _loot == null) return;
            _picked = true;
            _registry.Unregister(this);
            LootProgressStorage.Collect(_worldData, _id, _loot);
            View.ShowPickup(_loot);
            StartCoroutine(ReturnToPoolAfterDelay());
        }

        private static string GenerateId() => $"{SceneManager.GetActiveScene().name}_{Guid.NewGuid()}";

        private IEnumerator ReturnToPoolAfterDelay()
        {
            yield return new WaitForSeconds(1.5f);
            _pool.Despawn(this);
        }
    }
}
