using SQLite;
namespace OfflineShoppingApp.Shopping;

public class StoreDatabase
{
    private readonly SQLiteAsyncConnection connection;
    private readonly SemaphoreSlim initializationLock = new(1, 1);
    private bool initialized;

    public StoreDatabase()
    {
        string path = Path.Combine(FileSystem.Current.AppDataDirectory, "shopping.db");
        connection = new SQLiteAsyncConnection(path);
    }

    public async Task InitializeAsync()
    {
        await initializationLock.WaitAsync();
        try
        {
            if (initialized) return;
            string path = Path.Combine(FileSystem.Current.AppDataDirectory, "shopping.db");
            if (!File.Exists(path))
            {
                using Stream input = await FileSystem.Current.OpenAppPackageFileAsync("shopping.db");
                using FileStream output = File.Create(path);
                await input.CopyToAsync(output);
            }
            await connection.ExecuteAsync("PRAGMA foreign_keys = ON");
            initialized = true;
        }
        finally { initializationLock.Release(); }
    }

    public async Task<List<StoreCustomer>> GetCustomersAsync()
    {
        await InitializeAsync();
        return await connection.Table<StoreCustomer>().ToListAsync();
    }

    public async Task<List<Product>> GetProductsAsync()
    {
        await InitializeAsync();
        var products = await connection.Table<Product>().ToListAsync();
        var categories = await connection.Table<Category>().ToListAsync();
        foreach (var product in products)
        {
            foreach (var category in categories)
            {
                if (product.CategoryId == category.CategoryId)
                {
                    product.CategoryName = category.Name;
                    break;
                }
            }
        }
        return products;
    }

    public async Task<Product> GetProductAsync(int productId)
    {
        var products = await GetProductsAsync();
        foreach (var product in products)
        {
            if (product.ProductId == productId) return product;
        }
        throw new InvalidOperationException("This product could not be found.");
    }

    public async Task<List<CartLine>> GetCartAsync(int customerId)
    {
        await InitializeAsync();
        var items = await connection.Table<CartItem>().Where(item => item.CustomerId == customerId).ToListAsync();
        var products = await connection.Table<Product>().ToListAsync();
        var lines = new List<CartLine>();
        foreach (var item in items)
        {
            foreach (var product in products)
            {
                if (item.ProductId == product.ProductId)
                {
                    lines.Add(new CartLine { CartItemId = item.CartItemId, ProductId = product.ProductId, Name = product.Name, Quantity = item.Quantity, UnitPrice = product.Price });
                    break;
                }
            }
        }
        return lines;
    }

    public async Task AddToCartAsync(int customerId, int productId, int quantity)
    {
        if (quantity <= 0) throw new InvalidOperationException("Enter a whole number greater than zero.");
        await InitializeAsync();
        await connection.RunInTransactionAsync(db =>
        {
            if (db.Find<StoreCustomer>(customerId) == null) throw new InvalidOperationException("Select a customer first.");
            var product = db.Find<Product>(productId) ?? throw new InvalidOperationException("This product could not be found.");
            var existing = db.Table<CartItem>().Where(item => item.CustomerId == customerId && item.ProductId == productId).FirstOrDefault();
            int alreadyInCart = existing == null ? 0 : existing.Quantity;
            if (quantity > product.Stock - alreadyInCart)
                throw new InvalidOperationException($"Only {Math.Max(0, product.Stock - alreadyInCart)} more can be added. Your cart already contains {alreadyInCart}.");
            if (existing == null) db.Insert(new CartItem { CustomerId = customerId, ProductId = productId, Quantity = quantity });
            else
            {
                existing.Quantity += quantity;
                db.Update(existing);
            }
        });
    }

    public async Task RemoveCartItemAsync(int customerId, int cartItemId)
    {
        await InitializeAsync();
        await connection.RunInTransactionAsync(db =>
        {
            var item = db.Find<CartItem>(cartItemId);
            if (item != null && item.CustomerId == customerId) db.Delete(item);
        });
    }

    public async Task<StoreOrder> CheckoutAsync(int customerId)
    {
        await InitializeAsync();
        var order = new StoreOrder { CustomerId = customerId, OrderDate = DateTime.Now };
        await connection.RunInTransactionAsync(db =>
        {
            if (db.Find<StoreCustomer>(customerId) == null) throw new InvalidOperationException("Select a customer first.");
            var cart = db.Table<CartItem>().Where(item => item.CustomerId == customerId).ToList();
            if (cart.Count == 0) throw new InvalidOperationException("Your cart is empty. Add an item before checking out.");
            var orderItems = new List<OrderItem>();
            var products = new List<Product>();
            foreach (var item in cart)
            {
                var product = db.Find<Product>(item.ProductId) ?? throw new InvalidOperationException("A product in your cart is no longer available.");
                if (item.Quantity <= 0 || item.Quantity > product.Stock)
                    throw new InvalidOperationException($"There is not enough stock for {product.Name}. Remove it from your cart and choose a smaller quantity.");
                order.Total += product.Price * item.Quantity;
                orderItems.Add(new OrderItem { ProductId = product.ProductId, Quantity = item.Quantity, UnitPrice = product.Price });
                product.Stock -= item.Quantity;
                products.Add(product);
            }
            db.Insert(order);
            foreach (var item in orderItems)
            {
                item.OrderId = order.OrderId;
                db.Insert(item);
            }
            foreach (var product in products) db.Update(product);
            foreach (var item in cart) db.Delete(item);
        });
        return order;
    }
}
