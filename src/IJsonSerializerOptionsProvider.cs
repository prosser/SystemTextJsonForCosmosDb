namespace SystemTextJsonForCosmosDb;

using System.Text.Json;
using SystemTextJsonForCosmosDb.Serialization;

/// <summary>
/// Interface for providing <see cref="JsonSerializerOptions"/> to the default <see cref="SystemTextJsonSerializer"/>.
/// </summary>
public interface IJsonSerializerOptionsProvider
{
    /// <summary>
    /// Gets the <see cref="JsonSerializerOptions"/>.
    /// </summary>
    /// <returns>The <see cref="JsonSerializerOptions"/>.</returns>
    JsonSerializerOptions GetJsonSerializerOptions();
}