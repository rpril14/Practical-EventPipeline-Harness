using System.Text.Json.Serialization;

namespace EventPipeline.Worker.Models;

public record OrderSnapshot(
    [property: JsonPropertyName("id")] long Id,
    [property: JsonPropertyName("customer_id")] long CustomerId,
    [property: JsonPropertyName("status")] int Status,
    [property: JsonPropertyName("total_amount")] decimal TotalAmount,
    [property: JsonPropertyName("created_at")] long CreatedAt,
    [property: JsonPropertyName("updated_at")] long UpdatedAt);
