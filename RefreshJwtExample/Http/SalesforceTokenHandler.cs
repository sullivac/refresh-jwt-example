using System.Net.Http.Headers;
using Polly;
using RefreshJwtExample.Services;

namespace RefreshJwtExample.Http;

public class SalesforceTokenHandler(
    ISalesforceTokenService _tokenService) : DelegatingHandler
{
    private const string TokenRetrieval = "TokenRetrieval";
    private const string TokenKey = "Token";

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var context = await GetContext(request);

        var accessToken = context[TokenKey] as string;

        request.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", accessToken);

        return await base.SendAsync(request, cancellationToken);
    }

    private async Task<Context> GetContext(HttpRequestMessage request)
    {
        var context = request.GetPolicyExecutionContext();
        if (context is null || context.Count is 0)
        {
            var contextData = await GetContextData();

            context = new Context(TokenRetrieval, contextData);
        }

        return context;
    }

    private async Task<Dictionary<string, object>> GetContextData()
    {
        var accessToken =
            await _tokenService.GetAccessTokenAsync()
            ?? throw new InvalidOperationException("Access token is null.");

        return new Dictionary<string, object> { { TokenKey, accessToken } };
    }
}