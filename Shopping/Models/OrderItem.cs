using SQLite;

namespace OfflineShoppingApp.Shopping.Models;

[Table("OrderItem")]
public class OrderItem
{
    [PrimaryKey, AutoIncrement]
    public int OrderItemId { get; set; }

    [NotNull, Indexed]
    public int OrderId { get; set; }

    [NotNull, Indexed]
    public int ProductId { get; set; }

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }
}
