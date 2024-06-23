﻿using KristofferStrube.Blazor.WebWorkers.StringSumWorker;

if (!OperatingSystem.IsBrowser())
    throw new PlatformNotSupportedException("Can only be run in the browser!");

await new StringSumJob().Start();