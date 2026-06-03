using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Western_Restaurant.Models;
using Western_Restaurant.Services;

namespace Western_Restaurant.ViewModels;

public partial class MenuViewModel : BaseViewModel
{
    private readonly IApiService _apiService;
    private readonly IHardwareService _hardwareService;
    private List<Dish> _allDishes = new();
    private List<Category> _allCategories = new();

    [ObservableProperty] private ObservableCollection<Dish> _menuItems = new();
    [ObservableProperty] private ObservableCollection<Category> _categories = new();
    [ObservableProperty] private Category? _selectedCategory;
    [ObservableProperty] private string _searchQuery = string.Empty;
    [ObservableProperty] private bool _isUsingFallbackData;

    public MenuViewModel(IApiService apiService, IHardwareService hardwareService)
    {
        _apiService = apiService;
        _hardwareService = hardwareService;
        Title = "Western Restaurant";
    }

    [RelayCommand]
    private async Task LoadMenuAsync()
    {
        await ExecuteSafelyAsync(async () =>
        {
            var previousSelectedId = SelectedCategory?.Id;

            _allCategories = await _apiService.GetCategoriesAsync();
            _allDishes = await _apiService.GetMenuItemsAsync();
            IsUsingFallbackData = _apiService.IsUsingFallback;

            Categories = new ObservableCollection<Category>(_allCategories);
            Categories.Insert(0, new Category { Id = "all", Name = "All Items", Description = "Browse the full menu", IconGlyph = "\U0001F37D" });

            // Restore previous category selection
            var target = Categories.FirstOrDefault(c => c.Id == previousSelectedId) ?? Categories[0];
            foreach (var c in Categories)
                c.IsSelected = c.Id == target.Id;
            SelectedCategory = target;
            ApplyFilters();
        }, "Failed to load menu");
    }

    [RelayCommand]
    private void FilterByCategory(Category? category)
    {
        if (category == null) return;
        foreach (var c in Categories)
            c.IsSelected = c.Id == category.Id;
        SelectedCategory = category;
        ApplyFilters();
        _hardwareService.Vibrate();
    }

    [RelayCommand]
    private async Task SearchAsync()
    {
        if (string.IsNullOrWhiteSpace(SearchQuery)) { ApplyFilters(); return; }
        await ExecuteSafelyAsync(async () =>
        {
            var results = await _apiService.SearchMenuItemsAsync(SearchQuery);
            MenuItems =new ObservableCollection<Dish>(results);
        });
    }

    [RelayCommand]
    private void ClearSearch()
    {
        SearchQuery = string.Empty;
        ApplyFilters();
    }

    [RelayCommand]
    private async Task ViewItemDetailAsync(Dish? item)
    {
        if (item == null) return;
        _hardwareService.Vibrate();
        var navParams = new Dictionary<string, object> { { "Dish", item } };
        await Shell.Current.GoToAsync("detail", navParams);
    }

    [RelayCommand]
    private void EnableShakeRecommendation()
    {
        _hardwareService.StartShakeDetection(async () =>
        {
            _hardwareService.VibrateLong();
            await RecommendRandomItemAsync();
        });
    }

    [RelayCommand]
    private async Task RecommendRandomItemAsync()
    {
        var available = _allDishes.Where(m => m.IsAvailable).ToList();
        if (available.Count == 0) return;
        var picked = available[new Random().Next(available.Count)];
        await Shell.Current.DisplayAlert("Chef's Recommendation", $"We recommend: {picked.Name}\n{picked.PriceDisplay}", "View Details");
        await ViewItemDetailAsync(picked);
    }

    [RelayCommand]
    private void DisableShakeRecommendation() => _hardwareService.StopShakeDetection();

    private void ApplyFilters()
    {
        var filtered = SelectedCategory == null || SelectedCategory.Id == "all"
            ? _allDishes
            : _allDishes.Where(m => m.CategoryId == SelectedCategory.Id).ToList();
        MenuItems =new ObservableCollection<Dish>(filtered);
    }
}
