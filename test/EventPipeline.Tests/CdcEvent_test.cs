using System.Text.Json;
using EventPipeline.Worker.Models;
using Xunit;

namespace EventPipeline.Tests;

public class CdcEvent_test
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web);

    [Fact]
    public void Deserialize_CreateOp_MapsAfterAndOp()
    {
        // Arrange
        var json = """
            {
              "before": null,
              "after": {"id":1,"customer_id":2,"status":1,"total_amount":50.00,"created_at":0,"updated_at":0},
              "op": "c",
              "ts_ms": 1700000000000
            }
            """;

        // Act
        var evt = JsonSerializer.Deserialize<CdcEvent<OrderSnapshot>>(json, Options);

        // Assert
        Assert.NotNull(evt);
        Assert.Equal("c", evt.Op);
        Assert.Null(evt.Before);
        Assert.NotNull(evt.After);
        Assert.Equal(1, evt.After.Id);
        Assert.Equal(2, evt.After.CustomerId);
        Assert.Equal(1700000000000L, evt.TsMs);
    }

    [Fact]
    public void Deserialize_DeleteOp_MapsBeforeOnly()
    {
        // Arrange
        var json = """
            {
              "before": {"id":5,"customer_id":3,"status":2,"total_amount":100.00,"created_at":0,"updated_at":0},
              "after": null,
              "op": "d",
              "ts_ms": 1700000001000
            }
            """;

        // Act
        var evt = JsonSerializer.Deserialize<CdcEvent<OrderSnapshot>>(json, Options);

        // Assert
        Assert.Equal("d", evt!.Op);
        Assert.NotNull(evt.Before);
        Assert.Equal(5, evt.Before.Id);
        Assert.Null(evt.After);
    }

    [Fact]
    public void Deserialize_UpdateOp_MapsBothBeforeAndAfter()
    {
        // Arrange
        var json = """
            {
              "before": {"id":1,"customer_id":1,"status":1,"total_amount":10.00,"created_at":0,"updated_at":0},
              "after":  {"id":1,"customer_id":1,"status":2,"total_amount":10.00,"created_at":0,"updated_at":0},
              "op": "u",
              "ts_ms": 1700000002000
            }
            """;

        // Act
        var evt = JsonSerializer.Deserialize<CdcEvent<OrderSnapshot>>(json, Options);

        // Assert
        Assert.Equal("u", evt!.Op);
        Assert.Equal(1, evt.Before!.Status);
        Assert.Equal(2, evt.After!.Status);
    }
}
