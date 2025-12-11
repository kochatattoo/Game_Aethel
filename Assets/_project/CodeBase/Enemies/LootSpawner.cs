using CodeBase.Data;
using CodeBase.Infrastructure.Factory;
using CodeBase.Infrastructure.Services;
using CodeBase.Infrastructure.Services.PersistentProgress;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace CodeBase.Enemies
{
    public class LootSpawner : MonoBehaviour
    {
        public EnemyDeath EnemyDeath;
        private IGameFactory _factory;
        private IRandomService _random;
        private int _lootMin;
        private int _lootMax;

        public void Construct(IGameFactory gameFactory, IRandomService random)
        {
            _factory = gameFactory;
            _random = random;
        }

        private void Start()
        {
            EnemyDeath.Happened += SpawnLoot;
        }

        private async void SpawnLoot()
        {
            LootPiece loot = await _factory.CreateLoot();
            loot.transform.position = transform.position;

            Loot lootItem = GenerateLoot();
            loot.Initialize(lootItem);
        }

        private Loot GenerateLoot()
        {
            return new Loot()
            {
                Value = _random.Next(_lootMin, _lootMax)
            };
        }

        public void Setloot(int min, int max)
        {
            _lootMin = min;
            _lootMax = max;
        }
    }

    public class LootPiece : MonoBehaviour, ISavedProgress
    {
        public GameObject Skull;
        public GameObject PickupFxPrefab;
        public TextMeshPro LootText;
        public GameObject PickupPopup;

        private Loot _loot;
        private bool _picked;
        private WorldData _worldData;
        [SerializeField] private string _id;

        private IGameFactory _gameFactory;

        public void Construct(WorldData worldData, IGameFactory gameFactory)
        {
            _worldData = worldData;
            _gameFactory = gameFactory;
        }

        public void Initialize(Loot loot)
        {
            SetId(GenerateId());
            _loot = loot;
        }

        private void OnTriggerEnter(Collider other) =>
            PickUp();

        public void SetId(string id) => _id = id;

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
            HideSkull();
            PlayPickupFx();
            Showtext();

            StartCoroutine(StartDestroyTimer());
        }

        private void UpdateWorldData() =>
            _worldData.LootData.Collect(_loot);

        private void HideSkull() =>
            Skull.SetActive(false);

        private void PlayPickupFx() =>
            Instantiate(PickupFxPrefab, transform.position, Quaternion.identity);

        private void Showtext()
        {
            LootText.text = $"{_loot.Value}";
            PickupPopup.SetActive(true);
        }

        private IEnumerator StartDestroyTimer()
        {
            yield return new WaitForSeconds(1.5f);

            Destroy(gameObject);
        }
    }
}
