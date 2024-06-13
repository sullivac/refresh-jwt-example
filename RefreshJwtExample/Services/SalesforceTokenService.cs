using Microsoft.Extensions.Caching.Memory;
using RefreshJwtExample.DependencyInjection;
using RefreshJwtExample.Http;

namespace RefreshJwtExample.Services;

public class SalesforceTokenService(
    IServiceFactory<ISalesforceTokenClient> _clientFactory,
    IMemoryCache _cache)
    : ISalesforceTokenService
{
    private const string AccessTokenKey = "SalesforceAccessToken";

    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public async Task<string?> GetAccessTokenAsync()
    {
        if (!_cache.TryGetValue(AccessTokenKey, out string? accessToken))
        {
            await RefreshAccessTokenAsync();

            accessToken = _cache.Get<string>(AccessTokenKey);
        }

        return accessToken;
    }

    public async Task RefreshAccessTokenAsync()
    {
        var currentToken = _cache.Get<string>(AccessTokenKey);

        await _semaphore.WaitAsync();

        try
        {
            if (currentToken != _cache.Get<string>(AccessTokenKey))
            {
                return;
            }

            var newToken = await GetTokenAsync();

            _cache.Set(AccessTokenKey, newToken);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private async Task<string> GetTokenAsync()
    {
        var client = _clientFactory.Create();

        return await client.GetAccessTokenAsync();
    }
}