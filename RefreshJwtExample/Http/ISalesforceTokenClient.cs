namespace RefreshJwtExample.Http;

public interface ISalesforceTokenClient
{
    Task<string> GetAccessTokenAsync();
}
