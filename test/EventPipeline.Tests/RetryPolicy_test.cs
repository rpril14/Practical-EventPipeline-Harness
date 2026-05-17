using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading.Tasks;
using EventPipeline.Worker;
using Xunit;

namespace EventPipeline.Tests;

public class RetryPolicy_test
{
    private static RetryPolicy InstantRetry() => new(_ => TimeSpan.Zero);

    [Fact]
    public async Task ExecuteAsync_Success_ExecutesOnce()
    {
        // Arrange
        var policy = InstantRetry();
        var callCount = 0;

        // Act
        await policy.ExecuteAsync(() => { callCount++; return Task.CompletedTask; });

        // Assert
        Assert.Equal(1, callCount);
    }

    [Fact]
    public async Task ExecuteAsync_TransientException_RetriesUpToFiveAttempts()
    {
        // Arrange
        var policy = InstantRetry();
        var callCount = 0;

        // Act
        await Assert.ThrowsAsync<IOException>(async () =>
            await policy.ExecuteAsync(() =>
            {
                callCount++;
                throw new IOException("transient");
            }));

        // Assert
        Assert.Equal(5, callCount);
    }

    [Fact]
    public async Task ExecuteAsync_PermanentException_ThrowsImmediately()
    {
        // Arrange
        var policy = InstantRetry();
        var callCount = 0;

        // Act
        await Assert.ThrowsAsync<ArgumentException>(async () =>
            await policy.ExecuteAsync(() =>
            {
                callCount++;
                throw new ArgumentException("permanent");
            }));

        // Assert
        Assert.Equal(1, callCount);
    }

    [Fact]
    public async Task ExecuteAsync_TransientThenSuccess_StopsRetrying()
    {
        // Arrange
        var policy = InstantRetry();
        var callCount = 0;

        // Act
        await policy.ExecuteAsync(() =>
        {
            callCount++;
            if (callCount < 3) throw new SocketException();
            return Task.CompletedTask;
        });

        // Assert
        Assert.Equal(3, callCount);
    }

    [Fact]
    public async Task ExecuteAsync_BackoffDelay_FollowsExponentialPattern()
    {
        // Arrange
        var capturedDelays = new List<double>();
        var policy = new RetryPolicy(attempt =>
        {
            capturedDelays.Add(Math.Pow(2, attempt - 1));
            return TimeSpan.Zero;
        });
        var callCount = 0;

        // Act
        await Assert.ThrowsAsync<HttpRequestException>(async () =>
            await policy.ExecuteAsync(() =>
            {
                callCount++;
                throw new HttpRequestException();
            }));

        // Assert — 4 delays for 5 attempts (no delay after final failure)
        Assert.Equal(new[] { 1.0, 2.0, 4.0, 8.0 }, capturedDelays);
    }

    [Fact]
    public async Task ExecuteAsync_AllTransientTypes_AreRetried()
    {
        // Arrange + Act + Assert for each transient exception type
        foreach (var ex in new Exception[]
            { new HttpRequestException(), new SocketException(), new TimeoutException(), new IOException() })
        {
            var policy = InstantRetry();
            var callCount = 0;
            var thrownEx = ex;
            await Assert.ThrowsAsync(thrownEx.GetType(), async () =>
                await policy.ExecuteAsync(() =>
                {
                    callCount++;
                    throw thrownEx;
                }));
            Assert.Equal(5, callCount);
        }
    }
}
