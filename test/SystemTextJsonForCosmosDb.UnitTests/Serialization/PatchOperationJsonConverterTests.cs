namespace SystemTextJsonForCosmosDb.UnitTests.Serialization;

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.Azure.Cosmos;
using SystemTextJsonForCosmosDb.Serialization;
using Xunit;

public class PatchOperationJsonConverterTests
{
    [Flags]
    public enum TestDataType
    {
        Invalid = 0,
        IncludeNull = 0x1,
        IncludeNotNull = 0x2,

        All = IncludeNotNull | IncludeNull
    }

    public static IEnumerable<object?[]> GetBadJson()
    {
        yield return new object?[] { """{"path":"foo","op":"remove","value":{"prop":1}}""", "No value is permitted for operation type 'remove'." };
        yield return new object?[] { """{"path":"foo","operationType":"remove"}""", "Unexpected property 'operationType'" };

        yield return new object?[] { """{"op":"remove","value":1.23456}""", "Expected property 'path' is not present." };
        yield return new object?[] { """{"path":"foo","value":1.23456}""", "Expected property 'op' is not present." };
        yield return new object?[] { """{"path":"foo","op":"move","value":1.23456}""", "Expected property 'from' is not present." };

        yield return new object?[] { """{"path":"foo","op":"incr","value":"string"}""", "A numeric value is required for operation type 'incr'." };
        yield return new object?[] { """{"path":"foo","op":"incr"}""", "A numeric value is required for operation type 'incr'." };

        yield return new object?[] { """{"path":"foo","op":"invalid","value":1.23456}""", "'invalid' is not a valid enumeration value for 'op'" };
        yield return new object?[] { """{"path":"foo","op":"invalid"}""", "'invalid' is not a valid enumeration value for 'op'" };

        yield return new object?[] { """{"path":"foo","op":"remove","extra": 1}""", "Unexpected property 'extra'" };
        yield return new object?[] { """{"path":"foo","op":"remove","value":"foo"}""", "No value is permitted for operation type 'remove'." };
        yield return new object?[] { """{"path":"foo","op":"remove","value":1.23456}""", "No value is permitted for operation type 'remove'." };
        yield return new object?[] { """{"path":"foo","op":"remove","value":1}""", "No value is permitted for operation type 'remove'." };

        yield return new object?[] { """{"Path":"foo","op":"Remove"}""", "Unexpected property 'Path'" };
    }

