using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Western_Restaurant.Models;
using Western_Restaurant.Services;

namespace Western_Restaurant.ViewModels;

public partial class CartViewModel : BaseViewModel
{
    private readonly IHardwareService _hardwareService;
    private const decimal TaxRate = 0.10m;

    [ObservableProperty] private ObservableCollection<OrderItem> _cartItems = new();
    [ObservableProperty] private decimal _subtotal;
    [ObservableProperty] private decimal _tax;
    [ObservableProperty] private decimal _total;
    [ObservableProperty] private bool _isCartEmpty = true;

    public CartViewModel(IHardwareService hardwareService)
    {
        _hardwareService = hardwareService;
        Title = "Your Cart";
    }

    public void AddItem(Dish dish, int quantity = 1)
    {
        try
        {
            var existing = CartItems.FirstOrDefault(i => i.MenuItemId == dish.Id);
            if (existing != null)
            {
                existing.Quantity += quantity;
                var idx = CartItems.IndexOf(existing);
                CartItems.RemoveAt(idx);
                CartItems.Insert(idx, existing);
            }
            else
            {
                CartItems.Add(new OrderItem { MenuItemId = dish.Id, MenuItemName = dish.Name, UnitPrice = dish.Price, Quantity = quantity });
            }
            RecalculateTotals();
            _hardwareService.Vibrate();
        }
        catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"Add to cart error: {ex.Message}"); }
    }

    [RelayCommand]
    private void IncreaseItemQuantity(OrderItem? item)
    {
        if (item == null || item.Quantity >= 20) return;
        item.Quantity++;
        ForceRefreshItem(item);
    }

    [RelayCommand]
    private void DecreaseItemQuantity(OrderItem? item)
    {
        if (item == null) return;
        if (item.Quantity > 1) { item.Quantity--; ForceRefreshItem(item); }
        else RemoveItem(item);
    }

    [RelayCommand]
    private void RemoveItem(OrderItem? item)
    {
        if (item == null) return;
        CartItems.Remove(item);
        RecalculateTotals();
    }

    [RelayCommand]
    private async Task ClearCartAsync()
    {
        if (CartItems.Count == 0) return;
        bool confirm = await Shell.Current.DisplayAlert("Clear Cart", "Remove all items?", "Yes, Clear", "Cancel");
        if (confirm) { CartItems.Clear(); RecalculateTotals(); _hardwareService.VibrateLong(); }
    }

    [RelayCommand]
    private async Task CheckoutAsync()
    {
        if (CartItems.Count == 0)
        {
            await Shell.Current.DisplayAlert("Empty Cart", "Please add items before checking out.", "OK");
            return;
        }
        _hardwareService.Vibrate();
        await Shell.Current.GoToAsync("order");
    }

    public Order CreateOrder() => new() { Items = new List<OrderItem>(CartItems), OrderDate = DateTime.Now };

    public void ClearAfterOrder() { CartItems.Clear(); RecalculateTotals(); }

    private void ForceRefreshItem(OrderItem item)
    {
        var idx = CartItems.IndexOf(item);
        if (idx < 0) return;
        CartItems.RemoveAt(idx);
        CartItems.Insert(idx, item);
        RecalculateTotals();
    }

    private void RecalculateTotals()
    {
        Subtotal = CartItems.Sum(i => i.TotalPrice);
        Tax = Subtotal * TaxRate;
        Total = Subtotal + Tax;
        IsCartEmpty = CartItems.Count == 0;
        OnPropertyChanged(nameof(Subtotal));
        OnPropertyChanged(nameof(Tax));
        OnPropertyChanged(nameof(Total));
    }
}
