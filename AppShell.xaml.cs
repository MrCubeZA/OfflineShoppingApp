using OfflineShoppingApp.Shopping.Views;
namespace OfflineShoppingApp;
public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        Routing.RegisterRoute("catalogue", typeof(CataloguePage));
        Routing.RegisterRoute("product", typeof(ProductPage));
        Routing.RegisterRoute("cart", typeof(CartPage));
    }
}
