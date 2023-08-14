namespace SystemTextJsonForCosmosDb.UnitTests.Serialization;

using System.IO;
using System.Text;
using System.Text.Json;
using AutoFixture.Xunit2;
using FluentAssertions;
using SystemTextJsonForCosmosDb.Serialization;
using SystemTextJsonForCosmosDb.UnitTests.TestDoubles;
using Xunit;

public class SystemTextJsonSerializerTests
{
    private readonly JsonSerializerOptions serializerOptions;

    public SystemTextJsonSerializerTests()
    {
        serializerOptions = CosmosJsonSerializerOptionsFactory.Create(new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
    }

    [Fact]
    public void FromJsonRequiredThrowsOnNull()
    {
        // Arrange
        SystemTextJsonSerializer uut = new(serializerOptions);

        // Act
        Action act = () => uut.FromJsonRequired<TestDocument>("null");

        // Assert
        _ = act.Should().ThrowExactly<InvalidDataException>();
    }

    [Fact]
    public void FromJsonRequiredWorksWhenNotNull()
    {
        // Arrange
        const string Json = """
{
    "id": "test",
    "partition": "partitionKey"
}
""";
        SystemTextJsonSerializer uut = new(serializerOptions);

        // Act
        TestDocument doc = uut.FromJsonRequired<TestDocument>(Json);

        // Assert
        _ = doc.Should().NotBeNull();
    }

    [Fact]
    public void FromStreamAllowsNull()
    {
        // Arrange
        const string Json = "null";
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(Json));

        SystemTextJsonSerializer uut = new(serializerOptions);

        // Act
        string? actual = uut.FromStream<string?>(stream);

        // Assert
        _ = actual.Should().BeNull();
    }

    [Theory, AutoData]
    public void FromStreamDeserializes(TestDocument expected)
    {
        // Arrange
        string json = JsonSerializer.Serialize(expected, serializerOptions);
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));

        SystemTextJsonSerializer uut = new(serializerOptions);

        // Act
        TestDocument? actual = uut.FromStream<TestDocument>(stream);

        // Assert
        _ = actual.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void ToJsonAllowsNull()
    {
        // Arrange
        SystemTextJsonSerializer uut = new(serializerOptions);

        // Act
        string? actual = uut.ToJson((TestDocument?)null);

        // Assert
        _ = actual.Should().Be("null");
    }

    [Theory, AutoData]
    public void ToStreamSerializes(TestDocument document)
    {
        // Arrange
        string expected = JsonSerializer.Serialize(document, serializerOptions);

        SystemTextJsonSerializer uut = new(serializerOptions);

        // Act
        using var stream = uut.ToStream(document);

        // Assert on stream
        _ = stream.Should().NotBeNull();
        _ = stream.Position.Should().Be(0);

        // Act on stream
        using StreamReader reader = new(stream);
        string actual = reader.ReadToEnd();

        // Assert results
        _ = actual.Should().Be(expected);
    }
}