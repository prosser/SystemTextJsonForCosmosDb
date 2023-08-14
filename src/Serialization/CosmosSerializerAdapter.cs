namespace SystemTextJsonForCosmosDb.Serialization;

using System.IO;
using System.Text;
using SystemTextJsonForCosmosDb;

/// <summary>
/// An adapter for <see cref="CosmosSerializer"/> that allows implementation of <see cref="ICosmosSerializer"/> in an assembly that has no Cosmos SDK references.
/// </summary>
internal sealed class CosmosSerializerAdapter : CosmosSerializer, ICosmosSerializer
{
    private readonly ICosmosSerializer serializer;

    /// <summary>
    /// Initializes a new instance of <see cref="CosmosSerializerAdapter"/>.
    /// </summary>
    /// <param name="serializer">Instance of <see cref="ICosmosSerializer"/> to use for serialization operations.</param>
    public CosmosSerializerAdapter(ICosmosSerializer serializer)
    {
        this.serializer = serializer;
    }

    /// <inheritdoc/>
    public T? FromJson<T>(string json)
    {
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(json));
        return serializer.FromStream<T>(stream);
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
        return serializer.FromStream<T>(stream);
    }

#nullable enable

    /// <inheritdoc/>
    public T FromStreamRequired<T>(Stream stream)
    {
        return FromStream<T>(stream) ?? throw new InvalidDataException("Deserialization produced a null result");
    }

    /// <inheritdoc/>
    public string ToJson<T>(T obj)
    {
        using var stream = serializer.ToStream(obj);
        using StreamReader reader = new(stream);
        return reader.ReadToEnd();
    }

    /// <inheritdoc/>
    public override Stream ToStream<T>(T obj)
    {
        return serializer.ToStream(obj);
    }
}