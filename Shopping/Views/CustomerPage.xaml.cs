using OfflineShoppingApp.Shopping.ViewModels;

namespace OfflineShoppingApp.Shopping.Views;

public partial class CustomerPage : ContentPage
{
    private readonly CustomerViewModel viewModel;

    public CustomerPage(CustomerViewModel viewModel)
    {
        InitializeComponent();
        this.viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await viewModel.OnAppearingAsync();
    }
}
