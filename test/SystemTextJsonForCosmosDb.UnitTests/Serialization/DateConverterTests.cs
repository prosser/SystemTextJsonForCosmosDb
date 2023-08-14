namespace SystemTextJsonForCosmosDb.UnitTests.Serialization;

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using SystemTextJsonForCosmosDb.Serialization;
using Xunit;

public class CosmosDateConverterTests
{
    private const string NotNullJsonNumber = "637766606456789012";
    private const string NotNullJsonString = "2022-01-02T03:04:05.6789012Z";
    private const string NullJson = "null";

    [Flags]
    public enum TestDataType
    {
        Invalid = 0,
        IncludeNull = 0x1,
        IncludeNotNull = 0x2,
        IncludeTicks = 0x4,

        All = IncludeNotNull | IncludeNull | IncludeTicks,
    }

    public static IEnumerable<object?[]> GetDateTimeJson(TestDataType type)
    {
        foreach (object?[] data in GetDateTimes(type))
        {
            yield return new object?[] { data[1] };
        }
    }

    public static IEnumerable<object?[]> GetDateTimeOffsetJson(TestDataType type)
    {
        foreach (object?[] data in GetDateTimeOffsets(type))
        {
            yield return new object?[] { data[1] };
        }
    }

    public static IEnumerable<object?[]> GetDateTimeOffsets(TestDataType type)
    {
        DateTimeOffset value = new(2022, 1, 2, 3, 4, 5, 678, TimeSpan.Zero);
        string stringValue = value.ToString(Constants.CosmosDateFormat);

        if (type.HasFlag(TestDataType.IncludeNotNull))
        {
            yield return new object?[] { value, "\"" + stringValue + "\"" };
        }

        if (type.HasFlag(TestDataType.IncludeTicks))
        {
            yield return new object?[] { value, value.Ticks.ToString() };
        }

        if (type.HasFlag(TestDataType.IncludeNull))
        {
            yield return new object?[] { null, "null" };
        }
    }

    public static IEnumerable<object?[]> GetDateTimes(TestDataType type)
    {
        DateTime value = new(2022, 1, 2, 3, 4, 5, 678, DateTimeKind.Utc);
        string stringValue = value.ToString(Constants.CosmosDateFormat);
        if (type.HasFlag(TestDataType.IncludeNotNull))
        {
            yield return new object?[] { value, "\"" + stringValue + "\"" };
        }

        if (type.HasFlag(TestDataType.IncludeTicks))
        {
            yield return new object?[] { value, value.Ticks.ToString() };
        }

        if (type.HasFlag(TestDataType.IncludeNull))
        {
            yield return new object?[] { null, "null" };
        }
    }

    [Theory]
    [MemberData(nameof(GetDateTimes), TestDataType.IncludeNotNull | TestDataType.IncludeTicks)]
    public void CosmosDateTimeJsonConverterRead(DateTime expected, string json)
    {
        // Arrange
        Utf8JsonReader reader = new(Encoding.UTF8.GetBytes(json));
        var converter = new CosmosDateTimeJsonConverter();

        _ = reader.Read();

        // Act
        DateTime? uut = converter.Read(ref reader, typeof(DateTime?), new());

        // Assert
        _ = uut.Should().NotBeNull();
        _ = uut.Value.ToUniversalTime().Should().Be(expected);
    }

    [Theory]
    [MemberData(nameof(GetDateTimeJson), TestDataType.IncludeNull)]
    public void CosmosDateTimeJsonConverterReadNullThrows(string json)
    {
        // Arrange
        var converter = new CosmosDateTimeJsonConverter();

        // Act
        Action act = () =>
        {
            Utf8JsonReader reader = new(Encoding.UTF8.GetBytes(json));
            _ = reader.Read();

            _ = converter.Read(ref reader, typeof(DateTime), new());
        };

        // Assert
        _ = act.Should().Throw<JsonException>();
    }

    [Theory]
    [MemberData(nameof(GetDateTimes), TestDataType.IncludeNotNull)]
    public void CosmosDateTimeJsonConverterWrite(DateTime value, string expected)
    {
        // Arrange
        using MemoryStream stream = new();
        Utf8JsonWriter writer = new(stream);
        var converter = new CosmosDateTimeJsonConverter();

        // Act
        converter.Write(writer, value, new());

        writer.Flush();
        stream.Flush();
        stream.Position = 0;

        string actual = Encoding.UTF8.GetString(stream.ToArray());

        // Assert
        _ = actual.Should().Be(expected);
    }

