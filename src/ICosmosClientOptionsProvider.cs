namespace SystemTextJsonForCosmosDb;

/// <summary>
/// A service that returns a configured <see cref="CosmosClientOptions"/> instance.
/// </summary>
public interface ICosmosClientOptionsProvider
{
    /// <summary>
    /// Gets a configured instance of <see cref="CosmosClientOptions"/>.
    /// </summary>
    /// <returns>The options.</returns>
    CosmosClientOptions GetCosmosClientOptions();
}
