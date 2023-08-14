namespace SystemTextJsonForCosmosDb.Serialization;

using System.IO;
using System.Text.Json;
using Microsoft.Azure.Cosmos;
using SystemTextJsonForCosmosDb;

/// <inheritdoc/>
public class SystemTextJsonSerializer : CosmosSerializer, ICosmosSerializer
{
    private readonly JsonSerializerOptions serializerOptions;

    /// <summary>
    /// Creates a new instance of the <see cref="SystemTextJsonSerializer"/> class using the default serializer config.
    /// </summary>
    public SystemTextJsonSerializer()
    {
        serializerOptions = CosmosJsonSerializerOptionsFactory.Default;
    }

    /// <summary>Creates Cosmos DB-compatible <see cref="JsonSerializerOptions"/>.</summary>
    /// <param name="serializerOptions">
    /// The base config which will be overridden where necessary for Cosmos DB compatibility.
    /// </param>
    public SystemTextJsonSerializer(JsonSerializerOptions? serializerOptions)
    {
        this.serializerOptions = CosmosJsonSerializerOptionsFactory.Create(serializerOptions);
    }

    private SystemTextJsonSerializer(JsonSerializerOptions serializerOptions, bool _)
    {
        this.serializerOptions = serializerOptions;
    }

    /// <inheritdoc/>
    public T? FromJson<T>(string json)
    {
        return JsonSerializer.Deserialize<T>(json, serializerOptions);
    }

    /// <inheritdoc/>
    public T FromJsonRequired<T>(string json)
    {
        return FromJson<T>(json) ?? throw new InvalidDataException("Deserialization produced a null result");
    }

#nullable disable

    /// <inheritdoc/>
    public override T FromStream<T>(Stream stream)
    {
        try
        {
            return JsonSerializer.Deserialize<T>(stream, serializerOptions)!;
        }
        finally
        {
            stream.Dispose();
        }
    }

#nullable enable

    /// <inheritdoc/>
    public T FromStreamRequired<T>(Stream stream)
    {
        return FromStream<T>(stream) ?? throw new InvalidDataException("Deserialization produced a null result");
    }

    /// <summary>
    /// Gets a copy of the <see cref="JsonSerializerOptions"/> used for the serializer.
    /// </summary>
    /// <returns>A new instance of <see cref="JsonSerializerOptions"/>.</returns>
    public JsonSerializerOptions GetSerializerOptions()
    {
        // make a new instance to prevent callers from mutating our settings.
        return new(serializerOptions);
    }

    /// <inheritdoc/>
    public string ToJson<T>(T document)
    {
        return JsonSerializer.Serialize(document, serializerOptions);
    }

    /// <inheritdoc/>
    public override Stream ToStream<T>(T input)
    {
        var stream = new MemoryStream();
        JsonSerializer.Serialize(stream, input, serializerOptions);
        stream.Position = 0;
        return stream;
    }

    internal static SystemTextJsonSerializer CreateWithSpecificOptions(JsonSerializerOptions serializerOptions)
    {
        return new(serializerOptions, true);
    }
}