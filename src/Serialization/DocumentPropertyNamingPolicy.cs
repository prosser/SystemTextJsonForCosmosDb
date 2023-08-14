namespace SystemTextJsonForCosmosDb.Serialization;
internal class CosmosDocumentPropertyNamingPolicy : JsonNamingPolicy
{
    private readonly JsonNamingPolicy? basePolicy;

    public CosmosDocumentPropertyNamingPolicy(JsonNamingPolicy? basePolicy)
    {
        // If the base policy is explicitly CamelCase, we don't need to wrap it.
        this.basePolicy = basePolicy;
    }

    public override string ConvertName(string name)
    {
        if (string.Equals("id", name, StringComparison.OrdinalIgnoreCase))
        {
            return Constants.DocumentIdPropertyName;
        }
        else if (string.Equals("etag", name, StringComparison.OrdinalIgnoreCase) || string.Equals("_etag", name, StringComparison.OrdinalIgnoreCase))
        {
            return Constants.DocumentETagPropertyName;
        }

        return basePolicy?.ConvertName(name) ?? name;
    }
}