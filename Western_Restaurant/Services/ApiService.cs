using System.Net.Http.Json;
using System.Text.Json;
using Western_Restaurant.Helpers;
using Western_Restaurant.Models;

namespace Western_Restaurant.Services;

public class ApiService : IApiService
{
    private readonly HttpClient _httpClient;
    private List<Dish>? _cachedMenuItems;
    private List<Category>? _cachedCategories;
    private bool _isUsingFallback;
    private bool _initialized;

    public bool IsUsingFallback => _isUsingFallback;

    public ApiService()
    {
        _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(Constants.ApiTimeoutSeconds)
        };
    }

    private string? BuildApiUrl(string endpoint, string? id = null)
    {
        if (string.IsNullOrWhiteSpace(Constants.ApiBaseUrl))
            return null;
        var baseUrl = Constants.ApiBaseUrl.TrimEnd('/');
        var url = $"{baseUrl}/{endpoint}";
        if (!string.IsNullOrWhiteSpace(id))
            url += $"/{id}";
        return url;
    }

    private async Task<T?> TryFetchFromApiAsync<T>(string url) where T : class
    {
        try
        {
            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                System.Diagnostics.Debug.WriteLine($"API returned {(int)response.StatusCode} for {url}");
                return null;
            }
            return await response.Content.ReadFromJsonAsync<T>();
        }
        catch (TaskCanceledException)
        {
            System.Diagnostics.Debug.WriteLine($"API request timed out: {url}");
            return null;
        }
        catch (HttpRequestException ex)
        {
            System.Diagnostics.Debug.WriteLine($"API network error: {ex.Message}");
            return null;
        }
        catch (JsonException ex)
        {
            System.Diagnostics.Debug.WriteLine($"API JSON parse error: {ex.Message}");
            return null;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"API unexpected error: {ex.Message}");
            return null;
        }
    }

    public async Task<List<Category>> GetCategoriesAsync()
    {
        await EnsureInitializedAsync();
        return _cachedCategories ?? GetFallbackCategories();
    }

    public async Task<List<Dish>> GetMenuItemsAsync(string? categoryId = null)
    {
        await EnsureInitializedAsync();
        var items = _cachedMenuItems ?? GetFallbackMenuItems();
        if (!string.IsNullOrWhiteSpace(categoryId))
            items = items.Where(m => m.CategoryId == categoryId).ToList();
        return items;
    }

    public async Task<Dish?> GetMenuItemByIdAsync(string id)
    {
        await EnsureInitializedAsync();
        return (_cachedMenuItems ?? GetFallbackMenuItems()).FirstOrDefault(m => m.Id == id);
    }

    public async Task<bool> SubmitOrderAsync(Order order)
    {
        var url = BuildApiUrl(Constants.ApiOrderEndpoint);
        if (url == null)
        {
            SaveOrderLocally(order);
            return true;
        }

        try
        {
            var response = await _httpClient.PostAsJsonAsync(url, order);
            if (response.IsSuccessStatusCode) return true;
            SaveOrderLocally(order);
            return true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Order submission error: {ex.Message}");
            SaveOrderLocally(order);
            return true;
        }
    }

    public async Task<List<Dish>> SearchMenuItemsAsync(string query)
    {
        await EnsureInitializedAsync();
        var items = _cachedMenuItems ?? GetFallbackMenuItems();
        if (string.IsNullOrWhiteSpace(query))
            return items;

        query = query.Trim().ToLowerInvariant();
        return items.Where(m =>
            m.Name.Contains(query, StringComparison.OrdinalIgnoreCase) ||
            m.Description.Contains(query, StringComparison.OrdinalIgnoreCase) ||
            (m.Tags?.Any(t => t.Contains(query, StringComparison.OrdinalIgnoreCase)) ?? false)
        ).ToList();
    }

    private async Task EnsureInitializedAsync()
    {
        if (_initialized) return;

        var categoriesUrl = BuildApiUrl(Constants.ApiCategoryEndpoint);
        var menuUrl = BuildApiUrl(Constants.ApiMenuEndpoint);

        if (categoriesUrl != null && menuUrl != null)
        {
            var catTask = TryFetchFromApiAsync<List<Category>>(categoriesUrl);
            var menuTask = TryFetchFromApiAsync<List<Dish>>(menuUrl);
            await Task.WhenAll(catTask, menuTask);
            _cachedCategories = catTask.Result;
            _cachedMenuItems = menuTask.Result;
            _isUsingFallback = _cachedCategories == null || _cachedMenuItems == null;
        }
        else
        {
            _isUsingFallback = true;
        }

        _initialized = true;
    }

    private void SaveOrderLocally(Order order)
    {
        try
        {
            var key = $"order_{DateTime.Now:yyyyMMddHHmmss}";
            var json = JsonSerializer.Serialize(order);
            Preferences.Set(key, json);
            System.Diagnostics.Debug.WriteLine($"Order saved locally: {key}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to save order locally: {ex.Message}");
        }
    }

    // ========================================================================
    // FALLBACK DATA
    // ========================================================================

    private static List<Category> GetFallbackCategories()
    {
        return new List<Category>
        {
            new() { Id = "cat1", Name = "Appetizers", Description = "Start your meal right", IconGlyph = "\U0001F96D" },
            new() { Id = "cat2", Name = "Main Courses", Description = "Hearty western classics", IconGlyph = "\U0001F969" },
            new() { Id = "cat3", Name = "Desserts", Description = "Sweet indulgences", IconGlyph = "\U0001F370" },
            new() { Id = "cat4", Name = "Beverages", Description = "Refreshing drinks", IconGlyph = "\U0001F379" },
        };
    }

    private static List<Dish> GetFallbackMenuItems()
    {
        return new List<Dish>
        {
            new() { Id = "m01", Name = "Classic Caesar Salad", Description = "Crisp romaine lettuce, parmesan, croutons, and our house Caesar dressing.", Price = 12.99m, CategoryId = "cat1", CategoryName = "Appetizers", IsAvailable = true, Tags = new() { "salad", "healthy", "vegetarian" } },
            new() { Id = "m02", Name = "French Onion Soup", Description = "Rich beef broth with caramelized onions, topped with melted Gruyere cheese.", Price = 10.99m, CategoryId = "cat1", CategoryName = "Appetizers", IsAvailable = true, Tags = new() { "soup", "hot", "cheese" } },
            new() { Id = "m03", Name = "Crispy Calamari", Description = "Lightly battered squid rings served with marinara and garlic aioli.", Price = 13.99m, CategoryId = "cat1", CategoryName = "Appetizers", IsAvailable = true, Tags = new() { "seafood", "fried" } },
            new() { Id = "m04", Name = "Bruschetta Trio", Description = "Toasted ciabatta with tomato-basil, mushroom, and roasted pepper toppings.", Price = 11.99m, CategoryId = "cat1", CategoryName = "Appetizers", IsAvailable = true, Tags = new() { "bread", "vegetarian", "italian" } },
            new() { Id = "m05", Name = "Stuffed Mushrooms", Description = "Button mushrooms filled with herbed cream cheese and breadcrumbs, baked golden.", Price = 9.99m, CategoryId = "cat1", CategoryName = "Appetizers", IsAvailable = false, Tags = new() { "vegetarian", "baked" } },
            new() { Id = "m06", Name = "Grilled Ribeye Steak", Description = "12oz prime ribeye, char-grilled to your liking, served with mashed potatoes and seasonal vegetables.", Price = 34.99m, CategoryId = "cat2", CategoryName = "Main Courses", IsAvailable = true, Tags = new() { "steak", "beef", "grilled", "signature" } },
            new() { Id = "m07", Name = "Pan-Seared Salmon", Description = "Atlantic salmon fillet with lemon butter sauce, asparagus, and wild rice.", Price = 28.99m, CategoryId = "cat2", CategoryName = "Main Courses", IsAvailable = true, Tags = new() { "seafood", "healthy", "fish" } },
            new() { Id = "m08", Name = "Classic Beef Burger", Description = "Angus beef patty with cheddar, bacon, lettuce, tomato, and house sauce. Served with fries.", Price = 18.99m, CategoryId = "cat2", CategoryName = "Main Courses", IsAvailable = true, Tags = new() { "burger", "beef", "popular" } },
            new() { Id = "m09", Name = "Creamy Carbonara", Description = "Spaghetti with pancetta, egg yolk, parmesan, and black pepper in a silky cream sauce.", Price = 19.99m, CategoryId = "cat2", CategoryName = "Main Courses", IsAvailable = true, Tags = new() { "pasta", "italian", "pork" } },
            new() { Id = "m10", Name = "Fish & Chips", Description = "Beer-battered cod with thick-cut chips, mushy peas, and tartar sauce.", Price = 17.99m, CategoryId = "cat2", CategoryName = "Main Courses", IsAvailable = true, Tags = new() { "seafood", "fried", "british" } },
            new() { Id = "m11", Name = "BBQ Baby Back Ribs", Description = "Slow-cooked pork ribs glazed with smoky BBQ sauce, coleslaw, and cornbread.", Price = 31.99m, CategoryId = "cat2", CategoryName = "Main Courses", IsAvailable = true, Tags = new() { "pork", "bbq", "signature" } },
            new() { Id = "m12", Name = "Grilled Chicken Caesar Wrap", Description = "Sliced grilled chicken, romaine, parmesan, and Caesar dressing in a flour tortilla.", Price = 15.99m, CategoryId = "cat2", CategoryName = "Main Courses", IsAvailable = false, Tags = new() { "chicken", "wrap", "light" } },
            new() { Id = "m13", Name = "New York Cheesecake", Description = "Creamy cheesecake on a graham cracker crust, topped with strawberry compote.", Price = 10.99m, CategoryId = "cat3", CategoryName = "Desserts", IsAvailable = true, Tags = new() { "cake", "cheese", "popular" } },
            new() { Id = "m14", Name = "Chocolate Lava Cake", Description = "Warm chocolate cake with a molten center, served with vanilla bean ice cream.", Price = 11.99m, CategoryId = "cat3", CategoryName = "Desserts", IsAvailable = true, Tags = new() { "chocolate", "hot", "icecream" } },
            new() { Id = "m15", Name = "Tiramisu", Description = "Classic Italian dessert with espresso-soaked ladyfingers, mascarpone, and cocoa.", Price = 9.99m, CategoryId = "cat3", CategoryName = "Desserts", IsAvailable = true, Tags = new() { "italian", "coffee", "classic" } },
            new() { Id = "m16", Name = "Apple Crumble", Description = "Warm baked apples with a buttery oat crumble topping and custard.", Price = 9.99m, CategoryId = "cat3", CategoryName = "Desserts", IsAvailable = true, Tags = new() { "apple", "baked", "warm" } },
            new() { Id = "m17", Name = "House Red Wine", Description = "A full-bodied Cabernet Sauvignon from Napa Valley. 150ml glass.", Price = 12.99m, CategoryId = "cat4", CategoryName = "Beverages", IsAvailable = true, Tags = new() { "wine", "alcohol", "red" } },
            new() { Id = "m18", Name = "Classic Mojito", Description = "White rum, fresh mint, lime, sugar, and soda water.", Price = 11.99m, CategoryId = "cat4", CategoryName = "Beverages", IsAvailable = true, Tags = new() { "cocktail", "alcohol", "mint" } },
            new() { Id = "m19", Name = "Espresso Martini", Description = "Vodka, Kahlua, and fresh espresso shaken and served in a chilled glass.", Price = 13.99m, CategoryId = "cat4", CategoryName = "Beverages", IsAvailable = true, Tags = new() { "cocktail", "alcohol", "coffee" } },
            new() { Id = "m20", Name = "Fresh Lemonade", Description = "House-made lemonade with fresh lemons, sugar, and a hint of mint.", Price = 5.99m, CategoryId = "cat4", CategoryName = "Beverages", IsAvailable = true, Tags = new() { "non-alcoholic", "citrus", "refreshing" } },
            new() { Id = "m21", Name = "Cappuccino", Description = "Double espresso with steamed milk and a thick layer of foam. Topped with cocoa.", Price = 5.99m, CategoryId = "cat4", CategoryName = "Beverages", IsAvailable = true, Tags = new() { "coffee", "hot", "non-alcoholic" } },
            new() { Id = "m22", Name = "Sparkling Water", Description = "San Pellegrino sparkling mineral water. 500ml bottle.", Price = 4.99m, CategoryId = "cat4", CategoryName = "Beverages", IsAvailable = true, Tags = new() { "non-alcoholic", "water" } },
        };
    }
}
