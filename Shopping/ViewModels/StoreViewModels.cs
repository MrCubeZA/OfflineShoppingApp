using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace OfflineShoppingApp.Shopping.ViewModels;

public abstract class BaseViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    public virtual Task OnAppearingAsync() => Task.CompletedTask;
    protected Task AlertAsync(string message) => Shell.Current.DisplayAlertAsync("Offline Store", message, "OK");
}

public class AsyncCommand : ICommand
{
    private readonly Func<object?, Task> action;
    private bool running;
    public AsyncCommand(Func<object?, Task> action) { this.action = action; }
    public event EventHandler? CanExecuteChanged;
    public bool CanExecute(object? parameter) => !running;
    public async void Execute(object? parameter)
    {
        if (running) return;
        running = true;
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        try { await action(parameter); }
        catch (Exception exception)
        {
            await Shell.Current.DisplayAlertAsync("Unable to complete action", exception.Message, "OK");
        }
        finally
        {
            running = false;
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}

public class CustomerViewModel : BaseViewModel
{
    private readonly StoreDatabase database;
    public ObservableCollection<StoreCustomer> Customers { get; } = new();
    private StoreCustomer? selectedCustomer;
    public StoreCustomer? SelectedCustomer
    {
        get => selectedCustomer;
        set { selectedCustomer = value; OnPropertyChanged(); }
    }
    public AsyncCommand SelectCustomerCommand { get; }
    public CustomerViewModel(StoreDatabase database)
    {
        this.database = database;
        SelectCustomerCommand = new AsyncCommand(async _ =>
        {
            var customer = SelectedCustomer;
            if (customer == null) return;
            SelectedCustomer = null;
            await Shell.Current.GoToAsync("catalogue", new ShellNavigationQueryParameters { { "Customer", customer } });
        });
    }
    public override async Task OnAppearingAsync()
    {
        var customers = await database.GetCustomersAsync();
        Customers.Clear();
        foreach (var customer in customers) Customers.Add(customer);
    }
}

public class CatalogueViewModel : BaseViewModel, IQueryAttributable
{
    private readonly StoreDatabase database;
    private StoreCustomer? customer;
    public string CustomerName => customer?.Name ?? "Customer";
    private int cartCount;
    public int CartCount { get => cartCount; private set { cartCount = value; OnPropertyChanged(); } }
    public ObservableCollection<Product> Products { get; } = new();
    private List<Product> allProducts = new();
    private string searchText = string.Empty;
    public string SearchText
    {
        get => searchText;
        set
        {
            string newText = value ?? string.Empty;
            if (searchText == newText) return;
            searchText = newText;
            OnPropertyChanged();
            FilterProducts();
        }
    }
    private Product? featuredProduct;
    public Product? FeaturedProduct
    {
        get => featuredProduct;
        private set { featuredProduct = value; OnPropertyChanged(); }
    }
    public AsyncCommand OpenFeaturedCommand { get; }
    private void FilterProducts()
    {
        string search = SearchText.Trim().ToLower();
        Products.Clear();
        foreach (var product in allProducts)
        {
            if (product.Name.ToLower().Contains(search) || product.CategoryName.ToLower().Contains(search))
                Products.Add(product);
        }
    }
    private Product? selectedProduct;
    public Product? SelectedProduct { get => selectedProduct; set { selectedProduct = value; OnPropertyChanged(); } }
    public AsyncCommand OpenProductCommand { get; }
    public AsyncCommand OpenCartCommand { get; }
    public AsyncCommand ChangeCustomerCommand { get; }
    public CatalogueViewModel(StoreDatabase database)
    {
        this.database = database;
        OpenProductCommand = new AsyncCommand(async _ =>
        {
            var product = SelectedProduct;
            if (product == null || customer == null) return;
            SelectedProduct = null;
            await Shell.Current.GoToAsync("product", new ShellNavigationQueryParameters
                { { "Customer", customer }, { "ProductId", product.ProductId } });
        });
        OpenFeaturedCommand = new AsyncCommand(async _ =>
        {
            if (FeaturedProduct == null || customer == null) return;
            await Shell.Current.GoToAsync("product", new ShellNavigationQueryParameters
                { { "Customer", customer }, { "ProductId", FeaturedProduct.ProductId } });
        });
        OpenCartCommand = new AsyncCommand(async _ =>
        {
            if (customer != null)
                await Shell.Current.GoToAsync("cart", new ShellNavigationQueryParameters { { "Customer", customer } });
        });
        ChangeCustomerCommand = new AsyncCommand(_ => Shell.Current.GoToAsync(".."));
    }
    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("Customer", out var value)) customer = (StoreCustomer)value;
        OnPropertyChanged(nameof(CustomerName));
    }
    public override async Task OnAppearingAsync()
    {
        if (customer == null) return;
        var products = await database.GetProductsAsync();
        allProducts = products;
        FeaturedProduct = null;
        foreach (var product in products)
        {
            if (product.IsFeatured) { FeaturedProduct = product; break; }
        }
        FilterProducts();
        var lines = await database.GetCartAsync(customer.CustomerId);
        int count = 0;
        foreach (var line in lines) count += line.Quantity;
        CartCount = count;
    }
}

