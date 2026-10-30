using OfflineShoppingApp.Shopping.Models;
using SQLite;

namespace OfflineShoppingApp.Shopping.Services;

public class StoreDatabase
{
    private string dbFileName = "shopping.db";
    private string localDbPath = string.Empty;
    private SQLiteAsyncConnection? dbConnection;

    public StoreDatabase()
    {
        localDbPath = Path.Combine(FileSystem.Current.AppDataDirectory, dbFileName);
    }

    private async Task<SQLiteAsyncConnection> GetDbConnectionAsync()
    {
        if (dbConnection != null)
        {
            return dbConnection;
        }

        if (!File.Exists(localDbPath))
        {
            using Stream inputStream = await FileSystem.Current.OpenAppPackageFileAsync(dbFileName);
            using FileStream outputStream = File.Create(localDbPath);
            await inputStream.CopyToAsync(outputStream);
        }

        dbConnection = new SQLiteAsyncConnection(localDbPath);
        return dbConnection;
    }

    public async Task<List<Customer>> GetCustomersAsync()
    {
        SQLiteAsyncConnection connection = await GetDbConnectionAsync();
        return await connection.Table<Customer>().ToListAsync();
    }

    public async Task<List<Product>> GetProductsAsync()
    {
        SQLiteAsyncConnection connection = await GetDbConnectionAsync();
        List<Product> products = await connection.Table<Product>().ToListAsync();
        List<Category> categories = await connection.Table<Category>().ToListAsync();

        foreach (Product product in products)
        {
            foreach (Category category in categories)
            {
                if (product.CategoryId == category.CategoryId)
                {
                    product.CategoryName = category.Name;
                }
            }
        }

        return products;
    }

    public async Task<Product?> GetProductAsync(int productId)
    {
        List<Product> products = await GetProductsAsync();

        foreach (Product product in products)
        {
            if (product.ProductId == productId)
            {
                return product;
            }
        }

        return null;
    }

    public async Task<List<CartLine>> GetCartAsync(int customerId)
    {
        SQLiteAsyncConnection connection = await GetDbConnectionAsync();
        List<CartItem> cartItems = await connection.Table<CartItem>()
            .Where(x => x.CustomerId == customerId)
            .ToListAsync();
        List<Product> products = await connection.Table<Product>().ToListAsync();

        List<CartLine> lines = new List<CartLine>();
        foreach (CartItem cartItem in cartItems)
        {
            foreach (Product product in products)
            {
                if (cartItem.ProductId == product.ProductId)
                {
                    CartLine line = new CartLine();
                    line.CartItemId = cartItem.CartItemId;
                    line.ProductId = product.ProductId;
                    line.Name = product.Name;
                    line.Quantity = cartItem.Quantity;
                    line.UnitPrice = product.Price;
                    lines.Add(line);
                }
            }
        }

        return lines;
    }

    public async Task AddToCartAsync(int customerId, int productId, int quantity)
    {
        if (quantity <= 0)
        {
            throw new InvalidOperationException("Please enter a whole number bigger than 0");
        }

        SQLiteAsyncConnection connection = await GetDbConnectionAsync();

        Product product = await connection.Table<Product>()
            .Where(x => x.ProductId == productId)
            .FirstOrDefaultAsync();

        if (product == null)
        {
            throw new InvalidOperationException("Could not find this product");
        }

        if (product.Stock == 0)
        {
            throw new InvalidOperationException("Sorry, this item is out of stock");
        }

        CartItem cartItem = await connection.Table<CartItem>()
            .Where(x => x.CustomerId == customerId && x.ProductId == productId)
            .FirstOrDefaultAsync();

        int alreadyInCart = 0;
        if (cartItem != null)
        {
            alreadyInCart = cartItem.Quantity;
        }

        int canStillAdd = product.Stock - alreadyInCart;

        if (canStillAdd <= 0)
        {
            throw new InvalidOperationException($"You already have all {product.Stock} of these in your cart");
        }

        if (quantity > canStillAdd)
        {
            throw new InvalidOperationException($"You can only add {canStillAdd} more because you already have {alreadyInCart} in your cart");
        }

        if (cartItem == null)
        {
            CartItem newCartItem = new CartItem();
            newCartItem.CustomerId = customerId;
            newCartItem.ProductId = productId;
            newCartItem.Quantity = quantity;
            await connection.InsertAsync(newCartItem);
        }
        else
        {
            cartItem.Quantity += quantity;
            await connection.UpdateAsync(cartItem);
        }
    }

    public async Task RemoveCartItemAsync(int cartItemId)
    {
        SQLiteAsyncConnection connection = await GetDbConnectionAsync();
        await connection.DeleteAsync<CartItem>(cartItemId);
    }

    public async Task<Order> CheckoutAsync(int customerId)
    {
        SQLiteAsyncConnection connection = await GetDbConnectionAsync();
        List<CartItem> cartItems = await connection.Table<CartItem>()
            .Where(x => x.CustomerId == customerId)
            .ToListAsync();

        if (cartItems.Count == 0)
        {
            throw new InvalidOperationException("Your cart is empty, add something first");
        }

        List<Product> products = new List<Product>();
        decimal total = 0;

        foreach (CartItem cartItem in cartItems)
        {
            Product product = await connection.Table<Product>()
                .Where(x => x.ProductId == cartItem.ProductId)
                .FirstOrDefaultAsync();

            if (product == null)
            {
                throw new InvalidOperationException("Something in your cart does not exist anymore");
            }

            if (cartItem.Quantity > product.Stock)
            {
                throw new InvalidOperationException($"There is not enough {product.Name} in stock anymore, please remove it from your cart");
            }

            total += product.Price * cartItem.Quantity;
            products.Add(product);
        }

        Order order = new Order();
        order.CustomerId = customerId;
        order.OrderDate = DateTime.Now;
        order.Total = total;
        await connection.InsertAsync(order);

        for (int i = 0; i < cartItems.Count; i++)
        {
            CartItem cartItem = cartItems[i];
            Product product = products[i];

            OrderItem orderItem = new OrderItem();
            orderItem.OrderId = order.OrderId;
            orderItem.ProductId = product.ProductId;
            orderItem.Quantity = cartItem.Quantity;
            orderItem.UnitPrice = product.Price;
            await connection.InsertAsync(orderItem);

            product.Stock -= cartItem.Quantity;
            await connection.UpdateAsync(product);

            await connection.DeleteAsync(cartItem);
        }

        return order;
    }
}
