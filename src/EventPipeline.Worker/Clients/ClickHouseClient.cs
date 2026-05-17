using System;
using System.Threading.Tasks;
using ClickHouse.Client.ADO;
using ClickHouse.Client.ADO.Parameters;
using EventPipeline.Worker.Models;
using EventPipeline.Worker.Options;
using Microsoft.Extensions.Options;

namespace EventPipeline.Worker.Clients;

public class ClickHouseClient(IOptions<ClickHouseOptions> options) : IClickHouseClient
{
    private readonly string _connectionString = options.Value.ConnectionString;

    public async Task InsertAsync(OrderSnapshot snapshot, string op, long tsMs)
    {
        await using var connection = new ClickHouseConnection(_connectionString);
        await connection.OpenAsync();

        await using var ddl = connection.CreateCommand();
        ddl.CommandText = @"
            CREATE TABLE IF NOT EXISTS order_events (
                Id UInt64, CustomerId UInt64, Status Int32,
                TotalAmount Decimal64(2), CreatedAt DateTime, UpdatedAt DateTime,
                Op String, TsMs Int64
            ) ENGINE = MergeTree() ORDER BY (Id, TsMs)";
        await ddl.ExecuteNonQueryAsync();

        await using var cmd = connection.CreateCommand();
        cmd.CommandText =
            "INSERT INTO order_events VALUES (@Id,@CustomerId,@Status,@TotalAmount,@CreatedAt,@UpdatedAt,@Op,@TsMs)";
        cmd.Parameters.Add(new ClickHouseDbParameter { ParameterName = "Id", Value = (ulong)snapshot.Id });
        cmd.Parameters.Add(new ClickHouseDbParameter { ParameterName = "CustomerId", Value = (ulong)snapshot.CustomerId });
        cmd.Parameters.Add(new ClickHouseDbParameter { ParameterName = "Status", Value = snapshot.Status });
        cmd.Parameters.Add(new ClickHouseDbParameter { ParameterName = "TotalAmount", Value = snapshot.TotalAmount });
        cmd.Parameters.Add(new ClickHouseDbParameter { ParameterName = "CreatedAt", Value = DateTime.UnixEpoch.AddMicroseconds(snapshot.CreatedAt) });
        cmd.Parameters.Add(new ClickHouseDbParameter { ParameterName = "UpdatedAt", Value = DateTime.UnixEpoch.AddMicroseconds(snapshot.UpdatedAt) });
        cmd.Parameters.Add(new ClickHouseDbParameter { ParameterName = "Op", Value = op });
        cmd.Parameters.Add(new ClickHouseDbParameter { ParameterName = "TsMs", Value = tsMs });
        await cmd.ExecuteNonQueryAsync();
    }
}
