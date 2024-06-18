﻿using System.Runtime.Versioning;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace KristofferStrube.Blazor.WebWorkers;

public abstract class JSONJob<TInput, TOutput>
{
    /// <summary>
    /// The actual work being done by the job. This will be run when the job is executed.
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public abstract TOutput Work(TInput input);

    /// <summary>
    /// To test the job, you can execute the job from Blazor. This will not use a worker at all.
    /// This method can be used to ensure that your inputs and outputs can be serialized/deserialized correctly.
    /// </summary>
    public TOutput ExecuteWithoutUsingWorker(TInput input)
    {
        TInput inputSerializedAndDeserialized = JsonSerializer.Deserialize<TInput>(JsonSerializer.Serialize(input))!;

        TOutput? output = Work(inputSerializedAndDeserialized);

        TOutput outputSerializedAndDeserialized = JsonSerializer.Deserialize<TOutput>(JsonSerializer.Serialize(output))!;

        return outputSerializedAndDeserialized;
    }

    /// <summary>
    /// This method is called from the Worker project.
    /// </summary>
    /// <remarks>
    /// Throws an argument exception if the first arg can't be deserialized as the input type.
    /// </remarks>
    /// <param name="args">Parse the args from the program for this parameter.</param>
    /// <exception cref="ArgumentException"></exception>
    [SupportedOSPlatform("browser")]
    public void Execute(string[] args)
    {
        JobArguments arguments;

        try
        {
            arguments = JsonSerializer.Deserialize<JobArguments>(args[0])!;
        }
        catch
        {
            throw new ArgumentException("First argument could not be serialized as job argument.");
        }

        if (!arguments.Type.Equals(GetType().Name))
        {
            return;
        }

        TInput input = JsonSerializer.Deserialize<TInput>(arguments.InputSerialized)!;
        TOutput output = Work(input);

        PostOutput(JsonSerializer.Serialize(output), arguments.RequestIdentifier);
    }

    /// <summary>
    /// Posts the output.
    /// </summary>
    /// <param name="output">The output serialized.</param>
    [SupportedOSPlatform("browser")]
    private void PostOutput(string output, string requestIdentifier)
    {
        System.Runtime.InteropServices.JavaScript.JSObject outputObject = Imports.CreateObject();

        outputObject.SetProperty("type", GetType().Name);
        outputObject.SetProperty("requestIdentifier", requestIdentifier);
        outputObject.SetProperty("outputSerialized", output);

        Imports.PostMessage(outputObject);
    }

    public class JobArguments
    {
        [JsonPropertyName("namespace")]
        public required string Namespace { get; set; }

        [JsonPropertyName("type")]
        public required string Type { get; set; }

        [JsonPropertyName("requestIdentifier")]
        public required string RequestIdentifier { get; set; }

        [JsonPropertyName("inputSerialized")]
        public required string InputSerialized { get; set; }
    }

    public class JobResponse
    {
        [JsonPropertyName("type")]
        public required string Type { get; set; }

        [JsonPropertyName("requestIdentifier")]
        public required string RequestIdentifier { get; set; }

        [JsonPropertyName("outputSerialized")]
        public required string OutputSerialized { get; set; }
    }
}