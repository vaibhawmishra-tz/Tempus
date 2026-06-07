# Tempus

A suite of .NET packages for correct, production-grade datetime handling — timezone conversions, DST safety, business calendars, fiscal calendars, SLA timers, recurring schedules, and holiday data.

## Why Tempus?

- **Timezone-aware from the start**: Built around `DateTimeOffset` and IANA timezone identifiers
- **DST-safe**: Handles ambiguous times during daylight saving transitions automatically
- **Business logic ready**: Business calendars, SLA timers, recurring schedules out of the box
- **Holiday data**: Embedded holiday providers for 10+ countries/regions
- **Testable**: Inject `ITempusClock` instead of `DateTime.UtcNow` for deterministic tests
- **Async-friendly**: Designed for modern async .NET patterns
- **EF Core integrated**: Automatic temporal value converters
- **Web-ready**: ASP.NET Core middleware and JSON attribute converters

## Packages at a Glance

| Package | NuGet | Purpose |
|---|---|---|
| **`Tempus.Core`** | [![NuGet](https://img.shields.io/nuget/v/Tempus.Core)](https://www.nuget.org/packages/Tempus.Core) | Fundamental: clock injection, timezone/DST resolution, date ranges |
| **`Tempus.Business`** | [![NuGet](https://img.shields.io/nuget/v/Tempus.Business)](https://www.nuget.org/packages/Tempus.Business) | Business calendars, SLA timers, recurring schedules, fiscal calendars |
| **`Tempus.Holidays`** | [![NuGet](https://img.shields.io/nuget/v/Tempus.Holidays)](https://www.nuget.org/packages/Tempus.Holidays) | Multi-region holiday provider engine (CA, AU, DE, JP, etc.) |
| **`Tempus.Holidays.US`** | [![NuGet](https://img.shields.io/nuget/v/Tempus.Holidays.US)](https://www.nuget.org/packages/Tempus.Holidays.US) | US federal + state holidays (legacy provider) |
| **`Tempus.AspNetCore`** | [![NuGet](https://img.shields.io/nuget/v/Tempus.AspNetCore)](https://www.nuget.org/packages/Tempus.AspNetCore) | Timezone middleware, `[UserLocalTime]`/`[UtcTime]` JSON converters |
| **`Tempus.EFCore`** | [![NuGet](https://img.shields.io/nuget/v/Tempus.EFCore)](https://www.nuget.org/packages/Tempus.EFCore) | EF Core value converters for all temporal types |
| **`Tempus.Testing`** | [![NuGet](https://img.shields.io/nuget/v/Tempus.Testing)](https://www.nuget.org/packages/Tempus.Testing) | `FakeClock` for deterministic unit tests |

## Dependency Diagram

```
Tempus.Core (foundation)
├── Tempus.Business
│   ├── Tempus.Holidays (multi-region engine)
│   │   ├── Tempus.Holidays.US
│   │   ├── Tempus.Holidays.UK
│   │   ├── Tempus.Holidays.IN
│   │   ├── Tempus.Holidays.EU
│   │   └── Tempus.Holidays.Exchange
│   └── Tempus.AspNetCore
├── Tempus.EFCore
└── Tempus.Testing
```

---

## Quick Start

Choose your configuration level based on needs:

### ⚡ Minimal: Just Clock Injection

For applications that only need testable datetime access:

```bash
dotnet add package Tempus.Core
```

```csharp
// Program.cs
builder.Services.AddTempus();

// Service.cs
public class MyService(ITempusClock clock)
{
    public DateOnly Today => clock.TodayUtc;
    public DateTimeOffset NowInZone(string tz) => clock.NowIn(tz);
}

// Tests
var fakeClock = new FakeClock(new DateTimeOffset(2026, 1, 1, 9, 0, 0, TimeSpan.Zero));
var service = new MyService(fakeClock);
fakeClock.Advance(TimeSpan.FromHours(2));
```

### 📅 Intermediate: Business Logic

For scheduling, SLAs, recurring events, and holiday awareness:

```bash
dotnet add package Tempus.Core
dotnet add package Tempus.Business
dotnet add package Tempus.Holidays
```

```csharp
// Program.cs
builder.Services
    .AddTempus()
    .AddHolidays("CA", "CA-ON")  // Canada + Ontario
    .AddBusinessCalendar(opts =>
    {
        opts.WorkWeek = WorkWeek.MondayToFriday;
        opts.BusinessHours = BusinessHours.NineToFive("America/Toronto");
    })
    .AddSlaTimer();

// Usage
public class TicketService(IBusinessCalendarFactory calFactory, ISlaTimerFactory slaFactory)
{
    public void CheckSla(DateTimeOffset ticketCreated)
    {
        var calendar = calFactory.Default;
        var sla = slaFactory.Create(ticketCreated, TimeSpan.FromHours(8));
        
        if (sla.IsBreached)
            Console.WriteLine("Ticket breached SLA!");
        
        // Next business day
        var dueDate = calendar.AddBusinessDays(DateOnly.FromDateTime(ticketCreated.Date), 2);
    }
}
```

### 🚀 Full-Featured: Web + Database

For ASP.NET Core applications with user timezones and EF Core:

```bash
dotnet add package Tempus.Core
dotnet add package Tempus.Business
dotnet add package Tempus.Holidays
dotnet add package Tempus.AspNetCore
dotnet add package Tempus.EFCore
```

```csharp
// Program.cs
builder.Services
    .AddTempus()
    .AddHolidays("CA", "CA-ON", "AU", "DE", "JP")
    .AddBusinessCalendar(opts =>
    {
        opts.WorkWeek = WorkWeek.MondayToFriday;
        opts.BusinessHours = BusinessHours.NineToFive("America/Toronto");
    })
    .AddFiscalCalendar(opts => opts.StartMonth = 4)  // April fiscal year
    .AddSlaTimer()
    .AddControllers()
    .AddTempusJson();  // JSON converters

app.UseTempusTimezone();  // Reads X-Timezone header or Accept-Language

// DbContext.cs
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.UseTemporalConventions();  // Auto-converters for all temporal types
}

// DTO.cs
public class EventDto
{
    [UtcTime] public DateTimeOffset CreatedAt { get; set; }       // Always UTC in JSON
    [UserLocalTime] public DateTimeOffset StartsAt { get; set; }  // Converted to user's TZ
    [RelativeTime] public DateTimeOffset UpdatedAt { get; set; }  // "2 hours ago"
}
```

### 🧪 Testing

For test projects:

```bash
dotnet add package Tempus.Testing
```

```csharp
[Fact]
public void CalculatesCorrectSla()
{
    var clock = new FakeClock(new DateTimeOffset(2026, 1, 1, 9, 0, 0, TimeSpan.Zero));
    var sla = new SlaTimer(clock.UtcNow, TimeSpan.FromHours(8), calendar: null);
    
    clock.Advance(TimeSpan.FromHours(7));
    Assert.False(sla.IsBreached);
    
    clock.Advance(TimeSpan.FromHours(2));
    Assert.True(sla.IsBreached);
}
```

---

---

## Detailed Documentation

### Tempus.Core

**Core types for timezone-aware datetime handling.**

#### ITempusClock — Testable Clock Injection

Inject instead of `DateTime.UtcNow`. Always returns UTC `DateTimeOffset`.

```csharp
public class MyService(ITempusClock clock)
{
    public DateOnly Today => clock.TodayUtc;
    public DateTimeOffset NowInToronto() => clock.NowIn("America/Toronto");
    public DateTimeOffset Now => clock.UtcNow;
}
```

**Why?** Enables deterministic unit tests by injecting a fake clock.

---

#### ITimezoneResolver — IANA ↔ Windows Conversion

```csharp
public class TimezoneLookup(ITimezoneResolver resolver)
{
    public void Resolve()
    {
        TimeZoneInfo tz = resolver.Resolve("America/New_York");
        string windowsId = resolver.ToWindowsId("America/New_York");  // "Eastern Standard Time"
        string ianaId = resolver.ToIanaId("Eastern Standard Time");   // "America/New_York"
    }
}
```

**Why?** Windows and IANA use different timezone identifiers. Tempus handles the mapping.

---

#### IDstResolver — Ambiguous Time Handling

Automatically resolves ambiguous times during DST transitions (when clocks "fall back").

```csharp
public class DstHandler(IDstResolver dst)
{
    public void HandleAmbiguousTime()
    {
        // Nov 1, 2026: 1:30 AM happens twice during "fall back"
        var result = dst.ToUtc(
            new DateTime(2026, 11, 1, 1, 30, 0), 
            "America/New_York"
        );
        
        if (result.WasAmbiguous)
            Console.WriteLine($"Resolved using {result.StrategyApplied}");
        
        // Check if near a DST transition
        var next = dst.NextTransition("America/New_York");
        bool near = dst.IsNearTransition(DateTimeOffset.UtcNow, "America/New_York", TimeSpan.FromHours(48));
    }
}
```

**Why?** DST transitions cause bugs. Tempus detects and resolves them automatically.

---

#### DateRange — Date Span Utilities

```csharp
var range = new DateRange(new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31));

Console.WriteLine(range.TotalDays);                          // 365
Console.WriteLine(range.Contains(new DateOnly(2026, 6, 15))); // true

foreach (DateOnly date in range.ToDates()) { /* iterate */ }

DateRange? overlap = range.Intersect(other);
```

---

#### Extension Methods

```csharp
// Convert local time to UTC safely
DateTimeOffset utc = someLocal.ToUtcSafe("America/New_York");

// Convert UTC to any timezone
DateTimeOffset local = utc.InZone("Europe/Berlin");

// Check DST status
bool isDst = utc.IsDst("America/Chicago");

// Day boundaries in any timezone
DateTimeOffset startOfDay = utc.StartOfDay("Asia/Tokyo");
DateTimeOffset endOfDay = utc.EndOfDay("Asia/Tokyo");
```

---

### Tempus.Business

**Business logic: calendars, SLAs, schedules, fiscal years.**

#### IBusinessCalendar — Work Day Calculations

```csharp
public class SchedulingService(IBusinessCalendarFactory calFactory)
{
    public void Schedule()
    {
        var calendar = calFactory.Default;
        
        bool isWorkday = calendar.IsBusinessDay(new DateOnly(2026, 12, 25)); // false (Christmas)
        DateOnly next = calendar.NextBusinessDay(new DateOnly(2026, 12, 24));
        DateOnly due = calendar.AddBusinessDays(new DateOnly(2026, 1, 1), 10);
        int elapsed = calendar.BusinessDaysBetween(from, to);
    }
}
```

**Configuration:**
```csharp
builder.Services.AddBusinessCalendar(opts =>
{
    opts.WorkWeek = WorkWeek.MondayToFriday;
    opts.BusinessHours = BusinessHours.NineToFive("America/Toronto");
});
```

**Why?** Automatically accounts for weekends and holidays when calculating due dates.

---

#### ISlaTimer / ISlaTimerFactory — SLA Breach Detection

```csharp
public class TicketService(ISlaTimerFactory slaFactory)
{
    public void CheckSla(DateTimeOffset ticketCreated)
    {
        ISlaTimer sla = slaFactory.Create(ticketCreated, TimeSpan.FromHours(8));
        
        Console.WriteLine(sla.ElapsedBusinessTime);      // time spent on ticket
        Console.WriteLine(sla.RemainingBusinessTime);    // time left to resolve
        Console.WriteLine(sla.IsBreached);               // breached?
        Console.WriteLine(sla.BreachesAt);               // when will it breach?
    }
}
```

**Why?** Tracks SLA compliance automatically, accounting for business hours and holidays.

---

#### RecurringSchedule — Event Recurrence

```csharp
var schedule = new RecurringSchedule
{
    Start = new DateOnly(2026, 1, 5),  // Monday
    Frequency = RecurrenceFrequency.Weekly,
    Count = 10,
};

foreach (DateOnly occurrence in schedule.GetOccurrences()) { /* ... */ }

// Efficient bounded queries (no full enumeration)
var inQ2 = schedule.GetOccurrences(
    new DateOnly(2026, 4, 1),
    new DateOnly(2026, 6, 30)
);

bool happens = schedule.OccursOn(new DateOnly(2026, 3, 9)); // true
```

**Frequencies:** `Daily`, `Weekly`, `BiWeekly`, `Monthly`, `Quarterly`, `Annually`

**Why?** Avoid reinventing recurrence logic; this is a complex domain.

---

#### FiscalCalendar — Fiscal Year Tracking

```csharp
// April fiscal year (UK/AU style)
var fiscal = new FiscalCalendar(startMonth: 4, startDay: 1);

int year = fiscal.GetFiscalYear(new DateOnly(2026, 5, 1));      // 2026
int quarter = fiscal.GetFiscalQuarter(new DateOnly(2026, 5, 1)); // Q1
DateOnly quarterStart = fiscal.GetQuarterStart(2026, 1);         // 2026-04-01
```

**Why?** Fiscal calendars often differ from calendar years; map dates correctly for reporting.

---

### Tempus.Holidays

**Multi-region holiday data engine.**

#### Supported Regions

| Code | Region |
|------|--------|
| `CA` | Canada (national) |
| `CA-ON` | Ontario |
| `CA-BC` | British Columbia |
| `CA-QC` | Québec |
| `CA-AB` | Alberta |
| `AU` | Australia (national) |
| `AU-NSW` | New South Wales |
| `AU-VIC` | Victoria |
| `AU-QLD` | Queensland |
| `AU-WA` | Western Australia |
| `DE` | Germany (national) |
| `DE-BW` | Baden-Württemberg |
| `JP` | Japan (with substitute holiday rules) |

#### Usage

```csharp
builder.Services.AddHolidays("CA", "CA-ON", "DE", "JP");

public class HolidayService(IEnumerable<IHolidayProvider> providers)
{
    public IHolidayProvider For(string region) =>
        providers.First(p => string.Equals(p.Region, region, StringComparison.Ordinal));
}

var ca = service.For("CA");
bool isHoliday = ca.IsHoliday(new DateOnly(2026, 7, 1)); // Canada Day
IEnumerable<Holiday> holidays = ca.GetHolidays(2026);
```

#### Custom Holiday Provider

```bash
dotnet new install Tempus.Templates
dotnet new tempus-holiday --name MyCompany --namespace Acme.Calendar
```

**Why?** Embed holiday data in your app; no external APIs or maintenance burden.

---

### Tempus.AspNetCore

**ASP.NET Core integration: timezone middleware, JSON converters.**

#### Middleware — Automatic User Timezone Detection

```csharp
app.UseTempusTimezone();
```

Reads timezone from:
1. `X-Timezone` header (e.g., `"America/Toronto"`)
2. `Accept-Language` header (if timezone can be inferred)
3. Falls back to UTC

Available via `ITempusUserContext`:

```csharp
public class EventController(ITempusUserContext ctx)
{
    [HttpGet]
    public IActionResult GetEvent(int id)
    {
        var userTz = ctx.UserTimezone;  // "America/Toronto"
        // ...
    }
}
```

---

#### JSON Attributes — Automatic Serialization

```csharp
public class EventDto
{
    [UtcTime]
    public DateTimeOffset CreatedAt { get; set; }       // Always UTC in JSON

    [UserLocalTime]
    public DateTimeOffset StartsAt { get; set; }        // Converted to user's timezone

    [RelativeTime]
    public DateTimeOffset UpdatedAt { get; set; }       // "2 hours ago", "1 day from now"
}
```

**Why?** Automatically converts times in/out of the user's timezone without manual code.

---

### Tempus.EFCore

**Entity Framework Core temporal type converters.**

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.UseTemporalConventions();
    // Auto-registers converters for:
    // - DateTime, DateTime?
    // - DateTimeOffset, DateTimeOffset?
    // - DateOnly, DateOnly?
    // - TimeOnly, TimeOnly?
}
```

**Why?** Ensures your database stores temporal data safely and consistently.

---

### Tempus.Testing

**Deterministic testing with FakeClock.**

```csharp
[Fact]
public void SlaIsBreached_WhenTargetExceeded()
{
    var clock = new FakeClock(new DateTimeOffset(2026, 1, 1, 9, 0, 0, TimeSpan.Zero));
    var sla = new SlaTimer(clock.UtcNow, TimeSpan.FromHours(8), calendar: null);
    
    clock.Advance(TimeSpan.FromHours(7));
    Assert.False(sla.IsBreached);
    
    clock.Advance(TimeSpan.FromHours(2));
    Assert.True(sla.IsBreached);
}
```

**Methods:**
- `clock.Advance(TimeSpan)` — Skip forward
- `clock.AdvanceDays(int)` — Skip days
- `clock.SetTo(DateTimeOffset)` — Jump to exact time

---

## Contributing

### Getting Started

1. **Clone and build:**
   ```bash
   git clone https://github.com/[username]/Tempus.git
   cd Tempus
   dotnet build
   ```

2. **Run all tests:**
   ```bash
   dotnet test
   ```

3. **Make changes:**
   - Create a feature branch: `git checkout -b feature/my-feature`
   - Keep commits focused and atomic
   - Include tests for new functionality
   - Ensure no compiler warnings

4. **Validate before pushing:**
   ```bash
   dotnet build --configuration Release
   dotnet test
   ```

5. **Submit a PR** — CI runs on Ubuntu, Windows, and macOS.

### Code Style

- Follow C# naming conventions (PascalCase for public members, camelCase for locals)
- Use nullable reference types (`#nullable enable`)
- Write XML documentation for public APIs
- Prefer expression-bodied members where readable

### Testing

- Write unit tests for all public APIs
- Use `Tempus.Testing.FakeClock` for datetime-dependent tests
- Aim for >80% code coverage on new code

---

## License

MIT — see [LICENSE](LICENSE).

## Changelog

See [CHANGELOG.md](CHANGELOG.md) for release history and version information.

---

**Built with ❤️ for developers who care about time.**
