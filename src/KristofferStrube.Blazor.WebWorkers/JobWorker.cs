using KristofferStrube.Blazor.WebIDL;
using KristofferStrube.Blazor.WebWorkers.Extensions;
using Microsoft.JSInterop;
using System.Collections.Concurrent;

namespace KristofferStrube.Blazor.WebWorkers;

/// <summary>
/// A <see cref="Worker"/> that can execute some specific <typeparamref name="TJob"/> on a worker thread.
/// </summary>
/// <typeparam name="TInput"></typeparam>
/// <typeparam name="TOutput"></typeparam>
/// <typeparam name="TJob"></typeparam>
public class JobWorker<TInput, TOutput, TJob> : Worker where TJob : IJob<TInput, TOutput>
{
    private readonly ConcurrentDictionary<string, TaskCompletionSource<TOutput>> pendingTasks = new();

    /// <summary>
    /// Creates a <see cref="JobWorker{TInput, TOutput, TJob}"/> that can execute some specific <typeparamref name="TJob"/> on a worker thread.
    /// </summary>
    /// <param name="jSRuntime">An <see cref="IJSRuntime"/> instance.</param>
    public static new async Task<JobWorker<TInput, TOutput, TJob>> CreateAsync(IJSRuntime jSRuntime)
    {
        await using IJSObjectReference helper = await jSRuntime.GetHelperAsync();
        IJSObjectReference jSInstance = await helper.InvokeAsync<IJSObjectReference>("constructWorker",
            "_content/KristofferStrube.Blazor.WebWorkers/KristofferStrube.Blazor.WebWorkers.JobWorker.js",
            new WorkerOptions() { Type = WorkerType.Module });

        return new JobWorker<TInput, TOutput, TJob>(jSRuntime, jSInstance, new() { DisposesJSReference = true });
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
}
