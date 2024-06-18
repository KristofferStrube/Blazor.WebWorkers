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
