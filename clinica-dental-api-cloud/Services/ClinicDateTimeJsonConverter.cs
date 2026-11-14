using System.Text.Json;
using System.Text.Json.Serialization;

namespace clinica_dental_api_cloud.Services;

/// <summary>
/// Every DateTime in the responses goes out in Costa Rica time with its offset ("2026-11-20T09:00:00-06:00")
/// instead of UTC ("2026-11-20T15:00:00Z"). Both are the same instant, but this way the hour can be read directly.
/// </summary>
/// <remarks>
/// Reading keeps the default behavior; the services pass the value through <see cref="ClinicTime.ToUtc"/>,
/// so a date without zone is taken as clinic time.
/// </remarks>
public class ClinicDateTimeJsonConverter : JsonConverter<DateTime>
{
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        reader.GetDateTime();

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options) =>
        writer.WriteStringValue(ClinicTime.ToClinicOffset(value));
}
