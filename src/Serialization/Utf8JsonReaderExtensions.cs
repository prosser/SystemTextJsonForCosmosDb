namespace SystemTextJsonForCosmosDb.Serialization;

using System.Runtime.CompilerServices;

internal static class Utf8JsonReaderExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ReadRequired(ref this Utf8JsonReader reader)
    {
        if (!reader.Read())
        {
            throw new JsonException($"Unexpected end of JSON at index {reader.TokenStartIndex}.");
        }
    }

    /// <summary>
    /// Reads the next token and throws if the new token is not the required token type.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static JsonTokenType ReadRequired(ref this Utf8JsonReader reader, JsonTokenType requiredTokenType)
    {
        reader.ReadRequired();
        return reader.RequireTokenType(requiredTokenType);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static JsonTokenType ReadRequired(ref this Utf8JsonReader reader, JsonTokenType requiredTokenType, JsonTokenType alternateTokenType)
    {
        reader.ReadRequired();
        return reader.RequireTokenType(requiredTokenType, alternateTokenType);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static JsonTokenType RequireTokenType(ref this Utf8JsonReader reader, JsonTokenType requiredTokenType)
    {
        return reader.TokenType != requiredTokenType
            ? throw new JsonException($"Expected {requiredTokenType} token but read {reader.TokenType} at index {reader.TokenStartIndex}")
            : reader.TokenType;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static JsonTokenType RequireTokenType(ref this Utf8JsonReader reader, JsonTokenType requiredTokenType, JsonTokenType alternateTokenType)
    {
        return reader.TokenType != requiredTokenType && reader.TokenType != alternateTokenType
            ? throw new JsonException($"Expected {requiredTokenType} or {alternateTokenType} token but read {reader.TokenType} at index {reader.TokenStartIndex}")
            : reader.TokenType;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static JsonTokenType RequireTokenType(ref this Utf8JsonReader reader, JsonTokenType requiredTokenType, params JsonTokenType[] alternateTokenTypes)
    {
        JsonTokenType tokenType = reader.TokenType;
        if (tokenType == requiredTokenType)
        {
            return tokenType;
        }

        foreach (JsonTokenType t in alternateTokenTypes)
        {
            if (t == tokenType)
            {
                return t;
            }
        }

        throw new JsonException($"Expected one of [{requiredTokenType},{string.Join(",", alternateTokenTypes)}] tokens but read {reader.TokenType} at index {reader.TokenStartIndex}");
    }
}