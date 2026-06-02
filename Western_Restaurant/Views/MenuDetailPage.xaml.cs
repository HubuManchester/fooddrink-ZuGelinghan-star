using Western_Restaurant.ViewModels;

namespace Western_Restaurant.Views;

public partial class MenuDetailPage : ContentPage
{
    public MenuDetailPage(MenuDetailViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
