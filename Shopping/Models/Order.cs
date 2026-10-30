using SQLite;

namespace OfflineShoppingApp.Shopping.Models;

[Table("Order")]
public class Order
{
    [PrimaryKey, AutoIncrement]
    public int OrderId { get; set; }

    [NotNull, Indexed]
    public int CustomerId { get; set; }

    public DateTime OrderDate { get; set; }

    public decimal Total { get; set; }
}
