using SQLite;
namespace OfflineShoppingApp.Shopping;

[Table("Customer")]
public class StoreCustomer
{
    [PrimaryKey, AutoIncrement] public int CustomerId { get; set; }
    [NotNull] public string Name { get; set; } = string.Empty;
}
[Table("Category")]
public class Category
{
    [PrimaryKey, AutoIncrement] public int CategoryId { get; set; }
    [NotNull] public string Name { get; set; } = string.Empty;
}
[Table("Product")]
public class Product
{
    [PrimaryKey, AutoIncrement] public int ProductId { get; set; }
    [Indexed] public int CategoryId { get; set; }
    [NotNull] public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public bool IsFeatured { get; set; }
    // Photo is an app image file (Resources/Images/product1.jpg etc.), not stored in the database.
    [Ignore] public string ImageFile => $"product{ProductId}.jpg";
    [Ignore] public string CategoryName { get; set; } = string.Empty;
}
[Table("CartItem")]
public class CartItem
{
    [PrimaryKey, AutoIncrement] public int CartItemId { get; set; }
    [Indexed("CustomerProduct", 1, Unique = true)] public int CustomerId { get; set; }
    [Indexed("CustomerProduct", 2, Unique = true)] public int ProductId { get; set; }
    public int Quantity { get; set; }
}
[Table("Order")]
public class StoreOrder
{
    [PrimaryKey, AutoIncrement] public int OrderId { get; set; }
    [Indexed] public int CustomerId { get; set; }
    public DateTime OrderDate { get; set; }
    public decimal Total { get; set; }
}
[Table("OrderItem")]
public class OrderItem
{
    [PrimaryKey, AutoIncrement] public int OrderItemId { get; set; }
    [Indexed] public int OrderId { get; set; }
    [Indexed] public int ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}
public class CartLine
{
    public int CartItemId { get; set; }
    public int ProductId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public string ImageFile => $"product{ProductId}.jpg";
    public decimal LineTotal => Quantity * UnitPrice;
}
