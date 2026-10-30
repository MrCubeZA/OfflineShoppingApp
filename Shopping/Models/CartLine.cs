namespace OfflineShoppingApp.Shopping.Models;

public class CartLine
{
    public int CartItemId { get; set; }

    public int ProductId { get; set; }

    public string Name { get; set; } = string.Empty;

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal LineTotal => Quantity * UnitPrice;

    public string ImageFile => $"product{ProductId}.jpg";
}
