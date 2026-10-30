using OfflineShoppingApp.Shopping.Commands;
using OfflineShoppingApp.Shopping.Models;
using OfflineShoppingApp.Shopping.Services;

namespace OfflineShoppingApp.Shopping.ViewModels;

public class ProductViewModel : BaseViewModel, IQueryAttributable
{
    private readonly StoreDatabase database;
    private Customer? customer;
    private int productId;

    public AsyncCommand AddCommand { get; }

    private Product? product;

    public Product? Product
    {
        get { return product; }
        set
        {
            if (product == value) return;
            product = value;
            OnPropertyChanged();
        }
    }

    private string quantity = "1";

    public string Quantity
    {
        get { return quantity; }
        set
        {
            if (quantity == value) return;
            quantity = value;
            OnPropertyChanged();
        }
    }

    private string feedback = string.Empty;

    public string Feedback
    {
        get { return feedback; }
        set
        {
            if (feedback == value) return;
            feedback = value;
            OnPropertyChanged();
        }
    }

    public ProductViewModel(StoreDatabase database)
    {
        this.database = database;
        AddCommand = new AsyncCommand(AddToCartAsync);
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("Customer", out var value))
        {
            customer = value as Customer;
        }

        if (query.TryGetValue("ProductId", out var id))
        {
            productId = (int)id;
        }
    }

    public override async Task OnAppearingAsync()
    {
        try
        {
            Product = await database.GetProductAsync(productId);
        }
        catch (Exception exception)
        {
            System.Diagnostics.Debug.WriteLine(exception);
            await Shell.Current.DisplayAlertAsync("Error", "Something went wrong loading this product", "OK");
        }
    }

    private async Task AddToCartAsync()
    {
        if (customer == null || Product == null)
        {
            return;
        }

        int amount;
        if (!int.TryParse(Quantity, out amount) || amount <= 0)
        {
            Feedback = "Please enter a whole number bigger than 0";
            return;
        }

        try
        {
            await database.AddToCartAsync(customer.CustomerId, Product.ProductId, amount);
            Feedback = $"Added {amount} to your cart";
            await Shell.Current.DisplayAlertAsync("Added", $"Added {amount} x {Product.Name} to your cart", "OK");
        }
        catch (InvalidOperationException exception)
        {
            Feedback = exception.Message;
        }
        catch (Exception exception)
        {
            System.Diagnostics.Debug.WriteLine(exception);
            Feedback = "Something went wrong, could not add it to your cart";
        }
    }
}
