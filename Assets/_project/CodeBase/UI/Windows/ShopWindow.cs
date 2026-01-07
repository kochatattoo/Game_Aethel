using CodeBase.Infrastructure.AssetManagement;
using CodeBase.Infrastructure.Services.Ads;
using CodeBase.Infrastructure.Services.IAP;
using CodeBase.Infrastructure.Services.ObjectPool;
using CodeBase.Infrastructure.Services.PersistentProgress;
using CodeBase.UI.Windows.Shop;
using TMPro;

namespace CodeBase.UI.Windows
{
    public class ShopWindow: WindowBase, IPoolable<ShopWindow>
    {
        public TextMeshProUGUI ValueText;
        public RewardedAdItem AdItem;
        public ShopItemsContainer ShopItemsContainer;
        private IPool<ShopWindow> _pool;

        public void Construct(IPersistentProgressService progressService,  
                              IAdsService adsService, 
                              IIAPService iapService, 
                              IAsset assets)
        {
            base.Construct(progressService);
            AdItem.Construct(adsService, progressService);
            ShopItemsContainer.Construct(iapService, progressService, assets);
        }
        public void SetPool(IPool<ShopWindow> pool)
        {
            _pool = pool;
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

        protected override void Close()
        {
            _pool.Despawn(this);
        }

        private void RefreshValuesText() =>
            ValueText.text = Progress.WorldData.LootData.Collected.ToString();
    }
}
