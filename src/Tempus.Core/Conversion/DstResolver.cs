using System.Diagnostics.CodeAnalysis;
using NodaTime;
using NodaTime.TimeZones;
using Tempus.Core.Abstractions;
using Tempus.Core.Configuration;
using Tempus.Core.Models;

namespace Tempus.Core.Conversion;

[SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Instantiated via DI")]
internal sealed class DstResolver : IDstResolver
{
    private readonly DstOptions _options;
    private readonly ITimezoneResolver _timezoneResolver;

    public DstResolver(DstOptions options, ITimezoneResolver timezoneResolver)
    {
        _options = options;
        _timezoneResolver = timezoneResolver;
    }

    public ConversionResult ToUtc(DateTime localDateTime, string ianaTimeZoneId)
        => ToUtc(localDateTime, _timezoneResolver.Resolve(ianaTimeZoneId));

    public ConversionResult ToUtc(DateTime localDateTime, TimeZoneInfo timeZone)
    {
        if (_options.StrictMode)
        {
            if (timeZone.IsAmbiguousTime(localDateTime))
                throw new InvalidOperationException(
                    $"'{localDateTime}' is ambiguous in '{timeZone.Id}'. Set StrictMode = false or handle ambiguity explicitly.");
            if (timeZone.IsInvalidTime(localDateTime))
                throw new InvalidOperationException(
                    $"'{localDateTime}' does not exist in '{timeZone.Id}' (DST spring-forward gap).");
        }

        if (timeZone.IsAmbiguousTime(localDateTime))
            return ResolveAmbiguous(localDateTime, timeZone);

        if (timeZone.IsInvalidTime(localDateTime))
            return ResolveInvalid(localDateTime, timeZone);

        DateTime utcDateTime = TimeZoneInfo.ConvertTimeToUtc(
            DateTime.SpecifyKind(localDateTime, DateTimeKind.Unspecified), timeZone);
        return ConversionResult.Clean(new DateTimeOffset(utcDateTime, TimeSpan.Zero));
    }

    public DstTransition? NextTransition(string ianaTimeZoneId)
        => NextTransition(ianaTimeZoneId, DateTimeOffset.UtcNow);

    public DstTransition? NextTransition(string ianaTimeZoneId, DateTimeOffset after)
    {
        var zone = GetZoneOrNull(ianaTimeZoneId);
        if (zone is null) return null;

        var instant = Instant.FromDateTimeOffset(after);
        var interval = zone.GetZoneInterval(instant);

        if (!interval.HasEnd) return null;

        return BuildTransition(ianaTimeZoneId, zone, interval);
    }

    public IReadOnlyList<DstTransition> TransitionsInYear(string ianaTimeZoneId, int year)
    {
        var zone = GetZoneOrNull(ianaTimeZoneId);
        if (zone is null) return [];

        var yearStart = Instant.FromUtc(year, 1, 1, 0, 0);
        var yearEnd = Instant.FromUtc(year + 1, 1, 1, 0, 0);

        var transitions = new List<DstTransition>();
        var interval = zone.GetZoneInterval(yearStart);

        while (interval.HasEnd && interval.End < yearEnd)
        {
            transitions.Add(BuildTransition(ianaTimeZoneId, zone, interval));
            interval = zone.GetZoneInterval(interval.End);
        }

        return transitions;
    }

    public bool IsNearTransition(DateTimeOffset utcTime, string ianaTimeZoneId, TimeSpan within)
    {
        DstTransition? next = NextTransition(ianaTimeZoneId, utcTime);
        if (next is null)
            return false;
        return (next.Value.OccursAt - utcTime) <= within;
    }

    private static DateTimeZone? GetZoneOrNull(string ianaTimeZoneId)
    {
        try { return DateTimeZoneProviders.Tzdb[ianaTimeZoneId]; }
        catch (DateTimeZoneNotFoundException) { return null; }
    }

    private static DstTransition BuildTransition(string ianaTimeZoneId, DateTimeZone zone, ZoneInterval before)
    {
        var transitionInstant = before.End;
        var afterInterval = zone.GetZoneInterval(transitionInstant);

        var offsetBeforeSecs = before.WallOffset.Seconds;
        var offsetAfterSecs = afterInterval.WallOffset.Seconds;

        var type = offsetAfterSecs > offsetBeforeSecs
            ? DstTransitionType.SpringForward
            : DstTransitionType.FallBack;

        var occursAt = transitionInstant.ToDateTimeOffset();
        var offsetBefore = TimeSpan.FromSeconds(offsetBeforeSecs);

        return new DstTransition
        {
            TimeZoneId = ianaTimeZoneId,
            OccursAt = occursAt,
            Type = type,
            OffsetBefore = offsetBefore,
            OffsetAfter = TimeSpan.FromSeconds(offsetAfterSecs),
            AffectedLocalTime = TimeOnly.FromTimeSpan(occursAt.ToOffset(offsetBefore).TimeOfDay)
        };
    }

    private ConversionResult ResolveAmbiguous(DateTime localDateTime, TimeZoneInfo timeZone)
    {
        TimeSpan[] offsets = timeZone.GetAmbiguousTimeOffsets(localDateTime);

        TimeSpan chosen = _options.OnAmbiguousTime switch
        {
            AmbiguousTimeStrategy.PreferStandard => offsets.Min(),
            AmbiguousTimeStrategy.PreferDaylight => offsets.Max(),
            AmbiguousTimeStrategy.PreferEarlier  => offsets.Max(),
            AmbiguousTimeStrategy.PreferLater    => offsets.Min(),
            _                                    => offsets.Min()
        };

        List<DateTimeOffset> candidates = offsets
            .Select(o => new DateTimeOffset(localDateTime, o))
            .ToList();

        DateTimeOffset value = new DateTimeOffset(localDateTime, chosen).ToUniversalTime();

        return new ConversionResult
        {
            Value = value,
            WasAmbiguous = true,
            StrategyApplied = _options.OnAmbiguousTime.ToString(),
            Candidates = candidates
        };
    }

    private ConversionResult ResolveInvalid(DateTime localDateTime, TimeZoneInfo timeZone)
    {
        DateTime adjusted = _options.OnInvalidTime switch
        {
            InvalidTimeStrategy.AdjustBackward => localDateTime.Subtract(TimeSpan.FromHours(1)),
            _                                  => localDateTime.Add(timeZone.BaseUtcOffset)
        };

        DateTime utcAdjusted = TimeZoneInfo.ConvertTimeToUtc(
            DateTime.SpecifyKind(adjusted, DateTimeKind.Unspecified), timeZone);

        return new ConversionResult
        {
            Value = new DateTimeOffset(utcAdjusted, TimeSpan.Zero),
            WasInvalid = true,
            StrategyApplied = _options.OnInvalidTime.ToString()
        };
    }
}
