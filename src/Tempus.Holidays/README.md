# Tempus.Holidays

Embedded holiday data engine for .NET — zero file I/O, zero external services. Holiday rules are compiled into the assembly as JSON resources and evaluated at runtime.

## Installation

```bash
dotnet add package Tempus.Holidays
```

## Supported regions

`CA`, `CA-ON`, `CA-BC`, `CA-QC`, `CA-AB`, `AU`, `AU-NSW`, `AU-VIC`, `AU-QLD`, `AU-WA`, `DE`, `DE-BW`, `JP`

## Quick start

```csharp
// Register one or more regions
builder.Services.AddHolidays("CA", "CA-ON", "JP");

// Consume
public class Scheduler(IEnumerable<IHolidayProvider> providers)
{
    bool IsCanadaHoliday(DateOnly date) =>
        providers.First(p => string.Equals(p.Region, "CA", StringComparison.Ordinal))
                 .IsHoliday(date);
}
```

## Custom provider template

```bash
dotnet new install Tempus.Templates
dotnet new tempus-holiday --name MyCompany --namespace Acme.Calendar
```

See the [root README](../../README.md) for full usage examples.
