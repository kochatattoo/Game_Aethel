using CodeBase.Data;
using CodeBase.Infrastructure.Services.IAP;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Purchasing;

namespace CodeBase.Infrastructure.Services.IAP
{
    public class IAPProvider : IStoreListener
    {
        private const string IAPConfigsPath = "IAP/products";

        private IStoreController _controller;
        private IExtensionProvider _extensions;
        private IAPService _iapService;

        public Dictionary<string, ProductConfig> Configs { get; private set; }
        public Dictionary<string, Product> Products { get; private set; }

        public event Action Initialized;

        public bool IsInitialized => _controller != null && _extensions != null;

        public void Initialize(IAPService iapService)
        {
            _iapService = iapService;

            Configs = new Dictionary<string, ProductConfig>();
            Products = new Dictionary<string, Product>();

            Load();

            ConfigurationBuilder builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());

            foreach (ProductConfig produccConfig in Configs.Values)
                builder.AddProduct(produccConfig.Id, produccConfig.ProductType);

            UnityPurchasing.Initialize(this, builder);
        }

        public void StartPurchase(string productId) =>
            _controller.InitiatePurchase(productId);

        public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
        {
            _controller = controller;
            _extensions = extensions;

            foreach (Product product in _controller.products.all)
                Products.Add(product.definition.id, product);


            Initialized?.Invoke();

            Debug.Log("UnityPurchasing initialization success");
        }

        public void OnInitializeFailed(InitializationFailureReason error)
        {
            Debug.Log($"UnityPurchasing OnInitializeFailed: {error}");
        }

        public void OnInitializeFailed(InitializationFailureReason error, string message = null)
        {
            Debug.Log($"UnityPurchasing OnInitializeFailed: {error}");
        }

        public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason) =>
            Debug.LogError($"Product {product.definition.id} purchase failed," +
                $" PurchaseFailureReason {failureReason}," +
                $" transaction id {product.transactionID} ");

        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
        {
            Debug.Log($"UnityPurchasing ProcessPurchase success {args.purchasedProduct.definition.id}");

            return _iapService.ProcessPurchase(args.purchasedProduct);
        }

        private void Load() =>
            Configs = Resources
            .Load<TextAsset>(IAPConfigsPath)
            .text
            .ToDeserialized<ProductConfigWrapper>()
            .Configs
            .ToDictionary(x => x.Id, x => x);

    }
}
