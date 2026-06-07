using Tempus.Business.Abstractions;
using Tempus.Business.Models;

namespace Tempus.Business;

public sealed class BusinessCalendar : IBusinessCalendar
{
    private readonly BusinessCalendarOptions _options;
    private readonly IReadOnlyList<IHolidayProvider> _providers;

    public BusinessCalendar(BusinessCalendarOptions options, IEnumerable<IHolidayProvider>? providers = null)
    {
        ArgumentNullException.ThrowIfNull(options);
        _options = options;
        _providers = providers?.ToList() ?? [];
    }

    public bool IsBusinessDay(DateOnly calendarDate)
    {
        if (_options.WeekendDays.Contains(calendarDate.DayOfWeek))
            return false;
        foreach (var provider in _providers)
            if (provider.IsHoliday(calendarDate))
                return false;
        return true;
    }

    public bool IsBusinessHour(DateTimeOffset moment)
    {
        var tz = TimeZoneInfo.FindSystemTimeZoneById(_options.BusinessHours.TimeZoneId);
        var local = TimeZoneInfo.ConvertTime(moment, tz);
        var date = DateOnly.FromDateTime(local.DateTime);
        var time = TimeOnly.FromDateTime(local.DateTime);
        return IsBusinessDay(date) && _options.BusinessHours.Contains(time);
    }

    public DateOnly NextBusinessDay(DateOnly calendarDate)
    {
        var current = calendarDate.AddDays(1);
        for (int i = 0; i < _options.MaxIterations; i++)
        {
            if (IsBusinessDay(current))
                return current;
            current = current.AddDays(1);
        }
        throw new InvalidOperationException(
            $"No business day found within {_options.MaxIterations} days after {calendarDate}. Check your holiday provider.");
    }

    public DateOnly PreviousBusinessDay(DateOnly calendarDate)
    {
        var current = calendarDate.AddDays(-1);
        for (int i = 0; i < _options.MaxIterations; i++)
        {
            if (IsBusinessDay(current))
                return current;
            current = current.AddDays(-1);
        }
        throw new InvalidOperationException(
            $"No business day found within {_options.MaxIterations} days before {calendarDate}. Check your holiday provider.");
    }

    public DateOnly AddBusinessDays(DateOnly calendarDate, int days)
    {
        if (days == 0)
            return calendarDate;

        int direction = Math.Sign(days);
        int remaining = Math.Abs(days);
        var current = calendarDate;

        for (int guard = 0; guard < _options.MaxIterations && remaining > 0; guard++)
        {
            current = current.AddDays(direction);
            if (IsBusinessDay(current))
                remaining--;
        }

        if (remaining > 0)
            throw new InvalidOperationException(
                $"Could not add {days} business days within {_options.MaxIterations} iterations. Check your holiday provider.");

        return current;
    }

    public DateTimeOffset AddBusinessHours(DateTimeOffset moment, double hours)
    {
        if (hours == 0)
            return moment;

        var tz = TimeZoneInfo.FindSystemTimeZoneById(_options.BusinessHours.TimeZoneId);
        var local = TimeZoneInfo.ConvertTime(moment, tz);
        var date = DateOnly.FromDateTime(local.DateTime);
        var time = TimeOnly.FromDateTime(local.DateTime);

        if (hours > 0)
            return AddPositiveBusinessHours(date, time, hours, tz);
        else
            return AddNegativeBusinessHours(date, time, -hours, tz);
    }

    public int BusinessDaysBetween(DateOnly startDate, DateOnly endDate)
    {
        if (startDate >= endDate)
            return 0;

        int count = 0;
        var current = startDate;
        while (current < endDate)
        {
            if (IsBusinessDay(current))
                count++;
            current = current.AddDays(1);
        }
        return count;
    }

