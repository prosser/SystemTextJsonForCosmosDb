namespace SystemTextJsonForCosmosDb.UnitTests.TestDoubles;

using SystemTextJsonForCosmosDb.Serialization;

public class TestCosmosSerializer : SystemTextJsonSerializer
{
    public TestCosmosSerializer()
        : base(CosmosJsonSerializerOptionsFactory.Default) { }
}
