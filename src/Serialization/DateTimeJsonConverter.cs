namespace SystemTextJsonForCosmosDb.Serialization;

using System.Globalization;

internal class CosmosDateTimeJsonConverter : JsonConverter<DateTime>
{
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        JsonTokenType tokenType = reader.RequireTokenType(JsonTokenType.String, JsonTokenType.Number);

        if (tokenType == JsonTokenType.String)
        {
            string str = reader.GetString()!; // cannot be null here, since tokenType == String
            return DateTime.Parse(str, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);
        }
        else
        {
            // JsonTokenType.Number:
            // must be in UTC if stored as ticks
            long ticks = reader.GetInt64();
            return new DateTime(ticks, DateTimeKind.Utc);
        }
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        // always write dates as strings per guidance from
        // https://docs.microsoft.com/en-us/azure/cosmos-db/sql/working-with-dates
        writer.WriteStringValue(value.ToString(Constants.CosmosDateFormat));
    }
}