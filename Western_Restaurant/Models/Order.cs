using System.Text.Json.Serialization;

namespace Western_Restaurant.Models;

public class Order
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("items")]
    public List<OrderItem> Items { get; set; } = new();

    [JsonPropertyName("customerName")]
    public string CustomerName { get; set; } = string.Empty;

    [JsonPropertyName("customerPhone")]
    public string CustomerPhone { get; set; } = string.Empty;

    [JsonPropertyName("customerEmail")]
    public string CustomerEmail { get; set; } = string.Empty;

    [JsonPropertyName("tableNumber")]
    public int TableNumber { get; set; }

    [JsonPropertyName("specialInstructions")]
    public string SpecialInstructions { get; set; } = string.Empty;

    [JsonPropertyName("orderDate")]
    public DateTime OrderDate { get; set; } = DateTime.Now;

    [JsonIgnore]
    public decimal TotalAmount => Items.Sum(i => i.TotalPrice);

    [JsonIgnore]
    public string TotalDisplay => $"£{TotalAmount:F2}";

    [JsonIgnore]
    public int TotalItems => Items.Sum(i => i.Quantity);
}

public class OrderItem
{
    [JsonPropertyName("menuItemId")]
    public string MenuItemId { get; set; } = string.Empty;

    [JsonPropertyName("menuItemName")]
    public string MenuItemName { get; set; } = string.Empty;

    [JsonPropertyName("unitPrice")]
    public decimal UnitPrice { get; set; }

    [JsonPropertyName("quantity")]
    public int Quantity { get; set; } = 1;

    [JsonIgnore]
    public decimal TotalPrice => UnitPrice * Quantity;

    [JsonIgnore]
    public string PriceDisplay => $"{Quantity} x £{UnitPrice:F2} = £{TotalPrice:F2}";
}
