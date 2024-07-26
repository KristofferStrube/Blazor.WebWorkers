using KristofferStrube.Blazor.DOM;
using KristofferStrube.Blazor.Window;
using System.Collections.Concurrent;

namespace KristofferStrube.Blazor.WebWorkers;

/// <summary>
/// A job that takes some input and returns some output.
/// </summary>
public interface IJob<TInput, TOutput>
{
    /// <summary>
    /// Initializes the job so that it is ready to be executed.
    /// </summary>
    /// <param name="worker">The worker that the job should be runned on.</param>
    /// <param name="pendingTasks">The dictionary that manages which executions finishes.</param>
    public abstract static Task<EventListener<MessageEvent>> InitializeAsync(Worker worker, ConcurrentDictionary<string, TaskCompletionSource<TOutput>> pendingTasks);

    /// <summary>
    /// Sends the <paramref name="input"/> to the <paramref name="worker"/>.
    /// </summary>
    /// <typeparam name="TJob">The type of the job.</typeparam>
    /// <param name="input">The input that the job should be executed with.</param>
    /// <param name="worker">The worker that the job should be runned on.</param>
    /// <param name="pendingTasks">The dictionary that manages which executions finishes.</param>
    /// <returns>The output of the job once it responds.</returns>
    public abstract static Task<TOutput> ExecuteAsync<TJob>(TInput input, Worker worker, ConcurrentDictionary<string, TaskCompletionSource<TOutput>> pendingTasks) where TJob : IJob<TInput, TOutput>;
}
