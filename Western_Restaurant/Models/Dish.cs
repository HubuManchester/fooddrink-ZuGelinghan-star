using System.Text.Json.Serialization;

namespace Western_Restaurant.Models;

public class Dish
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("price")]
    public decimal Price { get; set; }

    [JsonPropertyName("imageUrl")]
    public string ImageUrl { get; set; } = string.Empty;

    [JsonPropertyName("categoryId")]
    public string CategoryId { get; set; } = string.Empty;

    [JsonPropertyName("categoryName")]
    public string CategoryName { get; set; } = string.Empty;

    [JsonPropertyName("isAvailable")]
    public bool IsAvailable { get; set; } = true;

    [JsonPropertyName("tags")]
    public List<string> Tags { get; set; } = new();

    [JsonIgnore]
    public string PriceDisplay => $"£{Price:F2}";

    [JsonIgnore]
    public string AvailabilityLabel => IsAvailable ? string.Empty : "(Unavailable)";
}
