using Microsoft.AspNetCore.Authentication.BearerToken;

namespace RefreshJwtExample.Http;

public class SalesforceTokenClient(
    HttpClient _httpClient) : ISalesforceTokenClient
{
    public async Task<string> GetAccessTokenAsync()
    {
        var response = await _httpClient.GetAsync("oauth/token");

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException("Failed to get access token");
        }

        var content =
            await response.Content.ReadFromJsonAsync<AccessTokenResponse>()
            ?? throw new InvalidOperationException(
                "Failed to parse access token");

        return content.AccessToken;
    }
}