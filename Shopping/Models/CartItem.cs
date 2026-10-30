using SQLite;

namespace OfflineShoppingApp.Shopping.Models;

[Table("CartItem")]
public class CartItem
{
    [PrimaryKey, AutoIncrement]
    public int CartItemId { get; set; }

    [NotNull, Indexed]
    public int CustomerId { get; set; }

    [NotNull, Indexed]
    public int ProductId { get; set; }

    public int Quantity { get; set; }
}
