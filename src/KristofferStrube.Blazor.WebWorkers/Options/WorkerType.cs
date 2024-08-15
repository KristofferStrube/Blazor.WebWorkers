using System.Text.Json.Serialization;

namespace KristofferStrube.Blazor.WebWorkers;

/// <summary>
/// The type of worker.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<WorkerType>))]
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
