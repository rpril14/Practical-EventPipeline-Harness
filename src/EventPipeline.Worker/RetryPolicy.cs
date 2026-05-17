using System;
using System.IO;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace EventPipeline.Worker;

public class RetryPolicy(Func<int, TimeSpan>? delayStrategy = null)
{
    private readonly Func<int, TimeSpan> _delay =
        delayStrategy ?? (attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt - 1)));

    private const int MaxAttempts = 5;

    public async Task ExecuteAsync(Func<Task> action)
    {
        for (var attempt = 1; ; attempt++)
        {
            try
            {
                await action();
                return;
            }
            catch (Exception ex) when (IsTransient(ex))
            {
                if (attempt >= MaxAttempts) throw;
                await Task.Delay(_delay(attempt));
            }
            // non-transient: re-throws immediately
        }
    }

    private static bool IsTransient(Exception ex) =>
        ex is HttpRequestException or SocketException or TimeoutException or IOException;
}
