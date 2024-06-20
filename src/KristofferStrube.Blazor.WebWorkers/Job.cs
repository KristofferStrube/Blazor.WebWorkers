using System.Collections.Concurrent;

namespace KristofferStrube.Blazor.WebWorkers;

/// <summary>
/// A job that takes some input and returns some output.
/// </summary>
public interface Job<TInput, TOutput>
{
    /// <summary>
    /// How the job will send execute the job on the worker.
    /// </summary>
    /// <typeparam name="TJob">The type of the job.</typeparam>
    /// <param name="input">The input that the job should be executed with.</param>
    /// <param name="worker">The worker that the job should be runned on.</param>
    /// <param name="pendingTasks">The dictionary that manages which executions finishes.</param>
    /// <returns>The output of the job once it responds.</returns>
    public abstract static Task<TOutput> ExecuteAsync<TJob>(TInput input, Worker worker, ConcurrentDictionary<string, TaskCompletionSource<TOutput>> pendingTasks) where TJob : Job<TInput, TOutput>;
}
