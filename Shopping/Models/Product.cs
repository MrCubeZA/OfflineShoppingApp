using SQLite;

namespace OfflineShoppingApp.Shopping.Models;

[Table("Product")]
public class Product
{
    [PrimaryKey, AutoIncrement]
    public int ProductId { get; set; }

    [NotNull, Indexed]
    public int CategoryId { get; set; }

    [NotNull]
    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public int Stock { get; set; }

    public bool IsFeatured { get; set; }

    [Ignore]
    public string CategoryName { get; set; } = string.Empty;

    [Ignore]
    public string ImageFile => $"product{ProductId}.jpg";
}
