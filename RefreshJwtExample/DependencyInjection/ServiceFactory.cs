namespace RefreshJwtExample.DependencyInjection;

public class ServiceFactory<TService>(
    IServiceProvider serviceProvider) : IServiceFactory<TService>
    where TService : notnull
{
    public TService Create()
    {
        return serviceProvider.GetRequiredService<TService>();
    }
}
