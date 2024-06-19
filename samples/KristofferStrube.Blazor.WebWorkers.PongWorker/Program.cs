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