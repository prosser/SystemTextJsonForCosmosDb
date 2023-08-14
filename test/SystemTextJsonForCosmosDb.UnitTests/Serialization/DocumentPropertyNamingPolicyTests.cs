namespace SystemTextJsonForCosmosDb.UnitTests.Serialization;

using System.Collections.Generic;
using System.Text.Json;
using FluentAssertions;
using SystemTextJsonForCosmosDb.Serialization;
using Xunit;

public class CosmosDocumentPropertyNamingPolicyTests
{
    public static IEnumerable<object?[]> CreateTestData()
    {
        yield return new object?[] { null };
        yield return new object?[] { JsonNamingPolicy.CamelCase };
        yield return new object?[] { new CustomOverridingNamingPolicy() };
    }

    public static IEnumerable<object?[]> GetTestData()
    {
        yield return new object?[] { null, "Foo" };
        yield return new object?[] { JsonNamingPolicy.CamelCase, "foo" };
        yield return new object?[] { new CustomOverridingNamingPolicy(), "Bar" };
    }

    [Theory]
    [MemberData(nameof(CreateTestData))]
    public void CanCreate(JsonNamingPolicy? baseNamingPolicy)
    {
        var policyOne = new CosmosDocumentPropertyNamingPolicy(baseNamingPolicy);
        _ = policyOne.Should().NotBeNull();
    }

    [Theory]
    [MemberData(nameof(CreateTestData))]
    public void ConvertName_AlwaysOverridesReservedPropertyNames(JsonNamingPolicy? baseNamingPolicy)
    {
        // Arrange
        CosmosDocumentPropertyNamingPolicy uut = new(baseNamingPolicy);

        // Act
        string id = uut.ConvertName("Id");
        string eTag = uut.ConvertName("ETag");

        // Assert
        _ = id.Should().Be(Constants.DocumentIdPropertyName);
        _ = eTag.Should().Be(Constants.DocumentETagPropertyName);
    }

    [Theory]
    [MemberData(nameof(GetTestData))]
    public void NamingPolicy_Handles_SpecialProperties(JsonNamingPolicy? baseNamingPolicy, string expectedFoo)
    {
        // Arrange
        CosmosDocumentPropertyNamingPolicy uut = new(baseNamingPolicy);

        // Act
        string eTag = uut.ConvertName("ETag");
        string foo = uut.ConvertName("Foo");

        // Assert
        _ = eTag.Should().Be("_etag");
        _ = foo.Should().Be(expectedFoo);
    }

    private class CustomOverridingNamingPolicy : JsonNamingPolicy
    {
        public override string ConvertName(string name)
        {
            return name switch
            {
                "Foo" => "Bar",
                _ => CamelCase.ConvertName(name)
            };
        }
    }
}