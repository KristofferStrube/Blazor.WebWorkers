namespace KristofferStrube.Blazor.WebWorkers;

/// <summary>
/// The options that can be used when constructing a <see cref="Worker"/>.
/// </summary>
/// <remarks><see href="https://html.spec.whatwg.org/multipage/workers.html#workeroptions">See the API definition here</see>.</remarks>
public class WorkerOptions
{
    /// <summary>
    /// The type of worker to create.
    /// </summary>
    public WorkerType Type { get; set; }
}
