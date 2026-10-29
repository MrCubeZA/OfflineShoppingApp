using OfflineShoppingApp.Shopping.ViewModels;

namespace OfflineShoppingApp.Shopping.Views;

public partial class CataloguePage : ContentPage, IQueryAttributable
{
    private readonly CatalogueViewModel viewModel;

    public CataloguePage(CatalogueViewModel viewModel)
    {
        InitializeComponent();
        this.viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        try
        {
            await viewModel.OnAppearingAsync();
        }
        catch (Exception exception)
        {
            System.Diagnostics.Debug.WriteLine(exception);
            await DisplayAlertAsync("Unable to load", "The local store could not load this page. Please go back and try again.", "OK");
        }
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        viewModel.ApplyQueryAttributes(query);
    }
}
