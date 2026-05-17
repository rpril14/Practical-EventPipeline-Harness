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
        // Arrange — PascalCase field names match actual Debezium MySQL output
        var json = """
            {
              "before": null,
              "after": {"Id":1,"CustomerId":2,"Status":1,"TotalAmount":50.00,"CreatedAt":0,"UpdatedAt":0},
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
              "before": {"Id":5,"CustomerId":3,"Status":2,"TotalAmount":100.00,"CreatedAt":0,"UpdatedAt":0},
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
              "before": {"Id":1,"CustomerId":1,"Status":1,"TotalAmount":10.00,"CreatedAt":0,"UpdatedAt":0},
              "after":  {"Id":1,"CustomerId":1,"Status":2,"TotalAmount":10.00,"CreatedAt":0,"UpdatedAt":0},
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
