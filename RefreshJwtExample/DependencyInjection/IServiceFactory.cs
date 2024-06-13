namespace RefreshJwtExample.DependencyInjection;

public interface IServiceFactory<TService>
{
    TService Create();
}
