using KristofferStrube.Blazor.DOM;
using KristofferStrube.Blazor.Window;
using System.Collections.Concurrent;
using System.Runtime.InteropServices.JavaScript;
using System.Runtime.Versioning;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace KristofferStrube.Blazor.WebWorkers;

/// <summary>
/// The base definition of a job that uses JSON as its serialization mechanism for the input and output.
/// </summary>
/// <typeparam name="TInput">The input type for the job.</typeparam>
/// <typeparam name="TOutput">The output type for the job.</typeparam>
public abstract class BaseJsonJob<TInput, TOutput> : IJob<TInput, TOutput>
{
    private static readonly bool inputIsString = typeof(TInput) == typeof(string);
    private static readonly bool outputIsString = typeof(TOutput) == typeof(string);

    /// <inheritdoc/>
    public static async Task<EventListener<MessageEvent>> InitializeAsync(Worker worker, ConcurrentDictionary<string, TaskCompletionSource<TOutput>> pendingTasks)
    {
        EventListener<MessageEvent> eventListener = default!;
        eventListener = await EventListener<MessageEvent>.CreateAsync(worker.JSRuntime, async e =>
        {
            JobResponse response = await e.GetDataAsync<JobResponse>();
            if (pendingTasks.Remove(response.RequestIdentifier, out TaskCompletionSource<TOutput>? successTaskCompletionSource))
            {
                if (outputIsString && response.OutputSerialized is TOutput stringOutput)
                {
                    successTaskCompletionSource.SetResult(stringOutput);
                }
                else
                {
                    successTaskCompletionSource.SetResult(JsonSerializer.Deserialize<TOutput>(response.OutputSerialized)!);
                }
            }
        });

        await worker.AddOnMessageEventListenerAsync(eventListener);

        return eventListener;
    }

    /// <inheritdoc/>
    public static async Task<TOutput> ExecuteAsync<TJob>(TInput input, Worker worker, ConcurrentDictionary<string, TaskCompletionSource<TOutput>> pendingTasks) where TJob : IJob<TInput, TOutput>
    {
        string requestIdentifier = Guid.NewGuid().ToString();
        var tcs = new TaskCompletionSource<TOutput>();
        pendingTasks[requestIdentifier] = tcs;

        string inputSerialized = input is string stringInput
            ? stringInput
            : JsonSerializer.Serialize(input);

        await worker.PostMessageAsync(new JobArguments()
        {
            RequestIdentifier = requestIdentifier,
            InputSerialized = inputSerialized
        });

        return await tcs.Task;
    }

    /// <summary>
    /// Gets the input from a message.
    /// </summary>
    /// <param name="message">The message sent to the worker.</param>
    /// <returns>Returns the input.</returns>
    [SupportedOSPlatform("browser")]
    protected (TInput input, string requestIdentifier) GetInputAndRequestIdentifier(JSObject message)
    {
        JSObject data = message.GetPropertyAsJSObject("data")!;
        string? inputSerialized = data.GetPropertyAsString("inputSerialized");
        string requestIdentifier = data.GetPropertyAsString("requestIdentifier")!;

        TInput input = inputIsString && inputSerialized is TInput stringInput
            ? stringInput
            : JsonSerializer.Deserialize<TInput>(inputSerialized!)!;

        return (input, requestIdentifier);
    }

    /// <summary>
    /// Posts the output.
    /// </summary>
    /// <param name="output">The output serialized.</param>
    /// <param name="requestIdentifier">The <see cref="JobResponse.RequestIdentifier"/>.</param>
    [SupportedOSPlatform("browser")]
    protected void PostOutput(TOutput output, string requestIdentifier)
    {
        JSObject outputObject = Imports.CreateObject();

        outputObject.SetProperty("requestIdentifier", requestIdentifier);
        if (output is string stringOutput)
        {
            outputObject.SetProperty("outputSerialized", stringOutput);
        }
        else
        {
            outputObject.SetProperty("outputSerialized", JsonSerializer.Serialize(output));
        }

        Imports.PostMessage(outputObject);
    }

    /// <summary>
    /// The arguments that are parsed to the job.
    /// </summary>
    public class JobArguments
    {
        /// <summary>
        /// The unique identifier for the specific request. Used to identify which task has finished when the job responds.
        /// </summary>
        [JsonPropertyName("requestIdentifier")]
        public required string RequestIdentifier { get; set; }

        /// <summary>
        /// The input serialized to JSON.
        /// </summary>
        [JsonPropertyName("inputSerialized")]
        public required string InputSerialized { get; set; }
    }

    /// <summary>
    /// The format of the respond.
    /// </summary>
    public class JobResponse
    {
        /// <summary>
        /// The same unique identifier sent in <see cref="JobArguments.RequestIdentifier"/>.
        /// </summary>
        [JsonPropertyName("requestIdentifier")]
        public required string RequestIdentifier { get; set; }

        /// <summary>
        /// The output serialized to JSON.
        /// </summary>
        [JsonPropertyName("outputSerialized")]
        public required string OutputSerialized { get; set; }
    }
}
