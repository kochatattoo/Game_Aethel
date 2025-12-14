using CodeBase.Infrastructure.AssetManagement;
using CodeBase.Infrastructure.Services.IAP;
using CodeBase.Infrastructure.Services.PersistentProgress;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace CodeBase.UI.Windows
{
    public class ShopItemsContainer : MonoBehaviour
    {
        private const string ShopItemPath = "ShopItem";

        public GameObject[] ShopUnavaibleObjects;
        public Transform Parent;
        private IIAPService _iapService;
        private IPersistentProgressService _progressService;
        private IAsset _assets;

        private readonly List<GameObject> _shopItems = new();

        public void Construct(IIAPService iAPService, IPersistentProgressService progressService, IAsset assets)
        {
            _iapService = iAPService;
            _progressService = progressService;
            _assets = assets;
        }

        public void Initialize() =>
            RefreshAvailableItems();

        public void Subscribe()
        {
            _iapService.Initialized += RefreshAvailableItems;
            _progressService.Progress.PurchaseData.Changed += RefreshAvailableItems;
        }

        public void CleanUp()
        {
            _iapService.Initialized -= RefreshAvailableItems;
            _progressService.Progress.PurchaseData.Changed -= RefreshAvailableItems;
        }

        private async void RefreshAvailableItems()
        {
            UpdateShopUnavailableObjects();

            if (!_iapService.IsInitialized)
                return;

            ClearShopItems();

            await FillShopItems();
        }

        private void ClearShopItems()
        {
            foreach (GameObject shopItem in _shopItems)
                Destroy(shopItem);
        }

        private async Task FillShopItems()
        {
            foreach (ProductDescription productDescription in _iapService.Products())
            {
                GameObject shopItemObject = await _assets.Instantiate(ShopItemPath, Parent);
                ShopItem shopItem = shopItemObject.GetComponent<ShopItem>();

                shopItem.Construct(_iapService, _assets, productDescription);

                shopItem.Initialize();

                _shopItems.Add(shopItemObject);
            }
        }

        private void UpdateShopUnavailableObjects()
        {
            foreach (GameObject shopUnavailableObject in ShopUnavaibleObjects)
                shopUnavailableObject.SetActive(!_iapService.IsInitialized);
        }
    }
}
