using CommunityToolkit.Mvvm.ComponentModel;

namespace Western_Restaurant.ViewModels;

public partial class BaseViewModel : ObservableObject
{
    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string _title = string.Empty;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _hasError;

    protected async Task ExecuteSafelyAsync(Func<Task> operation, string errorContext = "")
    {
        try
        {
            IsBusy = true;
            HasError = false;
            ErrorMessage = string.Empty;
            await operation();
        }
        catch (Exception ex)
        {
            HasError = true;
            ErrorMessage = string.IsNullOrWhiteSpace(errorContext)
                ? $"Error: {ex.Message}"
                : $"{errorContext}: {ex.Message}";
            System.Diagnostics.Debug.WriteLine(ErrorMessage);
        }
        finally
        {
            IsBusy = false;
        }
    }
}
