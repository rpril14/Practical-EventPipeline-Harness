using System.Text.Json.Serialization;

namespace EventPipeline.Worker.Models;

public record OrderSnapshot(
    [property: JsonPropertyName("Id")] long Id,
    [property: JsonPropertyName("CustomerId")] long CustomerId,
    [property: JsonPropertyName("Status")] int Status,
    [property: JsonPropertyName("TotalAmount")] decimal TotalAmount,
    [property: JsonPropertyName("CreatedAt")] long CreatedAt,
    [property: JsonPropertyName("UpdatedAt")] long UpdatedAt);