    public static IEnumerable<object?[]> GetPatchOperations()
    {
        string maxDouble = double.MaxValue.ToString();
        string minDouble = double.MinValue.ToString();
        string maxLong = long.MaxValue.ToString();
        string minLong = long.MinValue.ToString();

        yield return new object?[] { PatchOperation.Add("a1", 1.23456), """{"path":"a1","op":"add","value":1.23456}""" };
        yield return new object?[] { PatchOperation.Add("a2", 1), """{"path":"a2","op":"add","value":1}""" };
        yield return new object?[] { PatchOperation.Add("a3", double.MaxValue), """{"path":"a3","op":"add","value":""" + maxDouble + "}" };
        yield return new object?[] { PatchOperation.Add("a4", double.MinValue), """{"path":"a4","op":"add","value":""" + minDouble + "}" };
        yield return new object?[] { PatchOperation.Add("a5", long.MaxValue), """{"path":"a5","op":"add","value":""" + maxLong + "}" };
        yield return new object?[] { PatchOperation.Add("a6", long.MinValue), """{"path":"a6","op":"add","value":""" + minLong + "}" };
        yield return new object?[] { PatchOperation.Add("a7", new[] { "item1", "item2" }), """{"path":"a7","op":"add","value":["item1","item2"]}""" };
        yield return new object?[] { PatchOperation.Add("a8", "string"), """{"path":"a8","op":"add","value":"string"}""" };

        yield return new object?[] { PatchOperation.Replace("r1", "foo"), """{"path":"r1","op":"replace","value":"foo"}""" };
        yield return new object?[] { PatchOperation.Replace("r2", 1), """{"path":"r2","op":"replace","value":1}""" };
        yield return new object?[] { PatchOperation.Replace("r3", 1.23456), """{"path":"r3","op":"replace","value":1.23456}""" };
        yield return new object?[] { PatchOperation.Replace("r4", double.MaxValue), """{"path":"r4","op":"replace","value":""" + maxDouble + "}" };
        yield return new object?[] { PatchOperation.Replace("r5", double.MinValue), """{"path":"r5","op":"replace","value":""" + minDouble + "}" };
        yield return new object?[] { PatchOperation.Replace("r6", long.MaxValue), """{"path":"r6","op":"replace","value":""" + maxLong + "}" };
        yield return new object?[] { PatchOperation.Replace("r7", long.MinValue), """{"path":"r7","op":"replace","value":""" + minLong + "}" };
        yield return new object?[] { PatchOperation.Replace("r8", new { prop1 = "prop1", prop2 = "prop2" }), """{"path":"r8","op":"replace","value":{"prop1":"prop1","prop2":"prop2"}}""" };
        yield return new object?[] { PatchOperation.Replace("r9", new[] { "item1", "item2" }), """{"path":"r9","op":"replace","value":["item1","item2"]}""" };

        yield return new object?[] { PatchOperation.Increment("i1", 1), """{"path":"i1","op":"incr","value":1}""" };
        yield return new object?[] { PatchOperation.Increment("i2", 2), """{"path":"i2","op":"incr","value":2}""" };
        yield return new object?[] { PatchOperation.Increment("i3", double.MaxValue), """{"path":"i3","op":"incr","value":""" + maxDouble + "}" };

        yield return new object?[] { PatchOperation.Remove("x1"), """{"path":"x1","op":"remove"}""" };

        yield return new object?[] { PatchOperation.Move("foo/bar", "m1"), """{"from":"foo/bar","path":"m1","op":"move"}""" };

        yield return new object?[] { PatchOperation.Set("s1", "string"), """{"path":"s1","op":"set","value":"string"}""" };
        yield return new object?[] { PatchOperation.Set("s2", 1), """{"path":"s2","op":"set","value":1}""" };
        yield return new object?[] { PatchOperation.Set("s3", 1.23456), """{"path":"s3","op":"set","value":1.23456}""" };
        yield return new object?[] { PatchOperation.Set("s4", new { prop1 = "prop1", prop2 = "prop2" }), """{"path":"s4","op":"set","value":{"prop1":"prop1","prop2":"prop2"}}""" };
        yield return new object?[] { PatchOperation.Set("s5", new[] { "item1", "item2" }), """{"path":"s5","op":"set","value":["item1","item2"]}""" };
    }

    [Theory]
    [MemberData(nameof(GetPatchOperations))]
    public void PatchOperationJsonConverterRead(PatchOperation expected, string json)
    {
        // Arrange
        Utf8JsonReader reader = new(Encoding.UTF8.GetBytes(json));
        JsonSerializerOptions options = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

        PatchOperationJsonConverter uut = new(new SystemTextJsonSerializer(options));

        _ = reader.Read();

        using MemoryStream memory = new();
        Utf8JsonWriter writer = new(memory);

        // Act
        uut.Write(writer, expected, options);
        writer.Flush();
        string serialized = Encoding.UTF8.GetString(memory.ToArray());

        PatchOperation? deserialized = uut.Read(ref reader, typeof(PatchOperation), options);

        // Assert
        _ = deserialized.Should().NotBeNull();
        _ = serialized.Should().Be(json);
    }

    [Fact]
    public void PatchOperationType_ToEnumMemberString_ThrowsOnUnknownValue()
    {
        // Arrange
        var op = (PatchOperationType)int.MaxValue;

        // Act
        Action act = () => op.ToEnumMemberString();

        // Assert
        _ = act.Should().ThrowExactly<ArgumentException>();
    }

    [Theory]
    [MemberData(nameof(GetBadJson))]
    public void ReadThrowsOnBadJson(string json, string expectedExceptionMessage)
    {
        // Arrange
        JsonSerializerOptions options = new();
        var converter = new PatchOperationJsonConverter(new SystemTextJsonSerializer(options));

        // Act
        Action act = () =>
        {
            Utf8JsonReader reader = new(Encoding.UTF8.GetBytes(json));
            _ = reader.Read();
            _ = converter.Read(ref reader, typeof(PatchOperation), options);
        };

        _ = act.Should().Throw<JsonException>().WithMessage(expectedExceptionMessage);
    }
}