using System.Collections.Concurrent;

namespace KristofferStrube.Blazor.WebWorkers;

public interface Job<TInput, TOutput>
{
    public abstract static Task<TOutput> ExecuteAsync<TJob>(TInput input, Worker worker, ConcurrentDictionary<string, TaskCompletionSource<TOutput>> pendingTasks) where TJob : Job<TInput, TOutput>;
}
