using OfflineShoppingApp.Shopping.Commands;
using OfflineShoppingApp.Shopping.Models;
using OfflineShoppingApp.Shopping.Services;
using System.Collections.ObjectModel;

namespace OfflineShoppingApp.Shopping.ViewModels;

public class CustomerViewModel : BaseViewModel
{
    private readonly StoreDatabase database;

    public AsyncCommand SelectCustomerCommand { get; }

    private ObservableCollection<Customer> customers = new();

    public ObservableCollection<Customer> Customers
    {
        get { return customers; }
        set
        {
            if (customers == value) return;
            customers = value;
            OnPropertyChanged();
        }
    }

    private Customer? selectedCustomer;

    public Customer? SelectedCustomer
    {
        get { return selectedCustomer; }
        set
        {
            if (selectedCustomer == value) return;
            selectedCustomer = value;
            OnPropertyChanged();
        }
    }

    public CustomerViewModel(StoreDatabase database)
    {
        this.database = database;
        SelectCustomerCommand = new AsyncCommand(SelectCustomerAsync);
    }

    public override async Task OnAppearingAsync()
    {
        try
        {
            List<Customer> customerList = await database.GetCustomersAsync();
            Customers = new ObservableCollection<Customer>(customerList);
        }
        catch (Exception exception)
        {
            System.Diagnostics.Debug.WriteLine(exception);
            await Shell.Current.DisplayAlertAsync("Error", "Something went wrong loading the customers", "OK");
        }
    }

    private async Task SelectCustomerAsync()
    {
        Customer? customer = SelectedCustomer;
        SelectedCustomer = null;
        if (customer == null)
        {
            return;
        }

        await Shell.Current.GoToAsync("catalogue", new Dictionary<string, object>
        {
            { "Customer", customer }
        });
    }
}
