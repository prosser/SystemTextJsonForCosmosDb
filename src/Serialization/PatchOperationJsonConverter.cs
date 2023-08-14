namespace SystemTextJsonForCosmosDb.Serialization;
using System.IO;
using static PatchConstants.OperationTypeNames;

internal class PatchOperationJsonConverter : JsonConverter<PatchOperation>
{
    private readonly CosmosSerializer cosmosSerializer;

    public PatchOperationJsonConverter(CosmosSerializer serializer)
    {
        cosmosSerializer = serializer;
    }

    public override PatchOperation? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        _ = reader.ReadRequired(JsonTokenType.PropertyName);
        string? path = null;
        string? stringValue = null;
        double? doubleValue = null;
        long? longValue = null;
        JsonElement? objectValue = null;
        string? op = null;
        string? from = null;

        while (reader.TokenType != JsonTokenType.EndObject)
        {
            string propertyName = reader.GetString()!;
            switch (propertyName)
            {
                case PatchConstants.PropertyNames.Path:
                    _ = reader.ReadRequired(JsonTokenType.String);
                    path = reader.GetString()!;
                    break;

                case PatchConstants.PropertyNames.OperationType:
                    _ = reader.ReadRequired(JsonTokenType.String);
                    op = reader.GetString()!;
                    break;

                case PatchConstants.PropertyNames.Value:
                    reader.ReadRequired();
                    switch (reader.TokenType)
                    {
                        case JsonTokenType.String:
                        case JsonTokenType.Null:
                            stringValue = reader.GetString();
                            break;

                        case JsonTokenType.Number:
                            if (reader.TryGetInt64(out long l))
                            {
                                longValue = l;
                            }
                            else
                            {
                                doubleValue = reader.GetDouble();
                            }

                            break;

                        case JsonTokenType.StartObject:
                        case JsonTokenType.StartArray:
                            objectValue = JsonElement.ParseValue(ref reader);
                            break;
                    }

                    break;

                case PatchConstants.PropertyNames.From:
                    _ = reader.ReadRequired(JsonTokenType.String);
                    from = reader.GetString();
                    break;

                default:
                    throw new JsonException($"Unexpected property '{propertyName}'");
            }

            reader.ReadRequired();
        }

        // check for missing properties
        ThrowIfPropertyMissing(path, PatchConstants.PropertyNames.Path);
        ThrowIfPropertyMissing(op, PatchConstants.PropertyNames.OperationType);
        switch (op)
        {
            case Move:
                ThrowIfPropertyMissing(from, PatchConstants.PropertyNames.From);
                break;

            case Remove:
                ThrowIfValuePresent(op, () => objectValue is not null || stringValue is not null || longValue is not null || doubleValue is not null);
                break;
        }

        PatchOperation result = op switch
        {
            Add =>
                longValue.HasValue ? PatchOperation.Add(path, longValue) :
                doubleValue.HasValue ? PatchOperation.Add(path, doubleValue) :
                objectValue is not null ? PatchOperation.Add(path, objectValue) :
                PatchOperation.Add(path, stringValue),
            Remove => PatchOperation.Remove(path),
            Move => PatchOperation.Move(from, path),
            Replace =>
                longValue.HasValue ? PatchOperation.Replace(path, longValue) :
                doubleValue.HasValue ? PatchOperation.Replace(path, doubleValue) :
                objectValue is not null ? PatchOperation.Replace(path, objectValue) :
                PatchOperation.Replace(path, stringValue),
            Set => longValue.HasValue ? PatchOperation.Set(path, longValue) :
                doubleValue.HasValue ? PatchOperation.Set(path, doubleValue) :
                objectValue is not null ? PatchOperation.Set(path, objectValue) :
                PatchOperation.Set(path, stringValue),
            Increment => longValue.HasValue ? PatchOperation.Increment(path, longValue.Value) :
                doubleValue.HasValue ? PatchOperation.Increment(path, doubleValue.Value)
                : throw new JsonException($"A numeric value is required for operation type '{op}'."),
            _ => throw new JsonException($"'{op}' is not a valid enumeration value for '{PatchConstants.PropertyNames.OperationType}'"),
        };

        return result;
    }

    public override void Write(Utf8JsonWriter writer, PatchOperation po, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        if (po.From is not null)
        {
            writer.WritePropertyName(PatchConstants.PropertyNames.From);
            writer.WriteStringValue(po.From);
        }

        writer.WritePropertyName(PatchConstants.PropertyNames.Path);
        writer.WriteStringValue(po.Path);

        writer.WritePropertyName(PatchConstants.PropertyNames.OperationType);
        writer.WriteStringValue(po.OperationType.ToEnumMemberString());

        if (po.TrySerializeValueParameter(cosmosSerializer, out Stream? valueParam))
        {
            using (valueParam)
            {
                JsonElement elem = JsonSerializer.Deserialize<JsonElement>(valueParam, options);
                if (elem.ValueKind != JsonValueKind.Null)
                {
                    writer.WritePropertyName(PatchConstants.PropertyNames.Value);
                    elem.WriteTo(writer);
                }
            }
        }

        writer.WriteEndObject();
    }

    private static void ThrowIfPropertyMissing(string? property, string propertyName)
    {
        if (property is null)
        {
            throw new JsonException($"Expected property '{propertyName}' is not present.");
        }
    }

    private static void ThrowIfValuePresent(string operationType, Func<bool> predicate)
    {
        if (predicate())
        {
            throw new JsonException($"No value is permitted for operation type '{operationType}'.");
        }
    }
}