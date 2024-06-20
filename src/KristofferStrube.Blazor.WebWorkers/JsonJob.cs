﻿using KristofferStrube.Blazor.DOM;
using KristofferStrube.Blazor.Window;
using System.Collections.Concurrent;
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

    /// <summary>
    /// How an input is transfered to the <see cref="JobWorker{TInput, TOutput, TJob}"/> for the <see cref="JsonJob{TInput, TOutput}"/>.
    /// </summary>
    public static async Task<TOutput> ExecuteAsync<TJob>(TInput input, Worker worker, ConcurrentDictionary<string, TaskCompletionSource<TOutput>> pendingTasks) where TJob : IJob<TInput, TOutput>
    {
        string requestIdentifier = Guid.NewGuid().ToString();
        var tcs = new TaskCompletionSource<TOutput>();
        pendingTasks[requestIdentifier] = tcs;

        EventListener<MessageEvent> eventListener = default!;
        eventListener = await EventListener<MessageEvent>.CreateAsync(worker.JSRuntime, async e =>
        {
            await worker.RemoveOnMessageEventListenerAsync(eventListener);
            await eventListener.DisposeAsync();

            JobResponse response = await e.GetDataAsync<JobResponse>();
            if (pendingTasks.Remove(response.RequestIdentifier, out TaskCompletionSource<TOutput>? successTaskCompletionSource))
            {
                successTaskCompletionSource.SetResult(JsonSerializer.Deserialize<TOutput>(response.OutputSerialized)!);
            }
        });

        await worker.AddOnMessageEventListenerAsync(eventListener);

        await worker.PostMessageAsync(new JobArguments()
        {
            Namespace = typeof(TJob).Assembly.GetName().Name!,
            Type = typeof(TJob).Name,
            RequestIdentifier = requestIdentifier,
            InputSerialized = JsonSerializer.Serialize(input)
        });

        return await tcs.Task;
    }

    /// <summary>
    /// This method is called from the Worker project.
    /// </summary>
    /// <remarks>
    /// Throws an argument exception if the first arg can't be deserialized as the input type.
    /// </remarks>
    /// <param name="args">Parse the args from the program for this parameter.</param>
    /// <exception cref="ArgumentException"></exception>
    [SupportedOSPlatform("browser")]
    public void Execute(string[] args)
    {
        JobArguments arguments;

        try
        {
            arguments = JsonSerializer.Deserialize<JobArguments>(args[0])!;
        }
        catch
        {
            throw new ArgumentException("First argument could not be serialized as job argument.");
        }

        if (!arguments.Type.Equals(GetType().Name))
        {
            return;
        }

        TInput input = JsonSerializer.Deserialize<TInput>(arguments.InputSerialized)!;
        TOutput output = Work(input);

        PostOutput(JsonSerializer.Serialize(output), arguments.RequestIdentifier);
    }

    /// <summary>
    /// Posts the output.
    /// </summary>
    /// <param name="output">The output serialized.</param>
    /// <param name="requestIdentifier">The <see cref="JobResponse.RequestIdentifier"/>.</param>
    [SupportedOSPlatform("browser")]
    private void PostOutput(string output, string requestIdentifier)
    {
        System.Runtime.InteropServices.JavaScript.JSObject outputObject = Imports.CreateObject();

        outputObject.SetProperty("type", GetType().Name);
        outputObject.SetProperty("requestIdentifier", requestIdentifier);
        outputObject.SetProperty("outputSerialized", output);

        Imports.PostMessage(outputObject);
    }

    /// <summary>
    /// The arguments that are parsed to the job.
    /// </summary>
    public class JobArguments
    {
        /// <summary>
        /// The namespace of the job.
        /// </summary>
        [JsonPropertyName("namespace")]
        public required string Namespace { get; set; }

        /// <summary>
        /// The type of the job. Used to ensure that the correct job is run.
        /// </summary>
        [JsonPropertyName("type")]
        public required string Type { get; set; }

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
        /// The type of the job that ran.
        /// </summary>
        [JsonPropertyName("type")]
        public required string Type { get; set; }

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
