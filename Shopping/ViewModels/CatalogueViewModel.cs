using OfflineShoppingApp.Shopping.Commands;
using OfflineShoppingApp.Shopping.Models;
using OfflineShoppingApp.Shopping.Services;
using System.Collections.ObjectModel;

namespace OfflineShoppingApp.Shopping.ViewModels;

public class CatalogueViewModel : BaseViewModel, IQueryAttributable
{
    private readonly StoreDatabase database;

    private List<Product> allProducts = new();

    public AsyncCommand OpenProductCommand { get; }
    public AsyncCommand OpenFeaturedCommand { get; }
    public AsyncCommand OpenCartCommand { get; }
    public AsyncCommand ChangeCustomerCommand { get; }

    private Customer? customer;

    public Customer? Customer
    {
        get { return customer; }
        set
        {
            if (customer == value) return;
            customer = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(CustomerName));
        }
    }

    public string CustomerName => Customer == null ? string.Empty : Customer.Name;

    private int cartCount;

    public int CartCount
    {
        get { return cartCount; }
        set
        {
            if (cartCount == value) return;
            cartCount = value;
            OnPropertyChanged();
        }
    }

    private ObservableCollection<Product> products = new();

    public ObservableCollection<Product> Products
    {
        get { return products; }
        set
        {
            if (products == value) return;
            products = value;
            OnPropertyChanged();
        }
    }

    private Product? selectedProduct;

    public Product? SelectedProduct
    {
        get { return selectedProduct; }
        set
        {
            if (selectedProduct == value) return;
            selectedProduct = value;
            OnPropertyChanged();
        }
    }

    private Product? featuredProduct;

    public Product? FeaturedProduct
    {
        get { return featuredProduct; }
        set
        {
            if (featuredProduct == value) return;
            featuredProduct = value;
            OnPropertyChanged();
        }
    }

    private string searchText = string.Empty;

    public string SearchText
    {
        get { return searchText; }
        set
        {
            if (searchText == value) return;
            searchText = value;
            OnPropertyChanged();
            FilterProducts();
        }
    }

    public CatalogueViewModel(StoreDatabase database)
    {
        this.database = database;
        OpenProductCommand = new AsyncCommand(OpenProductAsync);
        OpenFeaturedCommand = new AsyncCommand(OpenFeaturedAsync);
        OpenCartCommand = new AsyncCommand(OpenCartAsync);
        ChangeCustomerCommand = new AsyncCommand(ChangeCustomerAsync);
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("Customer", out var value))
        {
            Customer = value as Customer;
        }
    }

    public override async Task OnAppearingAsync()
    {
        if (Customer == null)
        {
            return;
        }

        try
        {
            allProducts = await database.GetProductsAsync();

            FeaturedProduct = null;
            foreach (Product product in allProducts)
            {
                if (product.IsFeatured)
                {
                    FeaturedProduct = product;
                }
            }

            FilterProducts();

            List<CartLine> cartLines = await database.GetCartAsync(Customer.CustomerId);
            int count = 0;
            foreach (CartLine line in cartLines)
            {
                count += line.Quantity;
            }
            CartCount = count;
        }
        catch (Exception exception)
        {
            System.Diagnostics.Debug.WriteLine(exception);
            await Shell.Current.DisplayAlertAsync("Error", "Something went wrong loading the products", "OK");
        }
    }

    private void FilterProducts()
    {
        string search = SearchText.Trim().ToLower();
        ObservableCollection<Product> results = new ObservableCollection<Product>();

        foreach (Product product in allProducts)
        {
            if (product.Name.ToLower().Contains(search) || product.CategoryName.ToLower().Contains(search))
            {
                results.Add(product);
            }
        }

        Products = results;
    }

    private async Task OpenProductAsync()
    {
        Product? product = SelectedProduct;
        SelectedProduct = null;
        if (product == null)
        {
            return;
        }

        await GoToProductAsync(product);
    }

    private async Task OpenFeaturedAsync()
    {
        if (FeaturedProduct == null)
        {
            return;
        }

        await GoToProductAsync(FeaturedProduct);
    }

    private async Task GoToProductAsync(Product product)
    {
        if (Customer == null)
        {
            return;
        }

        await Shell.Current.GoToAsync("product", new Dictionary<string, object>
        {
            { "Customer", Customer },
            { "ProductId", product.ProductId }
        });
    }

    private async Task OpenCartAsync()
    {
        if (Customer == null)
        {
            return;
        }

        await Shell.Current.GoToAsync("cart", new Dictionary<string, object>
        {
            { "Customer", Customer }
        });
    }

    private async Task ChangeCustomerAsync()
    {
        await Shell.Current.GoToAsync("..");
    }
}
