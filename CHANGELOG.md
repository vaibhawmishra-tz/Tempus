# Changelog

All notable changes to Tempus are documented here.

Format follows [Keep a Changelog](https://keepachangelog.com/en/1.0.0/).
Versioning follows [Semantic Versioning](https://semver.org/).

**Publishing guide:** See [PUBLISHING.md](PUBLISHING.md) for version management and release procedures.

## [Unreleased]

### Added

#### Tempus.Core
- `ITempusClock`, `ITimezoneResolver`, `IDstResolver` abstractions
- `DateRange`, `ConversionResult`, `DstTransition` value types
- Extension methods on `DateTimeOffset`, `DateTime`, `DateOnly`
- `TempusOptions` with DST strategy, timezone source, and display config
- `TempusClockExtensions.StartOfDayUtc` — converts `DateOnly` to midnight UTC

#### Tempus.Business
- `IBusinessCalendar`, `IHolidayProvider`, `ISlaTimer`, `ISlaTimerFactory` abstractions
- `Holiday`, `BusinessHours` models
- `RecurringSchedule` with Daily/Weekly/Monthly/Yearly frequencies, `Count`/`Until` bounds, and bounded `GetOccurrences(from, to)` overload
- `FiscalCalendar` with short `(startMonth, startDay)` constructor
- `BusinessCalendarExtensions.GetBusinessDays` convenience overloads

#### Tempus.Holidays (new package)
- Embedded JSON data engine (`HolidayRuleEngine`) supporting `fixed`, `easter`, `nth-weekday`, `last-weekday`, `last-before`, `equinox`, and `boxing-day` rule types
- `EmbeddedHolidayProvider` backed by compiled-in JSON data files — zero file I/O at runtime
- `AddHolidays(params string[] regions)` DI extension with lazy registry
- Region data files: `CA`, `CA-ON`, `CA-BC`, `CA-QC`, `CA-AB`, `AU`, `AU-NSW`, `AU-VIC`, `AU-QLD`, `AU-WA`, `DE`, `DE-BW`, `JP`
- `since`/`until` rule date-range guards for time-bounded holidays (e.g. Japan Emperor's Birthday)
- Japan substitute holiday post-processing (`substituteRule: "japan"`)
- `dotnet new tempus-holiday` item template for custom holiday providers

#### Tempus.EFCore
- `UseTemporalConventions()` model builder extension registers value converters for all temporal CLR types

#### Tempus.AspNetCore
- `ITempusUserContext`, `TempusTimezoneMiddleware`
- `[UserLocalTime]`, `[RelativeTime]`, `[UtcTime]` JSON converter attributes

#### Tempus.Testing
- `FakeClock` with `Advance()`, `SetTo()`, `AdvanceDays()`

#### Infrastructure
- `Tempus.Sample.WebApi` sample app demonstrating every package
- BenchmarkDotNet benchmarks for `IBusinessCalendar`, `IHolidayProvider`, `IFiscalCalendar`
- FsCheck property-based tests for `RecurringSchedule` invariants
- XML documentation on all public interfaces and key value types
- MIT `LICENSE`, `.gitignore`, GitHub Actions CI (multi-OS) and release workflows
