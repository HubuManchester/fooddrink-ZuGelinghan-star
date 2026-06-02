using Western_Restaurant.Services;

namespace Western_Restaurant.Views;

public partial class CameraPage : ContentPage
{
    private readonly IHardwareService _hardwareService;
    private string? _currentPhotoPath;

    public CameraPage(IHardwareService hardwareService)
    {
        InitializeComponent();
        _hardwareService = hardwareService;
        StatusLabel.Text = hardwareService.IsCameraAvailable
            ? "Camera is available — tap a button to test"
            : "Camera is NOT available on this device";
    }

    private async void OnTakePhotoClicked(object? sender, EventArgs e)
    {
        try
        {
            StatusLabel.Text = "Opening camera...";
            var path = await _hardwareService.TakePhotoAsync();
            if (!string.IsNullOrWhiteSpace(path))
            {
                _currentPhotoPath = path;
                PhotoImage.Source = ImageSource.FromFile(path);
                PhotoImage.IsVisible = true;
                PlaceholderView.IsVisible = false;
                ClearButton.IsVisible = true;
                StatusLabel.Text = "Photo captured successfully!";
                _hardwareService.Vibrate();
            }
            else
            {
                StatusLabel.Text = "Photo capture cancelled or failed.";
                await DisplayAlert("Camera", "Could not capture photo. Check permissions.", "OK");
            }
        }
        catch (Exception ex)
        {
            StatusLabel.Text = $"Error: {ex.Message}";
            await DisplayAlert("Error", $"Camera error: {ex.Message}", "OK");
        }
    }

    private async void OnPickPhotoClicked(object? sender, EventArgs e)
    {
        try
        {
            StatusLabel.Text = "Opening gallery...";
            var path = await _hardwareService.PickPhotoAsync();
            if (!string.IsNullOrWhiteSpace(path))
            {
                _currentPhotoPath = path;
                PhotoImage.Source = ImageSource.FromFile(path);
                PhotoImage.IsVisible = true;
                PlaceholderView.IsVisible = false;
                ClearButton.IsVisible = true;
                StatusLabel.Text = "Photo selected from gallery.";
                _hardwareService.Vibrate();
            }
            else
            {
                StatusLabel.Text = "No photo selected.";
            }
        }
        catch (Exception ex)
        {
            StatusLabel.Text = $"Error: {ex.Message}";
            await DisplayAlert("Error", $"Gallery error: {ex.Message}", "OK");
        }
    }

    private void OnClearPhotoClicked(object? sender, EventArgs e)
    {
        _currentPhotoPath = null;
        PhotoImage.Source = null;
        PhotoImage.IsVisible = false;
        PlaceholderView.IsVisible = true;
        ClearButton.IsVisible = false;
        StatusLabel.Text = "Ready";
    }
}
