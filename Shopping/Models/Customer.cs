using SQLite;

namespace OfflineShoppingApp.Shopping.Models;

[Table("Customer")]
public class Customer
{
    [PrimaryKey, AutoIncrement]
    public int CustomerId { get; set; }

    [NotNull]
    public string Name { get; set; } = string.Empty;
}
