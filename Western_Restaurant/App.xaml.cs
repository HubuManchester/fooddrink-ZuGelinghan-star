namespace Western_Restaurant;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
        ApplyUserPreferences();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var window = new Window(new AppShell());
        return window;
    }

    private void ApplyUserPreferences()
    {
        try
        {
            bool isDark = Preferences.Get(Helpers.Constants.DarkModeKey, false);
            UserAppTheme = isDark ? AppTheme.Dark : AppTheme.Light;

            double fontSizeScale = Preferences.Get(Helpers.Constants.FontSizeKey, 1.0);
            if (Math.Abs(fontSizeScale - 1.0) > 0.01)
            {
                ApplyFontSizeScale(fontSizeScale);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error applying preferences: {ex.Message}");
        }
    }

    public static void ApplyFontSizeScale(double scale)
    {
        if (Current?.Resources == null) return;

        var fontKeys = new[] { "FontSizeSmall", "FontSizeBody", "FontSizeMedium", "FontSizeLarge", "FontSizeTitle", "FontSizeHeader" };
        var baseSizes = new[] { 12.0, 14.0, 16.0, 20.0, 24.0, 28.0 };

        for (int i = 0; i < fontKeys.Length; i++)
        {
            if (Current.Resources.TryGetValue(fontKeys[i], out var value) && value is double)
            {
                Current.Resources[fontKeys[i]] = baseSizes[i] * scale;
            }
        }
    }
}
