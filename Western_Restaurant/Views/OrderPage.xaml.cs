using Western_Restaurant.ViewModels;

namespace Western_Restaurant.Views;

public partial class OrderPage : ContentPage
{
    private readonly OrderViewModel _viewModel;

    public OrderPage(OrderViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.LoadOrderSummaryCommand.Execute(null);
    }
}
