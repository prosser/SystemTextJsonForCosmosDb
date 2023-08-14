namespace SystemTextJsonForCosmosDb;

using System.IO;

/// <summary>
/// Interface for a serializer that can convert objects to and from JSON compatible with Cosmos DB.
/// </summary>
/// <remarks>We're using a new interface rather than <see cref="CosmosSerializer"/> for two reasons:
/// <para>
/// 1. It's not very friendly to require the serializer implementor to reference Cosmos libraries.
/// </para>
/// <para>
/// 2. Serializers are often used outside the context of Cosmos operations, and it is useful to
/// be able to get the same JSON in those contexts, too.
/// </para></remarks>
public interface ICosmosSerializer
{
    /// <summary>
    /// Convert string JSON to an object.
    /// </summary>
    /// <typeparam name="T">The type of object to serialize.</typeparam>
    /// <param name="json">The JSON string to convert.</param>
    /// <returns>The object deserialized from the string.</returns>
    T? FromJson<T>(string json);

    /// <summary>
    /// Convert a JSON string to an object.
    /// </summary>
    /// <typeparam name="T">The type of object to serialize.</typeparam>
    /// <param name="json">JSON to convert</param>
    /// <returns>The object deserialized from the string.</returns>
    /// <exception cref="InvalidDataException">The stream contained <c>null</c> or could not deserialize.</exception>
    T FromJsonRequired<T>(string json);

    /// <summary>
    /// Convert a Stream of JSON to an object.
    /// The implementation is responsible for Disposing of the stream,
    /// including when an exception is thrown, to avoid memory leaks.
    /// </summary>
    /// <typeparam name="T">The type of object to serialize.</typeparam>
    /// <param name="stream">The Stream response containing JSON from Cosmos DB.</param>
    /// <returns>The object deserialized from the stream.</returns>
    T? FromStream<T>(Stream stream);

    /// <summary>
    /// Convert a Stream of JSON to an object.
    /// The implementation is responsible for Disposing of the stream,
    /// including when an exception is thrown, to avoid memory leaks.
    /// </summary>
    /// <typeparam name="T">The type of object to serialize.</typeparam>
    /// <param name="stream">The Stream response containing JSON from Cosmos DB.</param>
    /// <returns>The object deserialized from the stream.</returns>
    /// <exception cref="InvalidDataException">The stream contained <c>null</c> or could not deserialize.</exception>
    T FromStreamRequired<T>(Stream stream);

    /// <summary>
    /// Convert an object to a JSON string.
    /// </summary>
    /// <typeparam name="T">The type of object to serialize.</typeparam>
    /// <param name="obj">Object to convert.</param>
    /// <returns>The string JSON of the serialized object.</returns>
    string ToJson<T>(T obj);

    /// <summary>
    /// Convert an object to a Stream.
    /// The caller will take ownership of the stream and ensure it is correctly disposed of.
    /// <see href="https://docs.microsoft.com/dotnet/api/system.io.stream.canread">Stream.CanRead</see> must be true.
    /// </summary>
    /// <typeparam name="T">The type of object to serialize.</typeparam>
    /// <param name="input">Any type passed to <see cref="Container"/>.</param>
    /// <returns>A readable Stream containing JSON of the serialized object.</returns>
    Stream ToStream<T>(T document);
}
