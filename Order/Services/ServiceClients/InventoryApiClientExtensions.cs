using Order.Abstractions;

namespace Order.Services.ServiceClients;

public static class InventoryApiClientExtensions
{
    public static IServiceCollection AddInventoryApiClient(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddHttpClient<IInventoryApiClient, InventoryApiClient>(client =>
        {
            client.BaseAddress = new("https+http://inventory/");
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        return services;
    }
}