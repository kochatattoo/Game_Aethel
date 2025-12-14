using CodeBase.Infrastructure.Services.IAP;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using CodeBase.Infrastructure.AssetManagement;

namespace CodeBase.UI.Windows
{
    public class ShopItem : MonoBehaviour
    {
        public Button BuyItemButton;
        public TextMeshProUGUI PriceText;
        public TextMeshProUGUI QuantityText;
        public TextMeshProUGUI AvailableItemsLeftText;
        public Image Icon;

        private ProductDescription _productDescription;
        private IIAPService _iapService;
        private IAsset _assets;

        public void Construct(IIAPService iAPService, IAsset assets, ProductDescription productDescription)
        {
            _iapService = iAPService;
            _assets = assets;

            _productDescription = productDescription;
        }

        public async void Initialize()
        {
            BuyItemButton.onClick.AddListener(OnBuyItemClick);

            PriceText.text = _productDescription.Config.Price;
            QuantityText.text = _productDescription.Config.Quantity.ToString();
            AvailableItemsLeftText.text = _productDescription.AvailablePurchasesLeft.ToString();
            Icon.sprite = await _assets.Load<Sprite>(_productDescription.Config.Icon);
        }

        private void OnBuyItemClick() =>
            _iapService.StartPurchase(_productDescription.Id);
    }
}
