using KristofferStrube.Blazor.DOM;
using KristofferStrube.Blazor.WebIDL;
using KristofferStrube.Blazor.WebWorkers.Extensions;
using KristofferStrube.Blazor.Window;
using Microsoft.JSInterop;
using System.Collections.Concurrent;

namespace KristofferStrube.Blazor.WebWorkers;

/// <summary>
/// A <see cref="Worker"/> that can execute some specific <typeparamref name="TJob"/> on a worker thread.
/// </summary>
/// <typeparam name="TInput"></typeparam>
/// <typeparam name="TOutput"></typeparam>
/// <typeparam name="TJob"></typeparam>
public class JobWorker<TInput, TOutput, TJob> : Worker, IAsyncDisposable where TJob : IJob<TInput, TOutput>
{
    private readonly ConcurrentDictionary<string, TaskCompletionSource<TOutput>> pendingTasks = new();
    private EventListener<MessageEvent>? messageListener;

    /// <summary>
    /// Creates a <see cref="JobWorker{TInput, TOutput, TJob}"/> that can execute some specific <typeparamref name="TJob"/> on a worker thread.
    /// </summary>
    /// <param name="jSRuntime">An <see cref="IJSRuntime"/> instance.</param>
    public static new async Task<JobWorker<TInput, TOutput, TJob>> CreateAsync(IJSRuntime jSRuntime)
    {
        string scriptUrl = "_content/KristofferStrube.Blazor.WebWorkers/KristofferStrube.Blazor.WebWorkers.JobWorker.js"
            + $"?assembly={typeof(TJob).Assembly.GetName().Name}";

        await using IJSObjectReference helper = await jSRuntime.GetHelperAsync();
        IJSObjectReference jSInstance = await helper.InvokeAsync<IJSObjectReference>("constructWorker",
            scriptUrl, new WorkerOptions() { Type = WorkerType.Module });

        JobWorker<TInput, TOutput, TJob> worker = new(jSRuntime, jSInstance, new() { DisposesJSReference = true });

        var tcs = new TaskCompletionSource<JobWorker<TInput, TOutput, TJob>>();

        EventListener<MessageEvent> readyListener = default!;
        readyListener = await EventListener<MessageEvent>.CreateAsync(jSRuntime, async e =>
        {
            await worker.RemoveOnMessageEventListenerAsync(readyListener);
            await readyListener.DisposeAsync();

            worker.messageListener = await TJob.InitializeAsync(worker, worker.pendingTasks);
            tcs.SetResult(worker);
        });
        await worker.AddOnMessageEventListenerAsync(readyListener);

        return await tcs.Task;
    }

    /// <inheritdoc cref="Worker(IJSRuntime, IJSObjectReference, CreationOptions)"/>
    protected JobWorker(IJSRuntime jSRuntime, IJSObjectReference jSReference, CreationOptions options) : base(jSRuntime, jSReference, options)
    {
    }

    /// <summary>
    /// Executes the job on the worker.
    /// </summary>
    /// <param name="input">The input to the job.</param>
    /// <returns>The output of the job.</returns>
    public async Task<TOutput> ExecuteAsync(TInput input)
    {
        return await TJob.ExecuteAsync<TJob>(input, this, pendingTasks);
    }

    /// <summary>
    /// Diposes listener for events and the worker itself.
    /// </summary>
    public new async ValueTask DisposeAsync()
    {
        if (messageListener is not null)
        {
            await RemoveOnMessageEventListenerAsync(messageListener);
            await messageListener.DisposeAsync();
        }
        await base.DisposeAsync();
    }
}
