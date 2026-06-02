using Western_Restaurant.Views;

namespace Western_Restaurant;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        Routing.RegisterRoute("detail", typeof(MenuDetailPage));
        Routing.RegisterRoute("order", typeof(OrderPage));
        Routing.RegisterRoute("camera", typeof(CameraPage));
        Routing.RegisterRoute("help", typeof(HelpPage));
    }
}