    public TimeSpan BusinessTimeBetween(DateTimeOffset startDate, DateTimeOffset endDate)
    {
        if (startDate >= endDate)
            return TimeSpan.Zero;

        var tz = TimeZoneInfo.FindSystemTimeZoneById(_options.BusinessHours.TimeZoneId);
        var localFrom = TimeZoneInfo.ConvertTime(startDate, tz);
        var localTo = TimeZoneInfo.ConvertTime(endDate, tz);

        var dateFrom = DateOnly.FromDateTime(localFrom.DateTime);
        var dateTo = DateOnly.FromDateTime(localTo.DateTime);
        var timeFrom = TimeOnly.FromDateTime(localFrom.DateTime);
        var timeTo = TimeOnly.FromDateTime(localTo.DateTime);

        double totalHours = 0;

        if (dateFrom == dateTo)
        {
            if (IsBusinessDay(dateFrom))
            {
                var effectiveFrom = TimeOnlyMax(timeFrom, _options.BusinessHours.Start);
                var effectiveTo = TimeOnlyMin(timeTo, _options.BusinessHours.End);
                if (effectiveTo > effectiveFrom)
                    totalHours = (effectiveTo - effectiveFrom).TotalHours;
            }
        }
        else
        {
            // First partial day
            if (IsBusinessDay(dateFrom))
            {
                var effectiveStart = TimeOnlyMax(timeFrom, _options.BusinessHours.Start);
                if (effectiveStart < _options.BusinessHours.End)
                    totalHours += (_options.BusinessHours.End - effectiveStart).TotalHours;
            }

            // Full middle days
            var current = dateFrom.AddDays(1);
            while (current < dateTo)
            {
                if (IsBusinessDay(current))
                    totalHours += _options.BusinessHours.Duration.TotalHours;
                current = current.AddDays(1);
            }

            // Last partial day
            if (IsBusinessDay(dateTo))
            {
                var effectiveEnd = TimeOnlyMin(timeTo, _options.BusinessHours.End);
                if (effectiveEnd > _options.BusinessHours.Start)
                    totalHours += (effectiveEnd - _options.BusinessHours.Start).TotalHours;
            }
        }

        return TimeSpan.FromHours(totalHours);
    }

    private DateTimeOffset AddPositiveBusinessHours(DateOnly date, TimeOnly time, double hours, TimeZoneInfo tz)
    {
        // Normalize: if outside business hours, jump to next business start
        if (!IsBusinessDay(date) || time >= _options.BusinessHours.End)
        {
            date = NextBusinessDay(date);
            time = _options.BusinessHours.Start;
        }
        else if (time < _options.BusinessHours.Start)
        {
            time = _options.BusinessHours.Start;
        }

        double remaining = hours;
        for (int guard = 0; guard < _options.MaxIterations; guard++)
        {
            double available = (_options.BusinessHours.End - time).TotalHours;
            if (remaining <= available)
            {
                time = time.Add(TimeSpan.FromHours(remaining));
                return BuildDateTimeOffset(date, time, tz);
            }
            remaining -= available;
            date = NextBusinessDay(date);
            time = _options.BusinessHours.Start;
        }

        throw new InvalidOperationException(
            $"Could not add {hours} business hours within {_options.MaxIterations} iterations.");
    }

    private DateTimeOffset AddNegativeBusinessHours(DateOnly date, TimeOnly time, double hours, TimeZoneInfo tz)
    {
        // Normalize: if outside business hours, jump to previous business end
        if (!IsBusinessDay(date) || time <= _options.BusinessHours.Start)
        {
            date = PreviousBusinessDay(date);
            time = _options.BusinessHours.End;
        }
        else if (time > _options.BusinessHours.End)
        {
            time = _options.BusinessHours.End;
        }

        double remaining = hours;
        for (int guard = 0; guard < _options.MaxIterations; guard++)
        {
            double available = (time - _options.BusinessHours.Start).TotalHours;
            if (remaining <= available)
            {
                time = time.Add(TimeSpan.FromHours(-remaining));
                return BuildDateTimeOffset(date, time, tz);
            }
            remaining -= available;
            date = PreviousBusinessDay(date);
            time = _options.BusinessHours.End;
        }

        throw new InvalidOperationException(
            $"Could not subtract {hours} business hours within {_options.MaxIterations} iterations.");
    }

    private static DateTimeOffset BuildDateTimeOffset(DateOnly date, TimeOnly time, TimeZoneInfo tz)
    {
        var local = new DateTime(date.Year, date.Month, date.Day, time.Hour, time.Minute, time.Second, DateTimeKind.Unspecified);
        return new DateTimeOffset(local, tz.GetUtcOffset(local));
    }

    private static TimeOnly TimeOnlyMax(TimeOnly a, TimeOnly b) => a > b ? a : b;
    private static TimeOnly TimeOnlyMin(TimeOnly a, TimeOnly b) => a < b ? a : b;
}
