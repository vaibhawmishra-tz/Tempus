# Tempus.Business

Business-day arithmetic, SLA timers, recurring schedules, and fiscal calendars for .NET.

## Installation

```bash
dotnet add package Tempus.Business
dotnet add package Tempus.Holidays   # for holiday-aware calendars
```

## Quick start

```csharp
builder.Services
    .AddHolidays("CA")
    .AddBusinessCalendar(opts =>
    {
        opts.WorkWeek      = WorkWeek.MondayToFriday;
        opts.BusinessHours = BusinessHours.NineToFive("America/Toronto");
    })
    .AddSlaTimer()
    .AddFiscalCalendar();
```

## Key types

- `IBusinessCalendar` — business-day/hour queries and arithmetic
- `ISlaTimer` / `ISlaTimerFactory` — elapsed/remaining business-time tracking against a target
- `RecurringSchedule` — daily/weekly/monthly/yearly occurrence generation
- `FiscalCalendar` — fiscal-year and quarter queries with configurable start date

See the [root README](../../README.md) for full usage examples.
