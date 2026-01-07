using CodeBase.Data;
using CodeBase.Infrastructure.Factory;
using CodeBase.Infrastructure.Services.ObjectPool;
using CodeBase.Infrastructure.Services.PersistentProgress;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

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

        private Loot _loot;
        private bool _picked;
        [SerializeField] private string _id;

        private WorldData _worldData;
        private IGameFactory _gameFactory;
        private IPool<LootPiece> _pool;

        public void Construct(WorldData worldData, IGameFactory gameFactory)
        {
            _worldData = worldData;
            _gameFactory = gameFactory;
        }

        public void SetPool(IPool<LootPiece> pool) => 
            _pool = pool;

        public void Initialize(Loot loot)
        {
            _loot = loot;
            SetId(GenerateId());

            Instantiate(PickupFxPrefab, transform.position, Quaternion.identity);
            PickupFxPrefab.SetActive(false);
        }

        private void OnTriggerEnter(Collider other) =>
            PickUp();

        public void SetId(string id) => _id = id;

        public void OnSpawned()
        {
            _picked = false;
            Prefab.SetActive(true);
        }

        public void OnDespawned()
        {
            PickupFxPrefab.SetActive(false);
            PickupPopup.SetActive(false);
        }

        public void UpdateProgress(PlayerProgress progress)
        {
            if (!CheckContainsId(progress))
            {
                LootObject lootObject = new(_id, transform.position, _loot);
                progress.WorldData.LootData.LootsOnGround.Dict.Add(_id, lootObject);
            }
        }

        public void LoadProgress(PlayerProgress progress)
        {
            _worldData = progress.WorldData;

            if (TryGetLootPiece(progress, out LootObject lootPiece))
            {
                LoadData(lootPiece);
                progress.WorldData.LootData.LootsOnGround.Dict.Remove(_id);
            }
        }

        private bool CheckContainsId(PlayerProgress progress)
        {
            return GetLootDataDict(progress).ContainsKey(_id);
        }

        private bool TryGetLootPiece(PlayerProgress progress, out LootObject lootPiece)
        {
            return GetLootDataDict(progress).TryGetValue(_id, out lootPiece);
        }

        private static Dictionary<string, LootObject> GetLootDataDict(PlayerProgress progress)
        {
            return progress.WorldData.LootData.LootsOnGround.Dict;
        }

        private void LoadData(LootObject lootObject)
        {
            _loot = lootObject.loot;
            transform.position = lootObject.positionData.AsUnityVector();
            _id = lootObject.id;
        }

        private string GenerateId()
        {
            return $"{gameObject.scene.name}_{Guid.NewGuid().ToString()}";
        }

        private void PickUp()
        {
            if (_picked)
                return;

            _picked = true;

            _gameFactory.ProgressWriters.Remove(this);
            _gameFactory.ProgressReaders.Remove(this);

            UpdateWorldData();
            HideLoot();
            PlayPickupFx();
            Showtext();

            StartCoroutine(ReturnToPoolAfterDelay());
        }

        private void UpdateWorldData() =>
            _worldData.LootData.Collect(_loot);

        private void HideLoot() =>
            Prefab.SetActive(false);

        private void PlayPickupFx() =>
           PickupFxPrefab.SetActive(true);

        private void Showtext()
        {
            LootText.text = $"{_loot.Value}";
            PickupPopup.SetActive(true);
        }

        private IEnumerator ReturnToPoolAfterDelay()
        {
            yield return new WaitForSeconds(1.5f);
            _pool.Despawn(this);
        }
    }
}
