namespace SystemTextJsonForCosmosDb;

using System.Text.Json;

public class JsonSerializerOptionsProvider : IJsonSerializerOptionsProvider
{
    private readonly JsonSerializerOptions options;

    public JsonSerializerOptionsProvider(JsonSerializerOptions options)
    {
        this.options = options;
    }

    public JsonSerializerOptions GetJsonSerializerOptions()
    {
        return options;
    }
}