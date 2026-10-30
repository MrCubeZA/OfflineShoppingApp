using OfflineShoppingApp.Shopping.Commands;
using OfflineShoppingApp.Shopping.Models;
using OfflineShoppingApp.Shopping.Services;
using System.Collections.ObjectModel;

namespace OfflineShoppingApp.Shopping.ViewModels;

public class CartViewModel : BaseViewModel, IQueryAttributable
{
    private readonly StoreDatabase database;
    private Customer? customer;

    public AsyncCommand RemoveCommand { get; }
    public AsyncCommand CheckoutCommand { get; }

    private ObservableCollection<CartLine> lines = new();

    public ObservableCollection<CartLine> Lines
    {
        get { return lines; }
        set
        {
            if (lines == value) return;
            lines = value;
            OnPropertyChanged();
        }
    }

    private CartLine? selectedLine;

    public CartLine? SelectedLine
    {
        get { return selectedLine; }
        set
        {
            if (selectedLine == value) return;
            selectedLine = value;
            OnPropertyChanged();
        }
    }

    private decimal total;

    public decimal Total
    {
        get { return total; }
        set
        {
            if (total == value) return;
            total = value;
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

    public CartViewModel(StoreDatabase database)
    {
        this.database = database;
        RemoveCommand = new AsyncCommand(RemoveItemAsync);
        CheckoutCommand = new AsyncCommand(CheckoutAsync);
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("Customer", out var value))
        {
            customer = value as Customer;
        }
    }

    public override async Task OnAppearingAsync()
    {
        await LoadCartAsync();
    }

    private async Task LoadCartAsync()
    {
        if (customer == null)
        {
            return;
        }

        try
        {
            List<CartLine> cartLines = await database.GetCartAsync(customer.CustomerId);
            Lines = new ObservableCollection<CartLine>(cartLines);

            decimal sum = 0;
            foreach (CartLine line in cartLines)
            {
                sum += line.LineTotal;
            }
            Total = sum;
        }
        catch (Exception exception)
        {
            System.Diagnostics.Debug.WriteLine(exception);
            await Shell.Current.DisplayAlertAsync("Error", "Something went wrong loading your cart", "OK");
        }
    }

    private async Task RemoveItemAsync()
    {
        CartLine? line = SelectedLine;
        SelectedLine = null;
        if (line == null)
        {
            return;
        }

        bool remove = await Shell.Current.DisplayAlertAsync("Remove item", $"Do you want to remove {line.Name} from your cart?", "Remove", "Cancel");
        if (!remove)
        {
            return;
        }

        try
        {
            await database.RemoveCartItemAsync(line.CartItemId);
            Feedback = string.Empty;
            await LoadCartAsync();
        }
        catch (Exception exception)
        {
            System.Diagnostics.Debug.WriteLine(exception);
            Feedback = "Something went wrong, could not remove the item";
        }
    }

    private async Task CheckoutAsync()
    {
        if (customer == null)
        {
            return;
        }

        if (Lines.Count == 0)
        {
            Feedback = "Your cart is empty, add something first";
            return;
        }

        bool confirmed = await Shell.Current.DisplayAlertAsync("Checkout", $"Are you sure you want to place this order for {Total:C2}?", "Yes", "Cancel");
        if (!confirmed)
        {
            return;
        }

        try
        {
            Order order = await database.CheckoutAsync(customer.CustomerId);
            Feedback = string.Empty;
            await Shell.Current.DisplayAlertAsync("Order placed", $"Your order was placed! Order number: {order.OrderId}, Total: {order.Total:C2}", "OK");

            await Shell.Current.GoToAsync("..");
        }
        catch (InvalidOperationException exception)
        {
            Feedback = exception.Message;
            await LoadCartAsync();
        }
        catch (Exception exception)
        {
            System.Diagnostics.Debug.WriteLine(exception);
            Feedback = "Something went wrong, the order was not placed";
        }
    }
}
