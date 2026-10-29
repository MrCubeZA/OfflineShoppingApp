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
    public byte[] ImageData { get; set; } = Array.Empty<byte>();
    public bool IsFeatured { get; set; }
    private ImageSource? imageSource;
    [Ignore] public ImageSource ImageSource => imageSource ??= ImageSource.FromStream(() => new MemoryStream(ImageData));
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
    public byte[] ImageData { get; set; } = Array.Empty<byte>();
    private ImageSource? imageSource;
    public ImageSource ImageSource => imageSource ??= ImageSource.FromStream(() => new MemoryStream(ImageData));
    public decimal LineTotal => Quantity * UnitPrice;
}
