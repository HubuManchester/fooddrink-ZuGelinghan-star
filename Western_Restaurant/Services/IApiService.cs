using Western_Restaurant.Models;

namespace Western_Restaurant.Services;

public interface IApiService
{
    bool IsUsingFallback { get; }
    Task<List<Category>> GetCategoriesAsync();
    Task<List<Dish>> GetMenuItemsAsync(string? categoryId = null);
    Task<Dish?> GetMenuItemByIdAsync(string id);
    Task<bool> SubmitOrderAsync(Order order);
    Task<List<Dish>> SearchMenuItemsAsync(string query);
}
