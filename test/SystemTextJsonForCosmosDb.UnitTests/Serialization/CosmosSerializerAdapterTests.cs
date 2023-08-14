namespace SystemTextJsonForCosmosDb.UnitTests.Serialization;

using System;
using System.IO;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using SystemTextJsonForCosmosDb.Serialization;
using SystemTextJsonForCosmosDb.UnitTests.TestDoubles;
using Xunit;

public class CosmosSerializerAdapterTests
{
    [Fact]
    public void FromJson_ShouldReturnObject()
    {
        // Arrange
        var serializer = new TestCosmosSerializer();
        var proxy = new CosmosSerializerAdapter(serializer);

        // Act
        object? obj = proxy.FromJson<object>("{}");

        // Assert
        _ = obj.Should().NotBeNull();
    }

    [Fact]
    public void FromJson_ShouldThrowWhenInvalid()
    {
        // Arrange
        var serializer = new SystemTextJsonSerializer();
        var proxy = new CosmosSerializerAdapter(serializer);

        // Act
        Action act = () => proxy.FromJson<object>("");

        // Assert
        _ = act.Should().ThrowExactly<JsonException>().WithMessage("The input does not contain any JSON tokens.*");
    }

    [Fact]
    public void FromJsonRequired_ShouldReturnObject()
    {
        // Arrange
        var serializer = new TestCosmosSerializer();
        var proxy = new CosmosSerializerAdapter(serializer);

        // Act
        object obj = proxy.FromJsonRequired<object>("{}");

        // Assert
        _ = obj.Should().NotBeNull();
    }

    [Fact]
    public void FromJsonRequired_ShouldThrowExceptionWhenInvalid()
    {
        // Arrange
        var serializer = new TestCosmosSerializer();
        var proxy = new CosmosSerializerAdapter(serializer);

        // Act & Assert
        _ = proxy.Invoking(p => p.FromJsonRequired<object>("null")).Should().Throw<InvalidDataException>();
    }

    [Fact]
    public void FromStream_ShouldReturnObject()
    {
        // Arrange
        var serializer = new TestCosmosSerializer();
        var proxy = new CosmosSerializerAdapter(serializer);
        var stream = new MemoryStream(Encoding.UTF8.GetBytes("{}"));

        // Act
        object? obj = proxy.FromStream<object>(stream);

        // Assert
        _ = obj.Should().NotBeNull();
    }

    [Fact]
    public void FromStreamRequired_ShouldReturnObject()
    {
        // Arrange
        var serializer = new TestCosmosSerializer();
        var proxy = new CosmosSerializerAdapter(serializer);
        var stream = new MemoryStream(Encoding.UTF8.GetBytes("{}"));

        // Act
        object obj = proxy.FromStreamRequired<object>(stream);

        // Assert
        _ = obj.Should().NotBeNull();
    }

    [Fact]
    public void FromStreamRequired_ShouldThrowExceptionWhenInvalid()
    {
        // Arrange
        var serializer = new TestCosmosSerializer();
        var proxy = new CosmosSerializerAdapter(serializer);
        var stream = new MemoryStream(Encoding.UTF8.GetBytes("null"));

        // Act & Assert
        _ = proxy.Invoking(p => p.FromStreamRequired<object>(stream)).Should().Throw<InvalidDataException>();
    }

    [Fact]
    public void ToJson_ShouldReturnJsonString()
    {
        // Arrange
        var serializer = new TestCosmosSerializer();
        string expected = "{}";
        var proxy = new CosmosSerializerAdapter(serializer);

        // Act
        string actual = proxy.ToJson(new object());

        // Assert
        _ = actual.Should().Be(expected);
    }

    [Fact]
    public void ToStream_ShouldReturnStream()
    {
        // Arrange
        var serializer = new TestCosmosSerializer();
        var proxy = new CosmosSerializerAdapter(serializer);

        // Act
        var actual = proxy.ToStream(new object());

        // Assert
        _ = actual.Should().NotBeNull();
    }
}