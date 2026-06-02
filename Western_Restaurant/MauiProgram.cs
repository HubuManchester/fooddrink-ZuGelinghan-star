using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using Western_Restaurant.Services;
using Western_Restaurant.ViewModels;
using Western_Restaurant.Views;

namespace Western_Restaurant;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // --- Services (Singleton) ---
        builder.Services.AddSingleton<IApiService, ApiService>();
        builder.Services.AddSingleton<IHardwareService, HardwareService>();

        // --- ViewModels ---
        builder.Services.AddSingleton<MenuViewModel>();
        builder.Services.AddSingleton<CartViewModel>();
        builder.Services.AddSingleton<CommentsViewModel>();
        builder.Services.AddSingleton<SettingsViewModel>();
        builder.Services.AddTransient<MenuDetailViewModel>();
        builder.Services.AddTransient<OrderViewModel>();

        // --- Pages ---
        builder.Services.AddSingleton<MenuPage>();
        builder.Services.AddSingleton<CartPage>();
        builder.Services.AddSingleton<CommentsPage>();
        builder.Services.AddSingleton<SettingsPage>();
        builder.Services.AddTransient<MenuDetailPage>();
        builder.Services.AddTransient<OrderPage>();
        builder.Services.AddTransient<CameraPage>();
        builder.Services.AddTransient<HelpPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
