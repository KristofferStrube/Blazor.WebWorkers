using System.Text.Json;
using System.Text.Json.Serialization;

namespace KristofferStrube.Blazor.WebWorkers.Converters;

internal class WorkerTypeConverter : JsonConverter<WorkerType>
{
    public override WorkerType Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        throw new NotSupportedException($"Reading a {nameof(WorkerType)} is not supported.");
    }

    public override void Write(Utf8JsonWriter writer, WorkerType value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value switch
        {
            WorkerType.Classic => "classic",
            WorkerType.Module => "module",
            _ => throw new ArgumentException($"Value '{value}' was not a valid {nameof(WorkerType)}.")
        });
    }
}
