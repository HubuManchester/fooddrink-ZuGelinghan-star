using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Western_Restaurant.Models;
using Western_Restaurant.Services;

namespace Western_Restaurant.ViewModels;

[QueryProperty(nameof(Dish), "Dish")]
public partial class MenuDetailViewModel : BaseViewModel
{
    private readonly IHardwareService _hardwareService;
    private readonly CartViewModel _cartViewModel;
    private CancellationTokenSource? _ttsCts;

    [ObservableProperty] private Dish? _dish;
    [ObservableProperty] private int _quantity = 1;
    [ObservableProperty] private bool _isSpeaking;
    [ObservableProperty] private string _cartActionLabel = "Add to Cart";

    public MenuDetailViewModel(IHardwareService hardwareService, CartViewModel cartViewModel)
    {
        _hardwareService = hardwareService;
        _cartViewModel = cartViewModel;
        Title = "Dish Details";
    }

    [RelayCommand]
    private void IncreaseQuantity() { if (Quantity < 20) Quantity++; _hardwareService.Vibrate(); }

    [RelayCommand]
    private void DecreaseQuantity() { if (Quantity > 1) Quantity--; _hardwareService.Vibrate(); }

    [RelayCommand]
    private async Task AddToCartAsync()
    {
        if (Dish == null) return;
        _cartViewModel.AddItem(Dish, Quantity);
        _hardwareService.VibrateLong();
        CartActionLabel = "Added! Add More?";
        await Shell.Current.DisplayAlert("Cart Updated", $"{Quantity}x {Dish.Name} added to your cart.", "OK");
    }

    [RelayCommand]
    private async Task SpeakDescriptionAsync()
    {
        if (Dish == null || IsSpeaking) return;
        try
        {
            IsSpeaking = true;
            _ttsCts = new CancellationTokenSource();
            var text = $"{Dish.Name}. {Dish.Description}. Priced at {Dish.PriceDisplay}.";
            await _hardwareService.SpeakAsync(text, _ttsCts.Token);
        }
        catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"TTS error: {ex.Message}"); }
        finally { IsSpeaking = false; }
    }

    [RelayCommand]
    private void StopSpeaking() { _ttsCts?.Cancel(); IsSpeaking = false; }
}
