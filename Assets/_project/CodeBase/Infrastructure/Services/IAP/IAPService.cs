using CodeBase.Data;
using CodeBase.Infrastructure.Services.IAP;
using CodeBase.Infrastructure.Services.PersistentProgress;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Purchasing;
using Zenject;

namespace Assets._project.CodeBase.Infrastructure.Services.IAP
{
    public class IAPService : IIAPService, IInitializable
    {
        private readonly IAPProvider _iapProvider;
        private readonly IPersistentProgressService _progressService;

        public bool IsInitialized => _iapProvider.IsInitialized;
        public event Action Initialized;

        public IAPService(IPersistentProgressService progressService)
        {
            _iapProvider = new();
            _progressService = progressService;
        }

        public void Initialize()
        {
            _iapProvider.Initialize(this);
            _iapProvider.Initialized += () => Initialized?.Invoke();
        }

        public List<ProductDescription> Products() =>
            ProductDescription().ToList();

        public void StartPurchase(string productId) =>
            _iapProvider.StartPurchase(productId);

        public PurchaseProcessingResult ProcessPurchase(Product purchaseProduct)
        {
            ProductConfig productConfig = _iapProvider.Configs[purchaseProduct.definition.id];

            switch (productConfig.ItemType)
            {
                case ItemType.Skulls:
                    _progressService.Progress.WorldData.LootData.Add(productConfig.Quantity);
                    _progressService.Progress.PurchaseData.AddPurchase(purchaseProduct.definition.id);
                    break;
                default:

                    break;
            }

            return PurchaseProcessingResult.Complete;
        }

        private IEnumerable<ProductDescription> ProductDescription()
        {
            PurchaseData purchaseData = _progressService.Progress.PurchaseData;

            foreach (string productId in _iapProvider.Products.Keys)
            {
                ProductConfig config = _iapProvider.Configs[productId];
                Product product = _iapProvider.Products[productId];

                if (purchaseData.BoughtIAPs.Dict.TryGetValue(productId, out BoughtIAP boughtIap))
                {
                    if (ProductBoughtOut(config, boughtIap))
                        continue;

                    yield return new ProductDescription
                    {
                        Id = productId,
                        Config = config,
                        Product = product,
                        AvailablePurchasesLeft = config.MaxPurchaseCount - boughtIap.Count
                    };

                }
                else
                    yield return new ProductDescription
                    {
                        Id = productId,
                        Config = config,
                        Product = product,
                        AvailablePurchasesLeft = config.MaxPurchaseCount
                    };
            }
        }

        private static bool ProductBoughtOut(ProductConfig config, BoughtIAP boughtIap) =>
            boughtIap.Count >= config.MaxPurchaseCount;
    }
}
