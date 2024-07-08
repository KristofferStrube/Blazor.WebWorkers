using KristofferStrube.Blazor.DOM;
using KristofferStrube.Blazor.Window;
using System.Collections.Concurrent;
using System.Runtime.InteropServices.JavaScript;
using System.Runtime.Versioning;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace KristofferStrube.Blazor.WebWorkers;

/// <summary>
/// A job that uses JSON as its serialization mechanism for the input and output.
/// </summary>
/// <typeparam name="TInput"></typeparam>
/// <typeparam name="TOutput"></typeparam>
public abstract class JsonJob<TInput, TOutput> : IJob<TInput, TOutput>
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

    /// <inheritdoc/>
    public static async Task<EventListener<MessageEvent>> InitializeAsync(Worker worker, ConcurrentDictionary<string, TaskCompletionSource<TOutput>> pendingTasks)
    {
        EventListener<MessageEvent> eventListener = default!;
        eventListener = await EventListener<MessageEvent>.CreateAsync(worker.JSRuntime, async e =>
        {
            JobResponse response = await e.GetDataAsync<JobResponse>();
            if (pendingTasks.Remove(response.RequestIdentifier, out TaskCompletionSource<TOutput>? successTaskCompletionSource))
            {
                if (typeof(TOutput) == typeof(string) && response.OutputSerialized is TOutput stringOutput)
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
    /// This method is called from the Worker project to start listening for events
    /// </summary>
    [SupportedOSPlatform("browser")]
    public async Task StartAsync()
    {
        Imports.RegisterOnMessage(message =>
        {
            JSObject data = message.GetPropertyAsJSObject("data")!;
            string? inputSerialized = data.GetPropertyAsString("inputSerialized");
            string requestIdentifier = data.GetPropertyAsString("requestIdentifier")!;

            if (inputSerialized is null) return;

            TInput input = typeof(TInput) == typeof(string) && inputSerialized is TInput stringInput
                ? stringInput
                : JsonSerializer.Deserialize<TInput>(inputSerialized)!;

            TOutput output = Work(input);

            PostOutput(output, requestIdentifier);
        });

        TaskCompletionSource tcs = new();
        await tcs.Task;
    }

    /// <summary>
    /// Posts the output.
    /// </summary>
    /// <param name="output">The output serialized.</param>
    /// <param name="requestIdentifier">The <see cref="JobResponse.RequestIdentifier"/>.</param>
    [SupportedOSPlatform("browser")]
    private void PostOutput(TOutput output, string requestIdentifier)
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
