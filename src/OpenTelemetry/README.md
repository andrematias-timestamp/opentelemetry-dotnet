# OpenTelemetry .NET SDK

[![NuGet](https://img.shields.io/nuget/v/OpenTelemetry.svg)](https://www.nuget.org/packages/OpenTelemetry)
[![NuGet](https://img.shields.io/nuget/dt/OpenTelemetry.svg)](https://www.nuget.org/packages/OpenTelemetry)

* [Installation](#installation)
* [Introduction](#introduction)
* [Getting started](#getting-started)
* [Customization](#customization)
  * [Customize Sampler](#customize-sampler)
  * [Customize Resource](#customize-resource)
  * [Filtering and enriching activities using
    Processor](#filtering-and-enriching-activities-using-processor)
  * [OpenTelemetry Instrumentation](#opentelemetry-instrumentation)
* [Advanced topics](#advanced-topics)
  * [Build your own Exporter](#build-your-own-exporter)
  * [Build your own Sampler](#build-your-own-sampler)
* [References](#references)

## Installation

```shell
dotnet add package OpenTelemetry
```

## Introduction

OpenTelemetry SDK is a reference implementation of the OpenTelemetry API. It
implements the Tracing API, the Metrics API, and the Context API. OpenTelemetry
SDK deals with concerns such as sampling, processing pipeline, exporting
telemetry to a particular backend etc. The default implementation consists of
the following.

* Set of [Built-in
  samplers](https://github.com/open-telemetry/opentelemetry-specification/blob/master/specification/trace/sdk.md#built-in-samplers)
* Set of [Built-in
  processors](https://github.com/open-telemetry/opentelemetry-specification/blob/master/specification/trace/sdk.md#built-in-span-processors).
  * SimpleProcessor which sends Activities to the exporter without any
    batching.
  * BatchingProcessor which batches and sends Activities to the exporter.
* Extensibility options for users to customize SDK.

## Getting started

Please follow the tutorial and [get started in 5
minutes](../../docs/getting-started.md).

## Customization

### Customize Sampler

[Samplers](https://github.com/open-telemetry/opentelemetry-specification/blob/master/specification/trace/sdk.md#sampler)
are used to control the noise and overhead introduced by OpenTelemetry by
reducing the number of samples of traces collected and sent to the backend. If
no sampler is explicitly specified, the default is to use
[AlwaysOnSampler](https://github.com/open-telemetry/opentelemetry-specification/blob/master/specification/trace/sdk.md#alwayson).
The following sample shows how to change it to
[ProbabilitySampler](https://github.com/open-telemetry/opentelemetry-specification/blob/master/specification/trace/sdk.md#probability)
with sampling probability of 25%.

```csharp
using var openTelemetry = OpenTelemetrySdk.EnableOpenTelemetry(builder => builder
                .AddActivitySource("companyname.product.library")
                .SetSampler(new ProbabilitySampler(.25))
                .UseConsoleExporter());
```

The above requires import of the namespace `OpenTelemetry.Trace.Samplers`.

### Customize Resource

### Filtering and enriching activities using Processor

### OpenTelemetry Instrumentation

This should link to the Instrumentation documentation.

## Advanced topics

### Build your own Exporter

#### Trace Exporter

* Exporters should subclass `ActivityExporter` and implement `ExportAsync` and
  `ShutdownAsync` methods.
* Depending on user's choice and load on the application `ExportAsync` may get
  called concurrently with zero or more activities.
* Exporters should expect to receive only sampled-in ended activities.
* Exporters must not throw.
* Exporters should not modify activities they receive (the same activity may be
  exported again by different exporter).

It's a good practice to make exporter `IDisposable` and shut it down in
IDispose unless it was shut down explicitly. This helps when exporters are
registered with dependency injection framework and their lifetime is tight to
the app lifetime.

```csharp
class MyExporter : ActivityExporter, IDisposable
{
    public override Task<ExportResult> ExportAsync(
        IEnumerable<Activity> batch, CancellationToken cancellationToken)
    {
        foreach (var activity in batch)
        {
            Console.WriteLine(
                $"[{activity.StartTimeUtc:o}] " +
                $"{activity.DisplayName} " +
                $"{activity.Context.TraceId.ToHexString()} " +
                $"{activity.Context.SpanId.ToHexString()}"
            );
        }

        return Task.FromResult(ExportResult.Success);
    }

    public override Task ShutdownAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        // ...
    }

    protected virtual void Dispose(bool disposing)
    {
        // ...
    }
}
```

* Users may configure the exporter similarly to other exporters.
* You should also provide additional methods to simplify configuration
  similarly to `UseZipkinExporter` extension method.

```csharp
OpenTelemetrySdk.EnableOpenTelemetry(b => b
    .AddActivitySource(ActivitySourceName)
    .UseMyExporter();
```

### Build your own Sampler

You can build your own sampler by subclassing `ActivitySampler`:

```csharp
class MySampler : ActivitySampler
{
    public override string Description { get; } = "my custom sampler";

    public override SamplingResult ShouldSample(in ActivitySamplingParameters samplingParameters)
    {
        bool sampledIn;
        var parentContext = samplingParameters.ParentContext;
        if (parentContext != null && parentContext.IsValid())
        {
            sampledIn = (
                parentContext.TraceFlags & ActivityTraceFlags.Recorded
            ) != 0;
        }
        else
        {
            sampledIn = Stopwatch.GetTimestamp() % 2 == 0;
        }

        return new Decision(sampledIn);
    }
}
```

## References

* [OpenTelemetry Project](https://opentelemetry.io/)
