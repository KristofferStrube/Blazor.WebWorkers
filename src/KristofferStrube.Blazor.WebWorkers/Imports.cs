using System.Runtime.InteropServices.JavaScript;
using System.Runtime.Versioning;

namespace KristofferStrube.Blazor.WebWorkers;

/// <summary>
/// Methods that help us making JS Interop.
/// </summary>
[SupportedOSPlatform("browser")]
public partial class Imports
{
    /// <summary>
    /// Creates a rented object that should be released again at some point by being parsed to <see cref="DisposeObject(JSObject)"/>
    /// </summary>
    /// <returns></returns>
    [JSImport("createObject", "boot.js")]
    public static partial JSObject CreateObject();
    
    /// <summary>
    /// Disposes an object created from <see cref="CreateObject"/>.
    /// </summary>
    /// <param name="obj"></param>
    [JSImport("disposeObject", "boot.js")]
    public static partial void DisposeObject(JSObject obj);

    /// <summary>
    /// Posts a <see cref="JSObject"/> on the worker.
    /// </summary>
    /// <param name="message"></param>
    [JSImport("postMessage", "boot.js")]
    public static partial void PostMessage([JSMarshalAs<JSType.Object>] JSObject message);

    /// <summary>
    /// Posts a <see cref="string"/> on the worker.
    /// </summary>
    [JSImport("postMessage", "boot.js")]
    public static partial void PostMessage([JSMarshalAs<JSType.String>] string message);

    /// <summary>
    /// Registers a listener for when a message is posted to the worker.
    /// </summary>
    /// <param name="handler"></param>
    [JSImport("registerOnMessage", "boot.js")]
    public static partial void RegisterOnMessage([JSMarshalAs<JSType.Function<JSType.Object>>] Action<JSObject> handler);
}
