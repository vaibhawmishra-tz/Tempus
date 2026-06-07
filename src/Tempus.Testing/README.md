# Tempus.Testing

`FakeClock` — a controllable `ITempusClock` implementation for deterministic unit tests.

## Installation

```bash
dotnet add package Tempus.Testing  # test projects only
```

## Quick start

```csharp
var clock = new FakeClock(new DateTimeOffset(2026, 1, 5, 9, 0, 0, TimeSpan.Zero));

// Inject into the service under test
var service = new MyService(clock);

clock.Advance(TimeSpan.FromHours(2));    // move forward 2 hours
clock.AdvanceDays(1);                   // move forward 1 day
clock.SetTo(new DateTimeOffset(...));   // jump to a specific instant

Assert.Equal(new DateOnly(2026, 1, 7), clock.TodayUtc);
```

See the [root README](../../README.md) for full usage examples.
