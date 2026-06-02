using Western_Restaurant.ViewModels;

namespace Western_Restaurant.Views;

public partial class CartPage : ContentPage
{
    public CartPage(CartViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    private async void OnBrowseMenuClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//menu");
    }
}
