# Tempus.Core

Cross-platform, DST-safe datetime utilities for .NET — zero external surface dependencies.

Provides `ITempusClock`, `ITimezoneResolver`, `IDstResolver`, the `DateRange` value type, and extension methods on `DateTimeOffset` / `DateTime` / `DateOnly`.

## Installation

```bash
dotnet add package Tempus.Core
```

## Quick start

```csharp
builder.Services.AddTempus();
```

```csharp
public class MyService(ITempusClock clock, IDstResolver dst)
{
    public DateTimeOffset SafeConvert(DateTime local, string iana)
        => dst.ToUtc(local, iana).Value;

    public DateOnly TodayIn(string iana) => clock.TodayIn(iana);
}
```

## Key types

- `ITempusClock` — testable replacement for `DateTime.UtcNow`
- `ITimezoneResolver` — cross-platform IANA ↔ Windows ID translation
- `IDstResolver` — DST-ambiguity resolution with pluggable strategies
- `DateRange` — inclusive calendar-date range with overlap, intersection, and enumeration
- `ConversionResult` — UTC conversion outcome with ambiguity/invalidity metadata
- `DstTransition` — describes a single spring-forward or fall-back event

See the [root README](../../README.md) for full usage examples.
