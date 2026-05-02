using Polly;
using Polly.Extensions.Http;
using System.Net.Http;
using PAN.API.Infrastructure.Logging;

namespace PAN.API.Infrastructure.Resilience;

public static class PollyPolicies
{
    public static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy(string providerName)
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(r => !r.IsSuccessStatusCode)
            .WaitAndRetryAsync(
                retryCount: 2,
                sleepDurationProvider: retry => TimeSpan.FromSeconds(Math.Pow(2, retry)),
                onRetry: (outcome, timespan, retryCount, context) =>
                {
                    SafeLogger.App(
                        $"[POLLY RETRY] Provider={providerName} | Attempt={retryCount} | Wait={timespan.TotalSeconds}s"
                    );
                });
    }

    public static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy(string providerName)
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(r => !r.IsSuccessStatusCode)
            .CircuitBreakerAsync(
                handledEventsAllowedBeforeBreaking: 3,
                durationOfBreak: TimeSpan.FromSeconds(30),
                onBreak: (outcome, breakDelay) =>
                {
                    SafeLogger.App(
                        $"[POLLY CIRCUIT OPEN] Provider={providerName} | Break={breakDelay.TotalSeconds}s"
                    );
                },
                onReset: () =>
                {
                    SafeLogger.App($"[POLLY CIRCUIT CLOSED] Provider={providerName}");
                },
                onHalfOpen: () =>
                {
                    SafeLogger.App($"[POLLY HALF-OPEN] Provider={providerName}");
                });
    }
}