namespace SystemTextJsonForCosmosDb.Serialization;

using System.Globalization;

internal class CosmosDateTimeOffsetJsonConverter : JsonConverter<DateTimeOffset>
{
    public override DateTimeOffset Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        JsonTokenType tokenType = reader.RequireTokenType(JsonTokenType.String, JsonTokenType.Number);

        if (tokenType == JsonTokenType.String)
        {
            string str = reader.GetString()!; // cannot be null here, since tokenType == String
            return DateTimeOffset.Parse(str, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);
        }
        else
        {
            // JsonTokenType.Number:
            // must be in UTC if stored as ticks
            long ticks = reader.GetInt64();
            return new DateTimeOffset(ticks, TimeSpan.Zero);
        }
    }

    public override void Write(Utf8JsonWriter writer, DateTimeOffset value, JsonSerializerOptions options)
    {
        // always write dates as strings per guidance from
        // https://docs.microsoft.com/en-us/azure/cosmos-db/sql/working-with-dates
        writer.WriteStringValue(value.ToUniversalTime().ToString(Constants.CosmosDateFormat));
    }
}