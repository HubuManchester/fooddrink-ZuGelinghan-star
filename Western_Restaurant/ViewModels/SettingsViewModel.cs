using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Western_Restaurant.Services;

namespace Western_Restaurant.ViewModels;

public partial class SettingsViewModel : BaseViewModel
{
    private readonly IHardwareService _hardwareService;

    [ObservableProperty] private bool _isDarkMode;
    [ObservableProperty] private double _fontSizeScale = 1.0;
    [ObservableProperty] private string _fontSizeLabel = "Normal (100%)";
    [ObservableProperty] private string _dataSourceLabel = "Loading...";
    [ObservableProperty] private string _apiUrlLabel = string.Empty;
    [ObservableProperty] private bool _isCameraAvailable;
    [ObservableProperty] private bool _isAccelerometerAvailable;
    [ObservableProperty] private string _appVersion = "v1.0.0";

    private readonly string[] _fontSizeLabels = { "Small (80%)", "Normal (100%)", "Large (120%)", "Extra Large (140%)", "Huge (160%)" };
    private readonly double[] _fontSizeValues = { 0.8, 1.0, 1.2, 1.4, 1.6 };

    public SettingsViewModel(IHardwareService hardwareService, IApiService apiService)
    {
        _hardwareService = hardwareService;
        Title = "Settings";
        LoadPreferences();
        CheckHardwareCapabilities();
        RefreshDataSourceInfo(apiService);
    }

    private void LoadPreferences()
    {
        try
        {
            IsDarkMode = Preferences.Get(Helpers.Constants.DarkModeKey, false);
            FontSizeScale = Preferences.Get(Helpers.Constants.FontSizeKey, 1.0);
            UpdateFontSizeLabel();
        }
        catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"Load preferences error: {ex.Message}"); }
    }

    private void CheckHardwareCapabilities()
    {
        IsCameraAvailable = _hardwareService.IsCameraAvailable;
        IsAccelerometerAvailable = _hardwareService.IsAccelerometerAvailable;
    }

    partial void OnIsDarkModeChanged(bool value)
    {
        try
        {
            Preferences.Set(Helpers.Constants.DarkModeKey, value);
            if (Application.Current != null)
                Application.Current.UserAppTheme = value ? AppTheme.Dark : AppTheme.Light;
            _hardwareService.Vibrate();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Theme change error: {ex.Message}");
        }
    }

    [RelayCommand]
    private void IncreaseFontSize()
    {
        var idx = Array.IndexOf(_fontSizeValues, FontSizeScale);
        if (idx < _fontSizeValues.Length - 1) { FontSizeScale = _fontSizeValues[idx + 1]; ApplyFontSizeChange(); }
    }

    [RelayCommand]
    private void DecreaseFontSize()
    {
        var idx = Array.IndexOf(_fontSizeValues, FontSizeScale);
        if (idx > 0) { FontSizeScale = _fontSizeValues[idx - 1]; ApplyFontSizeChange(); }
    }

    [RelayCommand]
    private void ResetFontSize() { FontSizeScale = 1.0; ApplyFontSizeChange(); }

    private void ApplyFontSizeChange()
    {
        try
        {
            Preferences.Set(Helpers.Constants.FontSizeKey, FontSizeScale);
            App.ApplyFontSizeScale(FontSizeScale);
            UpdateFontSizeLabel();
            _hardwareService.Vibrate();
        }
        catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"Font size change error: {ex.Message}"); }
    }

    private void UpdateFontSizeLabel()
    {
        var idx = Array.IndexOf(_fontSizeValues, FontSizeScale);
        FontSizeLabel = idx >= 0 ? _fontSizeLabels[idx] : "Custom";
    }

    [RelayCommand]
    private async Task OpenCameraTestAsync() => await Shell.Current.GoToAsync("camera");

    [RelayCommand]
    private void RefreshStatus()
    {
        CheckHardwareCapabilities();
        // Data source info is set at startup; no need to re-fetch here.
    }

    private void RefreshDataSourceInfo(IApiService apiService)
    {
        var apiUrl = Helpers.Constants.ApiBaseUrl;
        var hasApiUrl = !string.IsNullOrWhiteSpace(apiUrl);

        if (apiService.IsUsingFallback)
        {
            DataSourceLabel = "Offline — using local data";
            ApiUrlLabel = hasApiUrl ? $"API (unreachable): {apiUrl}" : "No API URL configured";
        }
        else
        {
            DataSourceLabel = "Online — connected to API";
            ApiUrlLabel = $"API: {apiUrl}";
        }
    }
}
