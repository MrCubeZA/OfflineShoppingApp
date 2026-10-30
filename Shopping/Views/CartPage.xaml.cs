using OfflineShoppingApp.Shopping.ViewModels;

namespace OfflineShoppingApp.Shopping.Views;

public partial class CartPage : ContentPage
{
    private readonly CartViewModel viewModel;

    public CartPage(CartViewModel viewModel)
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
