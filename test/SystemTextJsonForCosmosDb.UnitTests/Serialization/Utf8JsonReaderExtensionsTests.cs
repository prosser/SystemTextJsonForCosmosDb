namespace SystemTextJsonForCosmosDb.UnitTests.Serialization;

using System.Text;
using System.Text.Json;
using SystemTextJsonForCosmosDb.Serialization;
using Xunit;

public class Utf8JsonReaderExtensionsTests
{
    private const string TestJson = @"{ ""test"": 1 }";

    [Fact]
    public void ReadRequired()
    {
        Utf8JsonReader reader = new(Encoding.UTF8.GetBytes(TestJson));
        _ = reader.ReadRequired(JsonTokenType.StartObject);

        Assert.Equal(JsonTokenType.StartObject, reader.TokenType);

        reader.ReadRequired();

        Assert.Equal(JsonTokenType.PropertyName, reader.TokenType);
        _ = reader.ReadRequired(JsonTokenType.Number, JsonTokenType.String);

        Assert.Equal(JsonTokenType.Number, reader.RequireTokenType(JsonTokenType.Number));
        Assert.Equal(JsonTokenType.Number, reader.RequireTokenType(JsonTokenType.String, JsonTokenType.Number));
        Assert.Equal(JsonTokenType.Number, reader.RequireTokenType(JsonTokenType.Number, JsonTokenType.String));
        Assert.Equal(JsonTokenType.Number, reader.RequireTokenType(JsonTokenType.Number, JsonTokenType.String, JsonTokenType.Null));
        Assert.Equal(JsonTokenType.Number, reader.RequireTokenType(JsonTokenType.String, JsonTokenType.Null, JsonTokenType.Number));

        reader.ReadRequired();
    }

    [Fact]
    public void ReadRequiredThrowsWhenNoTokensLeft()
    {
        _ = Assert.Throws<JsonException>(() =>
        {
            Utf8JsonReader reader = new(Encoding.UTF8.GetBytes(TestJson));
            _ = reader.ReadRequired(JsonTokenType.StartObject);

            Assert.Equal(JsonTokenType.StartObject, reader.TokenType);

            reader.ReadRequired();

            Assert.Equal(JsonTokenType.PropertyName, reader.TokenType);

            JsonTokenType tokenType = reader.ReadRequired(JsonTokenType.Number, JsonTokenType.String);

            Assert.Equal(JsonTokenType.Number, reader.RequireTokenType(JsonTokenType.Number));

            reader.ReadRequired();

            Assert.Equal(JsonTokenType.EndObject, reader.TokenType);
            reader.ReadRequired();
        });
    }

    [Fact]
    public void RequireTokenTypeThrowsOnUnexpectedTokenType()
    {
        _ = Assert.Throws<JsonException>(() =>
        {
            Utf8JsonReader reader = new(Encoding.UTF8.GetBytes(TestJson));
            _ = reader.ReadRequired(JsonTokenType.StartObject);

            _ = reader.RequireTokenType(JsonTokenType.EndArray);
        });

        _ = Assert.Throws<JsonException>(() =>
        {
            Utf8JsonReader reader = new(Encoding.UTF8.GetBytes(TestJson));
            _ = reader.ReadRequired(JsonTokenType.StartObject);

            _ = reader.RequireTokenType(JsonTokenType.EndArray, JsonTokenType.True);
        });

        _ = Assert.Throws<JsonException>(() =>
        {
            Utf8JsonReader reader = new(Encoding.UTF8.GetBytes(TestJson));
            _ = reader.ReadRequired(JsonTokenType.StartObject);

            _ = reader.RequireTokenType(JsonTokenType.EndArray, JsonTokenType.True, JsonTokenType.Null);
        });
    }
}