public class ProductViewModel : BaseViewModel, IQueryAttributable
{
    private readonly StoreDatabase database;
    private StoreCustomer? customer;
    private int productId;
    private Product? product;
    public Product? Product { get => product; private set { product = value; OnPropertyChanged(); } }
    private string quantity = "1";
    public string Quantity { get => quantity; set { quantity = value; OnPropertyChanged(); } }
    private string feedback = "";
    public string Feedback { get => feedback; private set { feedback = value; OnPropertyChanged(); } }
    public AsyncCommand AddCommand { get; }
    public ProductViewModel(StoreDatabase database)
    {
        this.database = database;
        AddCommand = new AsyncCommand(async _ =>
        {
            if (customer == null || Product == null) return;
            if (!int.TryParse(Quantity, out int amount) || amount <= 0)
            {
                Feedback = "Enter a whole number greater than zero.";
                return;
            }
            try
            {
                await database.AddToCartAsync(customer.CustomerId, productId, amount);
                Feedback = $"Added {amount} to your cart.";
                await AlertAsync(Feedback);
            }
            catch (InvalidOperationException exception) { Feedback = exception.Message; }
        });
    }
    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("Customer", out var value)) customer = (StoreCustomer)value;
        if (query.TryGetValue("ProductId", out var id)) productId = (int)id;
    }
    public override async Task OnAppearingAsync() { Product = await database.GetProductAsync(productId); }
}

public class CartViewModel : BaseViewModel, IQueryAttributable
{
    private readonly StoreDatabase database;
    private StoreCustomer? customer;
    public ObservableCollection<CartLine> Lines { get; } = new();
    private decimal total;
    public decimal Total { get => total; private set { total = value; OnPropertyChanged(); } }
    private string feedback = "";
    public string Feedback { get => feedback; private set { feedback = value; OnPropertyChanged(); } }
    public AsyncCommand RemoveCommand { get; }
    public AsyncCommand CheckoutCommand { get; }
    public CartViewModel(StoreDatabase database)
    {
        this.database = database;
        RemoveCommand = new AsyncCommand(async parameter =>
        {
            if (customer == null || parameter is not CartLine line) return;
            await database.RemoveCartItemAsync(customer.CustomerId, line.CartItemId);
            await OnAppearingAsync();
        });
        CheckoutCommand = new AsyncCommand(async _ =>
        {
            if (customer == null) return;
            if (Lines.Count == 0) { Feedback = "Your cart is empty. Add a product first."; return; }
            bool confirmed = await Shell.Current.DisplayAlertAsync("Checkout", $"Place this order for {Total:C2}?", "Place order", "Cancel");
            if (!confirmed) return;
            try
            {
                var order = await database.CheckoutAsync(customer.CustomerId);
                await OnAppearingAsync();
                await AlertAsync($"Order #{order.OrderId} placed. Total: {order.Total:C2}");
                await Shell.Current.GoToAsync("..");
            }
            catch (InvalidOperationException exception) { Feedback = exception.Message; await OnAppearingAsync(); }
        });
    }
    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("Customer", out var value)) customer = (StoreCustomer)value;
    }
    public override async Task OnAppearingAsync()
    {
        if (customer == null) return;
        var lines = await database.GetCartAsync(customer.CustomerId);
        Lines.Clear();
        decimal sum = 0;
        foreach (var line in lines) { Lines.Add(line); sum += line.LineTotal; }
        Total = sum;
    }
}
