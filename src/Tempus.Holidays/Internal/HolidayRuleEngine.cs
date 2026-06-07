using Tempus.Business.Models;

namespace Tempus.Holidays.Internal;

internal static class HolidayRuleEngine
{
    internal static List<Holiday> Compute(HolidayDataFile data, int year)
    {
        var holidays = new List<Holiday>(data.Rules.Length + 4);

        foreach (var rule in data.Rules)
        {
            if (rule.Since.HasValue && year < rule.Since.Value) continue;
            if (rule.Until.HasValue && year > rule.Until.Value) continue;

            if (string.Equals(rule.Type, "boxing-day", StringComparison.Ordinal))
            {
                string name = string.IsNullOrEmpty(rule.Name) ? "Boxing Day" : rule.Name;
                holidays.Add(Make(name, ComputeBoxingDay(year, data), data.Region));
                continue;
            }

            holidays.Add(Make(rule.Name, ComputeDate(rule, year), data.Region));
        }

        if (string.Equals(data.SubstituteRule, "japan", StringComparison.Ordinal))
            ApplyJapanSubstitutes(holidays, data.Region);

        return holidays;
    }

    // ── rule computation ──────────────────────────────────────────────────────

    private static DateOnly ComputeDate(HolidayRule rule, int year) => rule.Type switch
    {
        "fixed"       => Observe(new DateOnly(year, rule.Month, rule.Day), rule.Observe),
        "easter"      => Easter(year).AddDays(rule.Offset),
        "nth"         => NthWeekday(year, rule.Month, ParseDay(rule.Weekday), rule.N),
        "last"        => LastWeekday(year, rule.Month, ParseDay(rule.Weekday)),
        "last-before" => LastWeekdayBefore(year, rule.Month, rule.Day, ParseDay(rule.Weekday)),
        "equinox"     => Equinox(year, rule.Which),
        _ => throw new NotSupportedException($"Unknown holiday rule type '{rule.Type}' in region '{rule.Name}'")
    };

    // Boxing Day is always 1 day after Christmas Day's observed date.
    // Looks up the Christmas Day rule in the data file to honour its observance setting.
    private static DateOnly ComputeBoxingDay(int year, HolidayDataFile data)
    {
        HolidayRule? xmasRule = Array.Find(data.Rules, r =>
            string.Equals(r.Type, "fixed", StringComparison.Ordinal) && r.Month == 12 && r.Day == 25);

        DateOnly christmas = xmasRule is not null
            ? Observe(new DateOnly(year, 12, 25), xmasRule.Observe)
            : new DateOnly(year, 12, 25);

        DateOnly boxing = christmas.AddDays(1);

        // If Boxing Day falls on a weekend after the shift push it to Monday/Tuesday
        return boxing.DayOfWeek switch
        {
            DayOfWeek.Saturday => boxing.AddDays(2), // → Monday
            DayOfWeek.Sunday   => boxing.AddDays(1), // → Monday
            _                  => boxing
        };
    }

    // ── observance rules ──────────────────────────────────────────────────────

    private static DateOnly Observe(DateOnly date, string? rule) => rule switch
    {
        // Sat → Fri, Sun → Mon  (US-style)
        "sat-fri-sun-mon" => date.DayOfWeek switch
        {
            DayOfWeek.Saturday => date.AddDays(-1),
            DayOfWeek.Sunday   => date.AddDays(1),
            _ => date
        },
        // Sat → Mon, Sun → Mon  (CA / AU-style)
        "sat-mon-sun-mon" => date.DayOfWeek switch
        {
            DayOfWeek.Saturday or DayOfWeek.Sunday => NextWeekday(date, DayOfWeek.Monday),
            _ => date
        },
        _ => date
    };

    // ── Japan substitute holiday ──────────────────────────────────────────────

