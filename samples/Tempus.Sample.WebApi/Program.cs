using System.Globalization;
using Microsoft.OpenApi;
using Tempus.AspNetCore.DependencyInjection;
using Tempus.AspNetCore.Extensions;
using Tempus.Business.Abstractions;
using Tempus.Business.Calendar;
using Tempus.Business.DependencyInjection;
using Tempus.Business.Schedule;
using Tempus.Core.Abstractions;
using Tempus.Core.DependencyInjection;
using Tempus.Core.Extensions;
using Tempus.Holidays;
using Tempus.Holidays.US;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title       = "Tempus Sample API",
        Version     = "v1",
        Description = "Demonstrates Tempus: timezone-aware time handling, business calendars, fiscal years, SLA timers, and holiday providers.",
    });
    var xmlPath = Path.Combine(AppContext.BaseDirectory, $"{typeof(Program).Assembly.GetName().Name}.xml");
    if (File.Exists(xmlPath))
        options.IncludeXmlComments(xmlPath);
});

// ── Tempus.Core: clock + DST-safe timezone resolution ────────────────────────
builder.Services.AddTempus(options =>
{
    options.DefaultTimeZone = "America/Toronto";
    options.Dst.OnAmbiguousTime = Tempus.Core.Configuration.AmbiguousTimeStrategy.PreferStandard;
    options.Dst.OnInvalidTime  = Tempus.Core.Configuration.InvalidTimeStrategy.AdjustForward;
});

// ── Tempus.Holidays: data-driven regions (single package) ────────────────────
builder.Services.AddHolidays("CA", "CA-ON", "AU", "DE", "JP");
builder.Services.AddHolidayProvider<UsHolidayProvider>(); // legacy hard-coded provider

// ── Tempus.Business: calendar + fiscal year + SLA timers ─────────────────────
builder.Services.AddBusinessCalendar(opts =>
{
    opts.BusinessHours = new Tempus.Business.Models.BusinessHours
    {
        Start      = new TimeOnly(9, 0),
        End        = new TimeOnly(17, 0),
        TimeZoneId = "America/Toronto",
    };
});
builder.Services.AddFiscalCalendar(opts =>
{
    opts.StartMonth = 4;                          // April fiscal year (CA/UK style)
    opts.StartDay   = 1;
    opts.Naming     = FiscalYearNaming.EndYear;   // FY2026 = Apr 2025 – Mar 2026
});
builder.Services.AddSlaTimer();

// ── Tempus.AspNetCore: timezone middleware + attribute-based JSON converters ──
// AddTempusJson() wires [UtcTime], [UserLocalTime], [RelativeTime] for MVC.
// See GET /json-demo for a live demo — pass X-Timezone: America/New_York.
builder.Services.AddControllers().AddTempusJson();

WebApplication app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Tempus API v1");
    options.RoutePrefix = "swagger";
});

app.UseHttpsRedirection();

// Reads user timezone from X-Timezone header, ?tz= query param, or JWT "timezone" claim.
app.UseTempusTimezone();

app.MapControllers();

// ── /time ─────────────────────────────────────────────────────────────────────

app.MapGet("/time/utc", (ITempusClock clock) => new
{
    utc   = clock.UtcNow,
    iso   = clock.UtcNow.ToString("o"),
    today = clock.TodayUtc,
}).WithName("GetUtcTime").WithTags("Time");

app.MapGet("/time/convert", (string tz, ITempusClock clock) =>
{
    var local = clock.UtcNow.ToZone(tz);
    return Results.Ok(new
    {
        zone           = tz,
        local          = local.ToString("o"),
        offset         = local.Offset.ToString(),
        sample2HoursAgo = clock.UtcNow.AddHours(-2).ToRelativeString(),
    });
}).WithName("ConvertTime").WithTags("Time");

app.MapGet("/time/zones", () =>
    TimeZoneInfo.GetSystemTimeZones()
        .Select(z => new { z.Id, z.DisplayName, utcOffset = z.BaseUtcOffset.ToString() })
).WithName("ListTimezones").WithTags("Time");

// ── /business ─────────────────────────────────────────────────────────────────

app.MapGet("/business/is-business-day", (string date, IBusinessCalendar cal) =>
{
    var d = DateOnly.Parse(date, CultureInfo.InvariantCulture);
    return new { date = d, isBusinessDay = cal.IsBusinessDay(d) };
}).WithName("IsBusinessDay").WithTags("Business");

app.MapGet("/business/next", (string date, IBusinessCalendar cal) =>
{
    var d = DateOnly.Parse(date, CultureInfo.InvariantCulture);
    return new { from = d, next = cal.NextBusinessDay(d), previous = cal.PreviousBusinessDay(d) };
}).WithName("NextBusinessDay").WithTags("Business");

