using System.Net;
using Polly;
using RefreshJwtExample.Services;

namespace RefreshJwtExample.Http;

public static class SalesforceTokenRefreshPolicy
{
    public static IAsyncPolicy<HttpResponseMessage>
        GetSalesforceTokenRefresPolicy(
            IServiceProvider serviceProvider,
            HttpRequestMessage request)
    {
        return Policy.HandleResult<HttpResponseMessage>(
                response => response.StatusCode == HttpStatusCode.Unauthorized)
            .RetryAsync(
                retryCount: 1,
                onRetry: async (_, __, ___) =>
                    await RefreshTokenAsync(serviceProvider, request));
    }

    private static async Task RefreshTokenAsync(
        IServiceProvider serviceProvider,
        HttpRequestMessage request)
    {
        var tokenService =
            serviceProvider.GetRequiredService<ISalesforceTokenService>();

        await tokenService.RefreshAccessTokenAsync();

        request.SetPolicyExecutionContext([]);
    }
}