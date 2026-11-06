using CodeBase.Data;
using TMPro;
using UnityEngine;

namespace CodeBase.Enemies
{
    internal sealed class LootPieceView
    {
        private readonly GameObject _prefab;
        private readonly GameObject _pickupPopup;
        private readonly TextMeshPro _lootText;
        private readonly GameObject _pickupFx;

        public LootPieceView(GameObject prefab, GameObject pickupPopup, TextMeshPro lootText, GameObject pickupFx)
        {
            _prefab = prefab;
            _pickupPopup = pickupPopup;
            _lootText = lootText;
            _pickupFx = pickupFx;
        }

        public void Initialize() => _pickupFx.SetActive(false);

        public void Spawn() => _prefab.SetActive(true);

        public void Reset()
        {
            _pickupFx.SetActive(false);
            _pickupPopup.SetActive(false);
        }

        public void ShowPickup(Loot loot)
        {
            _prefab.SetActive(false);
            _pickupFx.SetActive(true);
            _lootText.text = $"{loot.Value}";
            _pickupPopup.SetActive(true);
        }
    }
}
