namespace SystemTextJsonForCosmosDb.Serialization;
internal static class CosmosJsonSerializerOptionsFactory
{
    public static JsonSerializerOptions Default { get; } = CreateInner(null);

    public static JsonSerializerOptions Create(JsonSerializerOptions? baseOptions = null)
    {
        return baseOptions is null ? Default : CreateInner(baseOptions);
    }

    private static JsonSerializerOptions CreateInner(JsonSerializerOptions? baseOptions)
    {
        baseOptions ??= new()
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            MaxDepth = 64,
            NumberHandling = JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.AllowNamedFloatingPointLiterals,
            DefaultIgnoreCondition = JsonIgnoreCondition.Never,
            IgnoreReadOnlyProperties = false,
            WriteIndented = false,
        };

        // Create a new options object to avoid mutation of the one they passed in.
        // In .NET 8, there's a freezing method, but not in NetStandard.
        JsonSerializerOptions options = new(baseOptions)
        {
            PropertyNamingPolicy = new CosmosDocumentPropertyNamingPolicy(baseOptions.PropertyNamingPolicy),
        };

        // ensure that our converters are FIRST. Cosmos DB is opinionated about dates.
        // reverse because it's more intuitive to have this declaration in priority order and always insert at index 0.
        // The ordering here is important because the first converter that matches will be used.
        IEnumerable<(Type, JsonConverter)> converters = new (Type, JsonConverter)[]
        {
            (typeof(CosmosDateTimeOffsetJsonConverter), new CosmosDateTimeOffsetJsonConverter()),
            (typeof(CosmosDateTimeJsonConverter), new CosmosDateTimeJsonConverter()),
            (typeof(JsonStringEnumConverter), new JsonStringEnumConverter()),
            (typeof(CosmosNullableDateTimeOffsetJsonConverter), new CosmosNullableDateTimeOffsetJsonConverter()),
            (typeof(CosmosNullableDateTimeJsonConverter), new CosmosNullableDateTimeJsonConverter()),
            // use the static factory method on the serializer to avoid a circular dependency
            (typeof(PatchOperationJsonConverter), new PatchOperationJsonConverter(SystemTextJsonSerializer.CreateWithSpecificOptions(options))),
        }.Reverse();

        HashSet<Type> existingConverterTypes = new(options.Converters.Select(x => x.GetType()));

        foreach ((Type type, JsonConverter instance) in converters)
        {
            if (!existingConverterTypes.Contains(type))
            {
                options.Converters.Insert(0, instance);
            }
        }

        return options;
    }
}