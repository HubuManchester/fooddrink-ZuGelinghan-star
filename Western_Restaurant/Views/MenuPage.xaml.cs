using Western_Restaurant.ViewModels;

namespace Western_Restaurant.Views;

public partial class MenuPage : ContentPage
{
    private readonly MenuViewModel _viewModel;

    public MenuPage(MenuViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (_viewModel.MenuItems.Count == 0)
            _viewModel.LoadMenuCommand.Execute(null);
        _viewModel.EnableShakeRecommendationCommand.Execute(null);
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _viewModel.DisableShakeRecommendationCommand.Execute(null);
    }

    private async void OnHelpClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("help");
    }
}
