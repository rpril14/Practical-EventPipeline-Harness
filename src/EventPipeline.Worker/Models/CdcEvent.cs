using System.Text.Json.Serialization;

namespace EventPipeline.Worker.Models;

public record CdcEvent<T>(
    [property: JsonPropertyName("before")] T? Before,
    [property: JsonPropertyName("after")] T? After,
    [property: JsonPropertyName("op")] string Op,
    [property: JsonPropertyName("ts_ms")] long TsMs);
