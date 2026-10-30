using OfflineShoppingApp.Shopping.Services;
using OfflineShoppingApp.Shopping.Views;
using OfflineShoppingApp.Shopping.ViewModels;
using System.Globalization;
namespace OfflineShoppingApp;
public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("en-ZA");
        CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("en-ZA");
        var builder = MauiApp.CreateBuilder();
        builder.UseMauiApp<App>().ConfigureFonts(fonts =>
        {
            fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
        });
        builder.Services.AddSingleton<StoreDatabase>();
        builder.Services.AddTransient<CustomerViewModel>();
        builder.Services.AddTransient<CatalogueViewModel>();
        builder.Services.AddTransient<ProductViewModel>();
        builder.Services.AddTransient<CartViewModel>();
        builder.Services.AddTransient<CustomerPage>();
        builder.Services.AddTransient<CataloguePage>();
        builder.Services.AddTransient<ProductPage>();
        builder.Services.AddTransient<CartPage>();
        return builder.Build();
    }
}
