using System.Text.Json.Serialization;

namespace Western_Restaurant.Models;

public class Comment
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString("N")[..8];

    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;

    [JsonPropertyName("photoPath")]
    public string PhotoPath { get; set; } = string.Empty;

    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; } = DateTime.Now;

    [JsonIgnore]
    public string TimestampDisplay => Timestamp.ToString("MMM dd, HH:mm");

    [JsonIgnore]
    public bool HasPhoto => !string.IsNullOrWhiteSpace(PhotoPath);
}
