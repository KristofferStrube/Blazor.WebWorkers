[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](/LICENSE)
[![GitHub issues](https://img.shields.io/github/issues/KristofferStrube/Blazor.WebWorkers)](https://github.com/KristofferStrube/Blazor.WebWorkers/issues)
[![GitHub forks](https://img.shields.io/github/forks/KristofferStrube/Blazor.WebWorkers)](https://github.com/KristofferStrube/Blazor.WebWorkers/network/members)
[![GitHub stars](https://img.shields.io/github/stars/KristofferStrube/Blazor.WebWorkers)](https://github.com/KristofferStrube/Blazor.WebWorkers/stargazers)
<!--[![NuGet Downloads (official NuGet)](https://img.shields.io/nuget/dt/KristofferStrube.Blazor.WebWorkers?label=NuGet%20Downloads)](https://www.nuget.org/packages/KristofferStrube.Blazor.WebWorkers/)-->

# Blazor.WebWorkers
A Blazor wrapper for the [Web Workers part of the HTML API.](https://html.spec.whatwg.org/multipage/workers.html)
The API defines ways to run scripts in the background independently of any the primary thread. This allows for long-running scripts that are not interrupted by scripts that respond to clicks or other user interactions, and allows long tasks to be executed without yielding to keep the page responsive. This project implements a wrapper around the API for Blazor so that we can easily and safely create workers.

**This wrapper is still just an experiment.**

# Demo
The sample project can be demoed at https://kristofferstrube.github.io/Blazor.WebWorkers/

On each page, you can find the corresponding code for the example in the top right corner.

# Approach
Many others like [Tewr/BlazorWorker](https://github.com/Tewr/BlazorWorker) and [LostBeard/SpawnDev.BlazorJS](https://github.com/LostBeard/SpawnDev.BlazorJS) have made libraries like this before. This project differs a bit from the other projects by utilizing [the wasm-experimental workload](https://devblogs.microsoft.com/dotnet/use-net-7-from-any-javascript-app-in-net-7/). This simplifies the code needed for this to work a lot. The catch to this is that you will need to have the code for your workers in another project. For me this is not only a negative as it also makes it very clear that they do not share memory and that they run in separate contexts, similar to how the *Blazor WASM* project is separate in a *Blazor WebApp*.

So to get started you really only need to *create a new console project* and then make a few adjustments to the `.csproj`. In the end it should look something like this:
```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <OutputType>Exe</OutputType>
    <RuntimeIdentifier>browser-wasm</RuntimeIdentifier>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <AllowUnsafeBlocks>true</AllowUnsafeBlocks>
  </PropertyGroup>

  <ItemGroup>
    <ProjectReference Include="<path-to-blazor-webworkers-until-i-release-a-nuget-package>\KristofferStrube.Blazor.WebWorkers.csproj" />
  </ItemGroup>

</Project>
```
And then you can do whatever you want in the `Program.cs` file, but I've added some helpers that make it easier to communicate with the main window and create objects.

Here I have an example of the code needed for a simple pong worker that broadcasts when it is ready to listen for a ping, responds with a pong when it receives that, and then shuts down.

```csharp
using KristofferStrube.Blazor.WebWorkers;

if (!OperatingSystem.IsBrowser())
    throw new PlatformNotSupportedException("Can only be run in the browser!");

Console.WriteLine("Hey this is running on another thread!");

bool keepRunning = true;

if (args.Length >= 1)
{
    Console.WriteLine($"The worker was initialized with arguments: [{string.Join(", ", args)}]");
}

// This is a helper for listening on messages.
Imports.RegisterOnMessage(e =>
{
    // If we receive a "ping", respond with "pong".
    if (e.GetTypeOfProperty("data") == "string" && e.GetPropertyAsString("data") == "ping")
    {
        // Helper for posting a message.
        Console.WriteLine("Received ping; Sending pong!");
        Imports.PostMessage("pong");
        keepRunning = false;
    }
});

Console.WriteLine("We are now listening for messages.");
Imports.PostMessage("ready");

// We run forever to keep it alive.
while (keepRunning)
    await Task.Delay(100);

Console.WriteLine("Worker done, so stopping!");
```

And with very little extra setup we can start this and post a message to it once it is ready. For this we use a very simple abstraction over the worker that enable us to specify an assembly name and some arguments for the worker.

```csharp
SlimWorker slimWorker = await SlimWorker.CreateAsync(
    jSRuntime: JSRuntime,
    assembly: typeof(AssemblyPongWorker).Assembly.GetName().Name!,
    ["Argument1", "Argument2"]
);

EventListener<MessageEvent> eventListener = default!;
eventListener = await EventListener<MessageEvent>.CreateAsync(JSRuntime, async e =>
{
    object? data = await e.Data.GetValueAsync();
    switch (data)
    {
        case "ready":
            Log("We are sending a ping!");
            await slimWorker.PostMessageAsync("ping");
            break;
        case "pong":
            Log("We received a pong!");
            await slimWorker.RemoveOnMessageEventListenerAsync(eventListener);
            await eventListener.DisposeAsync();
            await slimWorker.DisposeAsync();
            break;
    }
});
await slimWorker.AddOnMessageEventListenerAsync(eventListener);
```
This looks like so:

![ping pong demo](./docs/ping-pong.gif?raw=true)


# Related repositories
The library uses the following other packages to support its features:
- https://github.com/KristofferStrube/Blazor.WebIDL (To make error handling JSInterop)
- https://github.com/KristofferStrube/Blazor.DOM (To implement *EventTarget*'s in the package like `Worker`)
- https://github.com/KristofferStrube/Blazor.Window (To use the `MessageEvent` type)

# Related articles
This repository was built with inspiration and help from the following series of articles:

- [Typed exceptions for JSInterop in Blazor](https://kristoffer-strube.dk/post/typed-exceptions-for-jsinterop-in-blazor/)
- [Blazor WASM 404 error and fix for GitHub Pages](https://blog.elmah.io/blazor-wasm-404-error-and-fix-for-github-pages/)
- [How to fix Blazor WASM base path problems](https://blog.elmah.io/how-to-fix-blazor-wasm-base-path-problems/)
