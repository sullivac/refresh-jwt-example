namespace RefreshJwtExample.Services;

public interface ISalesforceTokenService
{
    Task<string?> GetAccessTokenAsync();

    Task RefreshAccessTokenAsync();
}