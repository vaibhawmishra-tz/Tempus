# Tempus — Pending Tasks

Current state: **184 tests passing** across 5 test projects. All src packages build clean across net8/net9/net10.

---

## Completed

### P1 — EF Core ✅
`DateOnlyConverter`, `TimeOnlyConverter`, `UseTemporalConventions()`, `AddTempusEFCore()`, 18 tests.

### P2 — AspNetCore JSON ✅
`UtcDateTimeJsonConverter`, `UserLocalTimeJsonConverter`, `RelativeTimeJsonConverter`, `AddTempusJson()`, `TempusTypeInfoResolver`, 7 middleware tests.

### P3 — Tempus.Holidays (data-driven) ✅
Single package, 13 regions (CA + 4 provinces, AU + 4 states, DE + BW, JP), `AddHolidays("CA", "CA-ON")` DI extension, `dotnet new tempus-holiday` item template, 29 new tests.

### T1 — Sample API ✅
`samples/Tempus.Sample.WebApi` with endpoints for every feature: `/time/*`, `/business/*`, `/fiscal/*`, `/schedule/*`, `/holidays/{region}`, `/json-demo`. All packages wired in DI.

### T2 — P5 API gaps ✅
- `FiscalCalendar(int startMonth, int startDay)` short constructor
- `RecurringSchedule.GetOccurrences(DateOnly from, DateOnly to)` bounded overload
- `ITempusClock` extension `StartOfDayUtc(DateOnly date)`
- `IBusinessCalendar.GetBusinessDays` alias overloads
- Tests for all new methods

### T3 — Benchmarks ✅
`BusinessCalendarBenchmarks` (6), `HolidayProviderBenchmarks` (5), `FiscalCalendarBenchmarks` (6) — all using BenchmarkDotNet with `[MemoryDiagnoser]` and `[RankColumn]`.

### T4 — FsCheck property-based tests ✅
7 `[Property]` tests in `RecurringSchedulePropertyTests`: monotonic ordering, exact count, Until bound, OccursOn consistency (weekly/monthly), bounded range, interval spacing.

### T5 — XML documentation ✅
`<GenerateDocumentationFile>true</GenerateDocumentationFile>` enabled. Full `<summary>` (and member-level docs) on: `IDstResolver`, `ITimezoneResolver`, `ITempusClock`, `IBusinessCalendar`, `ISlaTimer`, `ISlaTimerFactory`, `IHolidayProvider`, `ConversionResult`, `DstTransition`, `DstTransitionType`, `DateRange`, `Holiday`, `BusinessHours`.

### T6 — Release infrastructure ✅
`LICENSE` (MIT), `.gitignore`, `CHANGELOG.md`, `.github/workflows/ci.yml` (multi-OS), `.github/workflows/release.yml` (tag-triggered NuGet publish + GitHub Release).

### T7 — README / User Guide ✅
Top-level `README.md` with package table, dependency diagram, quick-start, and per-section examples for every package. Per-package `README.md` stubs in all `src/*/` directories.

---

## Test coverage summary

| Project | Tests | Status |
|---------|-------|--------|
| `Tempus.Core.Tests` | 21 | ✅ All pass |
| `Tempus.Business.Tests` | 184 | ✅ All pass |
| `Tempus.AspNetCore.Tests` | 15 | ✅ All pass |
| `Tempus.EFCore.Tests` | 18 | ✅ All pass |
| `Tempus.Integration.Tests` | 11 | ✅ All pass |
| **Total** | **249** | |