    [Theory]
    [MemberData(nameof(GetDateTimeOffsets), TestDataType.IncludeNotNull | TestDataType.IncludeTicks)]
    public void CosmosDateTimeOffsetJsonConverterRead(DateTimeOffset expected, string json)
    {
        // Arrange
        Utf8JsonReader reader = new(Encoding.UTF8.GetBytes(json));
        var converter = new CosmosDateTimeOffsetJsonConverter();

        _ = reader.Read();

        // Act
        DateTimeOffset uut = converter.Read(ref reader, typeof(DateTimeOffset), new());

        // Assert
        _ = uut.Should().Be(expected);
    }

    [Theory]
    [MemberData(nameof(GetDateTimeOffsetJson), TestDataType.IncludeNull)]
    public void CosmosDateTimeOffsetJsonConverterReadNullThrows(string json)
    {
        // Arrange
        var converter = new CosmosDateTimeJsonConverter();

        // Act
        Action act = () =>
        {
            Utf8JsonReader reader = new(Encoding.UTF8.GetBytes(json));
            _ = reader.Read();

            _ = converter.Read(ref reader, typeof(DateTimeOffset), new());
        };

        // Assert
        _ = act.Should().ThrowExactly<JsonException>();
    }

    [Theory]
    [MemberData(nameof(GetDateTimeOffsets), TestDataType.IncludeNotNull)]
    public void CosmosDateTimeOffsetJsonConverterWrite(DateTimeOffset value, string expected)
    {
        // Arrange
        using MemoryStream stream = new();
        Utf8JsonWriter writer = new(stream);

        // Act
        var converter = new CosmosDateTimeOffsetJsonConverter();
        converter.Write(writer, value, new());

        writer.Flush();
        stream.Flush();
        stream.Position = 0;

        string actual = Encoding.UTF8.GetString(stream.ToArray());

        // Assert
        _ = actual.Should().Be(expected);
    }

    [Theory]
    [MemberData(nameof(GetDateTimeOffsets), TestDataType.All)]
    public void CosmosNullableDateOffsetTimeJsonConverterRead(DateTimeOffset? expected, string json)
    {
        // Arrange
        Utf8JsonReader reader = new(Encoding.UTF8.GetBytes(json));
        var converter = new CosmosNullableDateTimeOffsetJsonConverter();

        _ = reader.Read();

        // Act
        DateTimeOffset? uut = converter.Read(ref reader, typeof(DateTimeOffset?), new());

        // Assert
        if (expected.HasValue)
        {
            _ = uut.Should().NotBeNull();
            _ = uut!.Value.Should().Be(expected);
        }
        else
        {
            _ = uut.Should().BeNull();
        }
    }

    [Theory]
    [MemberData(nameof(GetDateTimes), TestDataType.All)]
    public void CosmosNullableDateTimeJsonConverterRead(DateTime? expected, string json)
    {
        // Arrange
        Utf8JsonReader reader = new(Encoding.UTF8.GetBytes(json));
        var converter = new CosmosNullableDateTimeJsonConverter();

        _ = reader.Read();

        // Act
        DateTime? uut = converter.Read(ref reader, typeof(DateTime?), new());

        // Assert
        if (expected.HasValue)
        {
            _ = uut.Should().NotBeNull();
            _ = uut!.Value.ToUniversalTime().Should().Be(expected);
        }
        else
        {
            _ = uut.Should().BeNull();
        }
    }

    [Theory]
    [MemberData(nameof(GetDateTimes), TestDataType.IncludeNotNull | TestDataType.IncludeNull)]
    public void CosmosNullableDateTimeJsonConverterWrite(DateTime? value, string expected)
    {
        // Arrange
        using MemoryStream stream = new();
        Utf8JsonWriter writer = new(stream);
        var converter = new CosmosNullableDateTimeJsonConverter();

        // Act
        converter.Write(writer, value, new());

        writer.Flush();
        stream.Flush();
        stream.Position = 0;

        string actual = Encoding.UTF8.GetString(stream.ToArray());

        // Assert
        _ = actual.Should().Be(expected);
    }

    [Theory]
    [MemberData(nameof(GetDateTimeOffsets), TestDataType.IncludeNotNull | TestDataType.IncludeNull)]
    public void CosmosNullableDateTimeOffsetJsonConverterWrite(DateTimeOffset? value, string expected)
    {
        // Arrange
        using MemoryStream stream = new();
        Utf8JsonWriter writer = new(stream);
        var converter = new CosmosNullableDateTimeOffsetJsonConverter();

        // Act
        converter.Write(writer, value, new());

        writer.Flush();
        stream.Flush();
        stream.Position = 0;

        string actual = Encoding.UTF8.GetString(stream.ToArray());

        // Assert
        _ = actual.Should().Be(expected);
    }
}