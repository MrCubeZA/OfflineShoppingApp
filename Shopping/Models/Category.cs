using SQLite;

namespace OfflineShoppingApp.Shopping.Models;

[Table("Category")]
public class Category
{
    [PrimaryKey, AutoIncrement]
    public int CategoryId { get; set; }

    [NotNull]
    public string Name { get; set; } = string.Empty;
}
