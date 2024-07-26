using System.Runtime.Versioning;
using System.Text.Json;

namespace KristofferStrube.Blazor.WebWorkers;

/// <summary>
/// A job that uses JSON as its serialization mechanism for the input and output.
/// </summary>
/// <typeparam name="TInput">The input type for the job.</typeparam>
/// <typeparam name="TOutput">The output type for the job.</typeparam>
public abstract class JsonJob<TInput, TOutput> : BaseJsonJob<TInput, TOutput>
{
    /// <summary>
    /// The actual work being done by the job. This will be run when the job is executed.
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public abstract TOutput Work(TInput input);

    /// <summary>
    /// To test the job, you can execute the job from Blazor. This will not use a worker at all.
    /// This method can be used to ensure that your inputs and outputs can be serialized/deserialized correctly.
    /// </summary>
    public TOutput ExecuteWithoutUsingWorker(TInput input)
    {
        TInput inputSerializedAndDeserialized = JsonSerializer.Deserialize<TInput>(JsonSerializer.Serialize(input))!;

        TOutput? output = Work(inputSerializedAndDeserialized);

        TOutput outputSerializedAndDeserialized = JsonSerializer.Deserialize<TOutput>(JsonSerializer.Serialize(output))!;

        return outputSerializedAndDeserialized;
    }

    /// <summary>
    /// This method is called from the Worker project to start listening for events
    /// </summary>
    [SupportedOSPlatform("browser")]
    public async Task StartAsync()
    {
        WorkerContext.RegisterOnMessage(message =>
        {
            (TInput input, string requestIdentifier) = GetInputAndRequestIdentifier(message);

            TOutput output = Work(input);

            PostOutput(output, requestIdentifier);
        });

        TaskCompletionSource tcs = new();
        await tcs.Task;
    }
}
