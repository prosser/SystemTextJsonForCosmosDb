namespace SystemTextJsonForCosmosDb.Serialization;

using System.Globalization;

internal class CosmosNullableDateTimeOffsetJsonConverter : JsonConverter<DateTimeOffset?>
{
    public override DateTimeOffset? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        JsonTokenType tokenType = reader.RequireTokenType(JsonTokenType.String, JsonTokenType.Number, JsonTokenType.Null);

        if (tokenType == JsonTokenType.Null)
        {
            return null;
        }
        else if (tokenType == JsonTokenType.String)
        {
            string str = reader.GetString()!; // cannot be null because tokenType = String
            return DateTimeOffset.Parse(str, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);
        }
        else // if (token == JsonTokenType.Number)
        {
            // must be in UTC if stored as ticks
            long ticks = reader.GetInt64();
            return new DateTimeOffset(ticks, TimeSpan.Zero);
        }
    }

    public override void Write(Utf8JsonWriter writer, DateTimeOffset? value, JsonSerializerOptions options)
    {
        // always write dates as strings per guidance from
        // https://docs.microsoft.com/en-us/azure/cosmos-db/sql/working-with-dates
        if (value.HasValue)
        {
            writer.WriteStringValue(value.Value.ToString(Constants.CosmosDateFormat));
        }
        else
        {
            writer.WriteNullValue();
        }
    }
}