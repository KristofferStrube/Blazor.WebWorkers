using KristofferStrube.Blazor.WebIDL;
using KristofferStrube.Blazor.WebWorkers.Extensions;
using Microsoft.JSInterop;
using System.Text.Json;

namespace KristofferStrube.Blazor.WebWorkers;

/// <summary>
/// A <see cref="Worker"/> that executes some assembly that is referenced by the main project.
/// </summary>
public class SlimWorker : Worker
{
    /// <summary>
    /// Create a <see cref="SlimWorker"/> that executes some assembly that is referenced by the main project.
    /// </summary>
    /// <param name="jSRuntime">An <see cref="IJSRuntime"/> instance.</param>
    /// <param name="assembly">The namespace of the assembly to run. You can get this by calling <c>myType.Assembly.GetName().Name!</c> on some type from the assembly. It is important that this assembly is also referenced by the project that initializes this <see cref="SlimWorker"/>.</param>
    /// <param name="args">The args to parse to the program in the specified assembly when running it.</param>
    public static async Task<SlimWorker> CreateAsync(IJSRuntime jSRuntime, string assembly, string[]? args = null)
    {
        args ??= [];

        string scriptUrl = "_content/KristofferStrube.Blazor.WebWorkers/KristofferStrube.Blazor.WebWorkers.SlimWorker.js"
            + $"?assembly={assembly}"
            + $"&serializedArgs={JsonSerializer.Serialize(args)}";

        await using IJSObjectReference helper = await jSRuntime.GetHelperAsync();
        IJSObjectReference jSInstance = await helper.InvokeAsync<IJSObjectReference>("constructWorker", scriptUrl,
            new WorkerOptions() { Type = WorkerType.Module });

        return new SlimWorker(jSRuntime, jSInstance, new() { DisposesJSReference = true });
    }

    /// <inheritdoc cref="Worker(IJSRuntime, IJSObjectReference, CreationOptions)"/>
    protected SlimWorker(IJSRuntime jSRuntime, IJSObjectReference jSReference, CreationOptions options) : base(jSRuntime, jSReference, options)
    {
    }
}
