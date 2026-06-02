using Western_Restaurant.ViewModels;

namespace Western_Restaurant.Views;

public partial class CommentsPage : ContentPage
{
    public CommentsPage(CommentsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
