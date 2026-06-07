# Tempus.AspNetCore

ASP.NET Core middleware and JSON attributes for automatic timezone-aware serialisation.

## Installation

```bash
dotnet add package Tempus.AspNetCore
```

## Quick start

```csharp
// Program.cs
builder.Services.AddControllers().AddTempusJson();
app.UseTempusTimezone(); // reads X-Timezone header or Accept-Language
```

```csharp
public class EventDto
{
    [UtcTime]       public DateTimeOffset CreatedAt  { get; set; } // UTC in JSON
    [UserLocalTime] public DateTimeOffset StartsAt   { get; set; } // user's local time
    [RelativeTime]  public DateTimeOffset UpdatedAt  { get; set; } // "3 minutes ago"
}
```

## ITempusUserContext

```csharp
public class MyController(ITempusUserContext userCtx) : ControllerBase
{
    [HttpGet("now")]
    public IActionResult Now() => Ok(userCtx.Now);   // current time in user's timezone
}
```

See the [root README](../../README.md) for full usage examples.