app.MapGet("/business/add-days", (string date, int days, IBusinessCalendar cal) =>
{
    var d = DateOnly.Parse(date, CultureInfo.InvariantCulture);
    return new { from = d, days, result = cal.AddBusinessDays(d, days) };
}).WithName("AddBusinessDays").WithTags("Business");

app.MapGet("/business/days-between", (string from, string to, IBusinessCalendar cal) =>
{
    var f = DateOnly.Parse(from, CultureInfo.InvariantCulture);
    var t = DateOnly.Parse(to,   CultureInfo.InvariantCulture);
    return new { from = f, to = t, businessDays = cal.BusinessDaysBetween(f, t) };
}).WithName("BusinessDaysBetween").WithTags("Business");

app.MapGet("/business/is-hour", (string at, IBusinessCalendar cal) =>
{
    var moment = DateTimeOffset.Parse(at, CultureInfo.InvariantCulture);
    return new { at = moment, isBusinessHour = cal.IsBusinessHour(moment) };
}).WithName("IsBusinessHour").WithTags("Business");

app.MapGet("/business/sla-deadline", (string start, double hours, IBusinessCalendar cal) =>
{
    var s        = DateTimeOffset.Parse(start, CultureInfo.InvariantCulture);
    var deadline = cal.AddBusinessHours(s, hours);
    var elapsed  = cal.BusinessTimeBetween(s, deadline);
    return new { start = s, slaHours = hours, deadline, elapsedBusinessHours = elapsed.TotalHours };
}).WithName("SlaDeadline").WithTags("Business");

// ── /fiscal ───────────────────────────────────────────────────────────────────

app.MapGet("/fiscal/info", (string date, FiscalCalendar fiscal) =>
{
    var d  = DateOnly.Parse(date, CultureInfo.InvariantCulture);
    int fy = fiscal.GetFiscalYear(d);
    int fq = fiscal.GetFiscalQuarter(d);
    return new
    {
        date          = d,
        fiscalYear    = fy,
        fiscalQuarter = fq,
        fyStart       = fiscal.GetFiscalYearStart(fy),
        fyEnd         = fiscal.GetFiscalYearEnd(fy),
    };
}).WithName("FiscalInfo").WithTags("Fiscal");

app.MapGet("/fiscal/quarter", (int year, int q, FiscalCalendar fiscal) => new
{
    fiscalYear = year,
    quarter    = q,
    start      = fiscal.GetQuarterStart(year, q),
    end        = fiscal.GetQuarterEnd(year, q),
}).WithName("FiscalQuarter").WithTags("Fiscal");

// ── /schedule ─────────────────────────────────────────────────────────────────

app.MapGet("/schedule/occurrences", (string start, string freq, int interval, int count) =>
{
    var schedule = new RecurringSchedule
    {
        Start     = DateOnly.Parse(start, CultureInfo.InvariantCulture),
        Frequency = Enum.Parse<RecurrenceFrequency>(freq, ignoreCase: true),
        Interval  = interval > 0 ? interval : 1,
        Count     = count  > 0 ? count  : 10,
    };
    return new
    {
        schedule    = new { start = schedule.Start, freq = schedule.Frequency, interval = schedule.Interval },
        occurrences = schedule.GetOccurrences().ToList(),
    };
}).WithName("ScheduleOccurrences").WithTags("Schedule");

// ── /holidays ─────────────────────────────────────────────────────────────────

app.MapGet("/holidays", (IEnumerable<IHolidayProvider> providers) =>
    providers.Select(p => p.Region).OrderBy(r => r, StringComparer.Ordinal)
).WithName("ListHolidayRegions").WithTags("Holidays");

app.MapGet("/holidays/{region}", (string region, int? year, IEnumerable<IHolidayProvider> providers) =>
{
    int y = year ?? DateOnly.FromDateTime(DateTime.Today).Year;
    var provider = providers.FirstOrDefault(p =>
        string.Equals(p.Region, region, StringComparison.OrdinalIgnoreCase));

    if (provider is null)
    {
        var available = providers.Select(p => p.Region).OrderBy(r => r, StringComparer.Ordinal);
        return Results.NotFound(new { error = $"Region '{region}' not registered.", available });
    }

    var holidays = provider.GetHolidays(y)
        .OrderBy(h => h.Date)
        .Select(h => new { h.Date, h.Name, h.IsNational });
    return Results.Ok(new { region = provider.Region, year = y, holidays });
}).WithName("GetHolidays").WithTags("Holidays");

app.Run();
