namespace SystemTextJsonForCosmosDb;

using SystemTextJsonForCosmosDb.Serialization;

public class CosmosClientOptionsProvider : ICosmosClientOptionsProvider
{
    private readonly ICosmosClientOptionsProvider? clientOptionsProvider;
    private readonly Func<CosmosSerializer> getSerializer;

    public CosmosClientOptionsProvider(
        ICosmosClientOptionsProvider clientOptionsProvider,
        ICosmosSerializer cosmosSerializer)
        : this(cosmosSerializer)
    {
        this.clientOptionsProvider = clientOptionsProvider;
    }

    public CosmosClientOptionsProvider(
        ICosmosClientOptionsProvider clientOptionsProvider,
        IJsonSerializerOptionsProvider jsonSerializerOptionsProvider)
        : this(jsonSerializerOptionsProvider)
    {
        this.clientOptionsProvider = clientOptionsProvider;
    }

    public CosmosClientOptionsProvider(ICosmosSerializer serializer)
    {
        getSerializer = () => new CosmosSerializerAdapter(serializer);
    }

    public CosmosClientOptionsProvider(IJsonSerializerOptionsProvider optionsProvider)
    {
        getSerializer = () => new SystemTextJsonSerializer(optionsProvider.GetJsonSerializerOptions());
    }

    public CosmosClientOptions GetCosmosClientOptions()
    {
        CosmosClientOptions options = clientOptionsProvider?.GetCosmosClientOptions()
            ?? new();

        options.Serializer = getSerializer();
        return options;
    }
}