# Tempus.EFCore

EF Core value converters that ensure all temporal properties are stored and retrieved in UTC.

## Installation

```bash
dotnet add package Tempus.EFCore
```

## Quick start

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.UseTemporalConventions();
    // Registers UTC converters for DateTime, DateTimeOffset, DateOnly, TimeOnly
    // and their nullable variants across the entire model automatically.
}
```

No per-property configuration needed — `UseTemporalConventions()` scans all entity types at startup.

See the [root README](../../README.md) for full usage examples.
