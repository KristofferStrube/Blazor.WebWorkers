using System.Runtime.InteropServices.JavaScript;
using System.Runtime.Versioning;

namespace KristofferStrube.Blazor.WebWorkers;

[SupportedOSPlatform("browser")]
public partial class Imports
{
    [JSImport("createObject", "boot.js")]
    public static partial JSObject CreateObject();

    [JSImport("disposeObject", "boot.js")]
    public static partial void DisposeObject(JSObject obj);

    [JSImport("postMessage", "boot.js")]
    public static partial string PostMessage([JSMarshalAs<JSType.Object>] JSObject message);

    [JSImport("postMessage", "boot.js")]
    public static partial string PostMessage([JSMarshalAs<JSType.String>] string message);

    [JSImport("registerOnMessage", "boot.js")]
    public static partial void RegisterOnMessage([JSMarshalAs<JSType.Function<JSType.Object>>] Action<JSObject> handler);
}
