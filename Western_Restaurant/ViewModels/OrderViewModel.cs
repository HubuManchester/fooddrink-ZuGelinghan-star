using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Western_Restaurant.Models;
using Western_Restaurant.Services;

namespace Western_Restaurant.ViewModels;

public partial class OrderViewModel : BaseViewModel
{
    private readonly IApiService _apiService;
    private readonly IHardwareService _hardwareService;
    private readonly CartViewModel _cartViewModel;

    [ObservableProperty] private string _customerName = string.Empty;
    [ObservableProperty] private string _customerPhone = string.Empty;
    [ObservableProperty] private string _customerEmail = string.Empty;
    [ObservableProperty] private string _tableNumberText = string.Empty;
    [ObservableProperty] private string _specialInstructions = string.Empty;

    [ObservableProperty] private string _nameError = string.Empty;
    [ObservableProperty] private string _phoneError = string.Empty;
    [ObservableProperty] private string _emailError = string.Empty;
    [ObservableProperty] private string _tableError = string.Empty;
    [ObservableProperty] private bool _hasNameError;
    [ObservableProperty] private bool _hasPhoneError;
    [ObservableProperty] private bool _hasEmailError;
    [ObservableProperty] private bool _hasTableError;

    [ObservableProperty] private int _itemCount;
    [ObservableProperty] private string _orderSummary = string.Empty;

    public OrderViewModel(IApiService apiService, IHardwareService hardwareService, CartViewModel cartViewModel)
    {
        _apiService = apiService;
        _hardwareService = hardwareService;
        _cartViewModel = cartViewModel;
        Title = "Checkout";
    }

    [RelayCommand]
    private void LoadOrderSummary()
    {
        var order = _cartViewModel.CreateOrder();
        ItemCount = order.TotalItems;
        OrderSummary = $"{ItemCount} item(s) — {order.TotalDisplay}";
        CustomerName = Preferences.Get(Helpers.Constants.UserNameKey, string.Empty);
    }

    private bool ValidateForm()
    {
        bool isValid = true;

        if (string.IsNullOrWhiteSpace(CustomerName)) { NameError = "Please enter your name."; HasNameError = true; isValid = false; }
        else if (CustomerName.Trim().Length < 2) { NameError = "Name must be at least 2 characters."; HasNameError = true; isValid = false; }
        else { NameError = string.Empty; HasNameError = false; }

        if (string.IsNullOrWhiteSpace(CustomerPhone)) { PhoneError = "Please enter your phone number."; HasPhoneError = true; isValid = false; }
        else if (!System.Text.RegularExpressions.Regex.IsMatch(CustomerPhone.Trim(), @"^[\d\s\-\(\)\+]{7,20}$")) { PhoneError = "Enter a valid phone number (7-20 digits)."; HasPhoneError = true; isValid = false; }
        else { PhoneError = string.Empty; HasPhoneError = false; }

        if (!string.IsNullOrWhiteSpace(CustomerEmail) && !System.Text.RegularExpressions.Regex.IsMatch(CustomerEmail.Trim(), @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
        { EmailError = "Enter a valid email address."; HasEmailError = true; isValid = false; }
        else { EmailError = string.Empty; HasEmailError = false; }

        if (string.IsNullOrWhiteSpace(TableNumberText)) { TableError = "Please enter your table number."; HasTableError = true; isValid = false; }
        else if (!int.TryParse(TableNumberText.Trim(), out int t) || t < 1 || t > 99) { TableError = "Table must be a number between 1 and 99."; HasTableError = true; isValid = false; }
        else { TableError = string.Empty; HasTableError = false; }

        return isValid;
    }

    [RelayCommand]
    private async Task SubmitOrderAsync()
    {
        if (!ValidateForm())
        {
            _hardwareService.VibrateLong();
            await Shell.Current.DisplayAlert("Validation Error", "Please correct the highlighted fields before submitting.", "OK");
            return;
        }

        var order = _cartViewModel.CreateOrder();
        order.CustomerName = CustomerName.Trim();
        order.CustomerPhone = CustomerPhone.Trim();
        order.CustomerEmail = CustomerEmail.Trim();
        order.TableNumber = int.Parse(TableNumberText.Trim());
        order.SpecialInstructions = SpecialInstructions.Trim();

        await ExecuteSafelyAsync(async () =>
        {
            bool success = await _apiService.SubmitOrderAsync(order);
            if (success)
            {
                Preferences.Set(Helpers.Constants.UserNameKey, CustomerName.Trim());
                _hardwareService.VibrateLong();
                var msg = _apiService.IsUsingFallback ? "Your order has been saved locally (offline mode)." : "Your order has been submitted successfully!";
                await Shell.Current.DisplayAlert("Order Confirmed", $"{msg}\n\nTotal: {order.TotalDisplay}\nThank you, {order.CustomerName}!", "OK");
                _cartViewModel.ClearAfterOrder();
                await Shell.Current.GoToAsync("..");
            }
            else await Shell.Current.DisplayAlert("Order Failed", "Could not process order. Please try again.", "OK");
        }, "Order submission failed");
    }

    [RelayCommand]
    private void ClearForm()
    {
        CustomerName = string.Empty; CustomerPhone = string.Empty; CustomerEmail = string.Empty;
        TableNumberText = string.Empty; SpecialInstructions = string.Empty;
        NameError = string.Empty; PhoneError = string.Empty; EmailError = string.Empty; TableError = string.Empty;
        HasNameError = false; HasPhoneError = false; HasEmailError = false; HasTableError = false;
    }
}
