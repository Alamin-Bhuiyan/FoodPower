using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FoodPower.BuildingBlocks.Json;

/// <summary>
/// Serializes every outgoing DateTime as UTC ISO 8601 with a trailing 'Z'.
/// DateTimes read from SQL Server come back with Kind=Unspecified (but are stored
/// as UTC), which System.Text.Json would otherwise emit without an offset, causing
/// browsers to parse them as local time. This guarantees the client always receives
/// an absolute UTC instant. Also applies to DateTime? (STJ uses this for the underlying type).
/// </summary>
public class UtcDateTimeConverter : JsonConverter<DateTime>
{
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => reader.GetDateTime();

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        var utc = value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
        };

        writer.WriteStringValue(utc.ToString("yyyy-MM-ddTHH:mm:ss.fffZ", CultureInfo.InvariantCulture));
    }
}
