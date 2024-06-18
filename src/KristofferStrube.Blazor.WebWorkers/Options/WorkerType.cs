using KristofferStrube.Blazor.WebWorkers.Converters;
using System.Text.Json.Serialization;

namespace KristofferStrube.Blazor.WebWorkers;

/// <summary>
/// The type of worker.
/// </summary>
[JsonConverter(typeof(WorkerTypeConverter))]
public enum WorkerType
{
    /// <summary>
    /// A classic worker.
    /// </summary>
    Classic,
    /// <summary>
    /// A worker that is also a module.
    /// </summary>
    Module
}
