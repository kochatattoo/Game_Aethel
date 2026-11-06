namespace CodeBase.Infrastructure.Services.IAP
{
    public sealed class ShopProductInfo
    {
        public string Price { get; }
        public int Quantity { get; }
        public string Icon { get; }
        public ShopProductInfo(string price, int quantity, string icon) { Price = price; Quantity = quantity; Icon = icon; }
    }
}
