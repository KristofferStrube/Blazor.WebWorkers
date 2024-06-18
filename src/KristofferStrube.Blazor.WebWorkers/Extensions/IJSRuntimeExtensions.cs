﻿using Microsoft.JSInterop;

namespace KristofferStrube.Blazor.WebWorkers.Extensions;

internal static class IJSRuntimeExtensions
{
    internal static async Task<IJSObjectReference> GetHelperAsync(this IJSRuntime jSRuntime)
    {
        return await jSRuntime.InvokeAsync<IJSObjectReference>(
            "import", "./_content/KristofferStrube.Blazor.WebWorkers/KristofferStrube.Blazor.WebWorkers.js");
    }

    internal static async Task<IJSInProcessObjectReference> GetInProcessHelperAsync(this IJSRuntime jSRuntime)
    {
        return await jSRuntime.InvokeAsync<IJSInProcessObjectReference>(
            "import", "./_content/KristofferStrube.Blazor.WebWorkers/KristofferStrube.Blazor.WebWorkers.js");
    }
}