    private static void ApplyJapanSubstitutes(List<Holiday> holidays, string region)
    {
        var dates = holidays.Select(h => h.Date).ToHashSet();
        var substitutes = new List<Holiday>(2);

        foreach (var holiday in holidays.Where(h => h.Date.DayOfWeek == DayOfWeek.Sunday))
        {
            // Find the next working day that isn't already a holiday
            DateOnly candidate = holiday.Date.AddDays(1);
            while (dates.Contains(candidate))
                candidate = candidate.AddDays(1);

            dates.Add(candidate);
            substitutes.Add(Make($"{holiday.Name} (Substitute)", candidate, region));
        }

        holidays.AddRange(substitutes);
    }

    // ── date helpers ──────────────────────────────────────────────────────────

    // Nth occurrence of a weekday in a month (1-based)
    private static DateOnly NthWeekday(int year, int month, DayOfWeek dow, int n)
    {
        var first = new DateOnly(year, month, 1);
        int skip = ((int)dow - (int)first.DayOfWeek + 7) % 7;
        return first.AddDays(skip + (n - 1) * 7);
    }

    // Last occurrence of a weekday in a month
    private static DateOnly LastWeekday(int year, int month, DayOfWeek dow)
    {
        var last = new DateOnly(year, month, DateTime.DaysInMonth(year, month));
        int back = ((int)last.DayOfWeek - (int)dow + 7) % 7;
        return last.AddDays(-back);
    }

    // Last occurrence of a weekday on or before month/day
    private static DateOnly LastWeekdayBefore(int year, int month, int day, DayOfWeek dow)
    {
        var date = new DateOnly(year, month, day);
        int back = ((int)date.DayOfWeek - (int)dow + 7) % 7;
        return back == 0 ? date : date.AddDays(-back);
    }

    // Next occurrence of a weekday on or after date
    private static DateOnly NextWeekday(DateOnly date, DayOfWeek dow)
    {
        int days = ((int)dow - (int)date.DayOfWeek + 7) % 7;
        return date.AddDays(days == 0 ? 7 : days);
    }

    // Approximate equinox dates using a simplified astronomical formula.
    // Accurate for years 2000–2050 (±1 day edge cases near century boundaries).
    private static DateOnly Equinox(int year, string which)
    {
        bool vernal = which.Equals("vernal", StringComparison.OrdinalIgnoreCase);
        double baseVal = vernal ? 20.8431 : 23.2488;
        int month = vernal ? 3 : 9;
        int day = (int)(baseVal + 0.242194 * (year - 1980) - Math.Floor((year - 1980) / 4.0));
        return new DateOnly(year, month, day);
    }

    // Anonymous Gregorian algorithm for Easter Sunday
    private static DateOnly Easter(int year)
    {
        int a = year % 19;
        int b = year / 100, c = year % 100;
        int d = b / 4,      e = b % 4;
        int f = (b + 8) / 25;
        int g = (b - f + 1) / 3;
        int h = (19 * a + b - d - g + 15) % 30;
        int i = c / 4,      k = c % 4;
        int l = (32 + 2 * e + 2 * i - h - k) % 7;
        int m = (a + 11 * h + 22 * l) / 451;
        int month = (h + l - 7 * m + 114) / 31;
        int day   = (h + l - 7 * m + 114) % 31 + 1;
        return new DateOnly(year, month, day);
    }

    private static DayOfWeek ParseDay(string s) => s.ToUpperInvariant() switch
    {
        "SUNDAY"    => DayOfWeek.Sunday,
        "MONDAY"    => DayOfWeek.Monday,
        "TUESDAY"   => DayOfWeek.Tuesday,
        "WEDNESDAY" => DayOfWeek.Wednesday,
        "THURSDAY"  => DayOfWeek.Thursday,
        "FRIDAY"    => DayOfWeek.Friday,
        "SATURDAY"  => DayOfWeek.Saturday,
        _ => throw new ArgumentException($"Unknown day-of-week: '{s}'", nameof(s))
    };

    private static Holiday Make(string name, DateOnly date, string region)
        => new() { Name = name, Date = date, Region = region, IsNational = true };
}
