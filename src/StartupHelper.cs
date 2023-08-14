namespace SystemTextJsonForCosmosDb;

using Microsoft.Extensions.DependencyInjection;

public static class StartupHelper
{
    public static IServiceCollection AddSystemTextJsonForCosmosDb(this IServiceCollection services)
    {
        return services
            .AddSingleton(sp =>
            {
                ICosmosClientOptionsProvider? clientOptionsProvider = sp.GetService<ICosmosClientOptionsProvider>();
                IJsonSerializerOptionsProvider? jsonSerializerOptionsProvider = sp.GetService<IJsonSerializerOptionsProvider>();
                ICosmosSerializer? serializer = sp.GetService<ICosmosSerializer>();

                ICosmosClientOptionsProvider created = serializer is not null
                    ? clientOptionsProvider is not null
                        // prefer explicit ICosmosSerializer
                        ? new CosmosClientOptionsProvider(clientOptionsProvider, serializer)
                        : new CosmosClientOptionsProvider(serializer)
                    : jsonSerializerOptionsProvider is not null
                        // fall back to JsonSerializerOptions provider
                        ? clientOptionsProvider is not null
                            ? new CosmosClientOptionsProvider(clientOptionsProvider, jsonSerializerOptionsProvider)
                            : new CosmosClientOptionsProvider(jsonSerializerOptionsProvider)
                        // whoops! neither one was registered. Throw a useful exception.
                        : throw new NotSupportedException($"Could not create {nameof(ICosmosClientOptionsProvider)}: neither {nameof(IJsonSerializerOptionsProvider)} nor {nameof(ICosmosSerializer)} was registered.");

                return created;
            });
    }
}