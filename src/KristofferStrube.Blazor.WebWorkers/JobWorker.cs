using KristofferStrube.Blazor.DOM;
using KristofferStrube.Blazor.WebIDL;
using KristofferStrube.Blazor.WebWorkers.Extensions;
using KristofferStrube.Blazor.Window;
using Microsoft.JSInterop;
using System.Collections.Concurrent;
using System.Text.Json;

namespace KristofferStrube.Blazor.WebWorkers;

public class JobWorker<TInput, TOutput, TJob> : Worker where TJob : JSONJob<TInput, TOutput>
{
    private readonly ConcurrentDictionary<string, TaskCompletionSource<TOutput>> pendingTasks = new();

    public static new async Task<JobWorker<TInput, TOutput, TJob>> CreateAsync(IJSRuntime jSRuntime)
    {
        await using IJSObjectReference helper = await jSRuntime.GetHelperAsync();
        IJSObjectReference jSInstance = await helper.InvokeAsync<IJSObjectReference>("constructWorker",
            "_content/KristofferStrube.Blazor.WebWorkers/KristofferStrube.Blazor.WebWorkers.JobWorker.js",
            new WorkerOptions() { Type = WorkerType.Module });

        return new JobWorker<TInput, TOutput, TJob>(jSRuntime, jSInstance, new() { DisposesJSReference = true });
    }

    protected internal JobWorker(IJSRuntime jSRuntime, IJSObjectReference jSReference, CreationOptions options) : base(jSRuntime, jSReference, options)
    {
    }

    public async Task<TOutput> ExecuteAsync(TInput input)
    {
        string requestIdentifier = Guid.NewGuid().ToString();
        var tcs = new TaskCompletionSource<TOutput>();
        pendingTasks[requestIdentifier] = tcs;

        EventListener<MessageEvent> eventListener = default!;
        eventListener = await EventListener<MessageEvent>.CreateAsync(JSRuntime, async e =>
        {
            await RemoveOnMessageEventListenerAsync(eventListener);
            await eventListener.DisposeAsync();

            JSONJob<object, object>.JobResponse response = await e.GetDataAsync<JSONJob<object, object>.JobResponse>();
            if (pendingTasks.Remove(response.RequestIdentifier, out TaskCompletionSource<TOutput> successTaskCompletionSource))
            {
                successTaskCompletionSource.SetResult(JsonSerializer.Deserialize<TOutput>(response.OutputSerialized)!);
            }
        });

        await AddOnMessageEventListenerAsync(eventListener);

        await PostMessageAsync(new JSONJob<object, object>.JobArguments()
        {
            Namespace = typeof(TJob).Namespace!,
            Type = typeof(TJob).Name,
            RequestIdentifier = requestIdentifier,
            InputSerialized = JsonSerializer.Serialize(input)
        });

        return await tcs.Task;
    }
}
