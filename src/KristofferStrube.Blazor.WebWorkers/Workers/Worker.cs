using KristofferStrube.Blazor.DOM;
using KristofferStrube.Blazor.WebIDL;
using KristofferStrube.Blazor.WebWorkers.Extensions;
using KristofferStrube.Blazor.Window;
using KristofferStrube.Blazor.Window.Options;
using Microsoft.JSInterop;

namespace KristofferStrube.Blazor.WebWorkers;

/// <summary>
/// A dedicated worker that can do some work.
/// </summary>
/// <remarks><see href="https://html.spec.whatwg.org/multipage/workers.html#dedicated-workers-and-the-worker-interface">See the API definition here</see>.</remarks>
public class Worker : EventTarget, IJSCreatable<Worker>
{
    /// <summary>
    /// A lazily evaluated task that gives access to helper methods from Blazor.WebWorkers.
    /// </summary>
    protected readonly Lazy<Task<IJSObjectReference>> workerHelperTask;

    /// <inheritdoc/>
    public static new async Task<Worker> CreateAsync(IJSRuntime jSRuntime, IJSObjectReference jSReference)
    {
        return await CreateAsync(jSRuntime, jSReference, new());
    }

    /// <inheritdoc/>
    public static new Task<Worker> CreateAsync(IJSRuntime jSRuntime, IJSObjectReference jSReference, CreationOptions options)
    {
        return Task.FromResult(new Worker(jSRuntime, jSReference, options));
    }

    /// <summary>
    /// Creates a <see cref="Worker"/> using the standard constructor.
    /// </summary>
    /// <param name="jSRuntime">An <see cref="IJSRuntime"/> instance.</param>
    /// <param name="scriptUrl">Will be fetched and executed in the background.</param>
    /// <param name="options">Optional options for constructing this <see cref="Worker"/>.</param>
    public static async Task<Worker> CreateAsync(IJSRuntime jSRuntime, string scriptUrl, WorkerOptions? options = null)
    {
        await using IJSObjectReference helper = await jSRuntime.GetHelperAsync();
        IJSObjectReference jSInstance = await helper.InvokeAsync<IJSObjectReference>("constructWorker", scriptUrl, options);
        return new Worker(jSRuntime, jSInstance, new() { DisposesJSReference = true });
    }

    /// <inheritdoc cref="CreateAsync(IJSRuntime, IJSObjectReference, CreationOptions)"/>
    protected Worker(IJSRuntime jSRuntime, IJSObjectReference jSReference, CreationOptions options) : base(jSRuntime, jSReference, options)
    {
        workerHelperTask = new(jSRuntime.GetHelperAsync);
    }

    /// <summary>
    /// Posts a message to the given worker.
    /// </summary>
    /// <param name="message">Messages can be structured objects, e.g. nested JSON objects and arrays, can contain JavaScript values (strings, numbers, Date objects, etc.), and can contain certain data objects that are marked as <see cref="ITransferable"/> like <see cref="ArrayBuffer"/></param>
    /// <param name="options">The options used for posting the message. Defining the target origin and what objects should be transfered.</param>
    public async Task PostMessageAsync(object message, StructuredSerializeOptions? options = null)
    {
        await JSReference.InvokeVoidAsync("postMessage", message, options);
    }

    /// <summary>
    /// Adds an <see cref="EventListener{TEvent}"/> for when the worker receives a message.
    /// </summary>
    /// <param name="callback">Callback that will be invoked when the event is dispatched.</param>
    /// <param name="options"><inheritdoc cref="EventTarget.AddEventListenerAsync{TEvent}(string, EventListener{TEvent}?, AddEventListenerOptions?)" path="/param[@name='options']"/></param>
    public async Task AddOnMessageEventListenerAsync(EventListener<MessageEvent> callback, AddEventListenerOptions? options = null)
    {
        await AddEventListenerAsync("message", callback, options);
    }

    /// <summary>
    /// Removes the event listener from the event listener list if it has been parsed to <see cref="AddOnMessageEventListenerAsync"/> previously.
    /// </summary>
    /// <param name="callback">The callback <see cref="EventListener{TEvent}"/> that you want to stop listening to events.</param>
    /// <param name="options"><inheritdoc cref="EventTarget.RemoveEventListenerAsync{TEvent}(string, EventListener{TEvent}?, EventListenerOptions?)" path="/param[@name='options']"/></param>
    public async Task RemoveOnMessageEventListenerAsync(EventListener<MessageEvent> callback, EventListenerOptions? options = null)
    {
        await RemoveEventListenerAsync("message", callback, options);
    }
}
