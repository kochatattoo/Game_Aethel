using CodeBase.Infrastructure.AssetManagement;
using CodeBase.Infrastructure.Services.Ads;
using CodeBase.Infrastructure.Services.IAP;
using CodeBase.Infrastructure.Services.ObjectPool;
using CodeBase.Infrastructure.Services.PersistentProgress;
using CodeBase.UI.Windows.Shop;
using TMPro;
using UnityEngine;

namespace CodeBase.UI.Windows
{
    public class ShopWindow: WindowBase
    {
        public TextMeshProUGUI ValueText;
        public RewardedAdItem AdItem;
        public ShopItemsContainer ShopItemsContainer;

        public void Construct(IPersistentProgressService progressService, 
                              IPoolService poolService, 
                              IAdsService adsService, 
                              IIAPService iapService, 
                              IAsset assets)
        {
            base.Construct(progressService, poolService);
            AdItem.Construct(adsService, progressService);
            ShopItemsContainer.Construct(iapService, progressService, assets);
        }
        protected override void Initialize()
        {
            AdItem.Initialize();
            ShopItemsContainer.Initialize();
            RefreshValuesText();
        }
        protected override void SubscribeUpdates()
        {
            AdItem.Subscribe();
            ShopItemsContainer.Subscribe();
            Progress.WorldData.LootData.Changed += RefreshValuesText;
        }

        protected override void Cleanup()
        {
            base.Cleanup();
            AdItem.CleanUp();
            ShopItemsContainer.CleanUp();
            Progress.WorldData.LootData.Changed -= RefreshValuesText;
        }

        private void RefreshValuesText() =>
            ValueText.text = Progress.WorldData.LootData.Collected.ToString();

        protected override void Close()
        {
            Debug.Log(_poolService.GetPool<ShopWindow>());
            _poolService.GetPool<ShopWindow>().Despawn(this);
        }
    }
}
