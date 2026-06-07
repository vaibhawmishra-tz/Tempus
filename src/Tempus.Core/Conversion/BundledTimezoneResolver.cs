using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Tempus.Core.Abstractions;
using Tempus.Core.Configuration;

namespace Tempus.Core.Conversion;

[SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Instantiated via DI")]
internal sealed partial class BundledTimezoneResolver : ITimezoneResolver
{
    private readonly TimezoneOptions _options;
    private readonly ILogger<BundledTimezoneResolver> _logger;

    public IReadOnlyList<string> AllIanaIds { get; } = TimeZoneInfo.GetSystemTimeZones()
        .Select(tz => tz.Id)
        .ToList();

    public BundledTimezoneResolver(TimezoneOptions options, ILogger<BundledTimezoneResolver> logger)
    {
        _options = options;
        _logger = logger;
    }

    public TimeZoneInfo Resolve(string timezoneId)
    {
        if (!TryResolve(timezoneId, out TimeZoneInfo? tz) || tz is null)
        {
            if (_options.OnMissingZone == MissingZoneStrategy.Throw)
                throw new TimeZoneNotFoundException($"Timezone '{timezoneId}' could not be resolved.");

            LogFallbackToUtc(_logger, timezoneId);
            return TimeZoneInfo.Utc;
        }
        return tz;
    }

    public bool TryResolve(string timezoneId, out TimeZoneInfo? timeZoneInfo)
    {
        string normalized = NormalizeId(timezoneId);
        try
        {
            timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(normalized);
            return true;
        }
        catch (TimeZoneNotFoundException)
        {
            timeZoneInfo = null;
            return false;
        }
    }

    public string ToIanaId(string timezoneId)
    {
        if (TimeZoneInfo.TryConvertWindowsIdToIanaId(timezoneId, out string? ianaId) && ianaId is not null)
            return ianaId;
        return timezoneId;
    }

    public string ToWindowsId(string ianaId)
    {
        if (TimeZoneInfo.TryConvertIanaIdToWindowsId(ianaId, out string? windowsId) && windowsId is not null)
            return windowsId;
        return ianaId;
    }

    private string NormalizeId(string timezoneId)
    {
        if (!_options.AcceptWindowsIds)
            return timezoneId;

        if (TimeZoneInfo.TryConvertWindowsIdToIanaId(timezoneId, out string? ianaId) && ianaId is not null)
        {
            if (_options.WarnOnWindowsIds)
                LogWindowsIdDetected(_logger, timezoneId, ianaId);
            return ianaId;
        }

        return timezoneId;
    }

    [LoggerMessage(Level = LogLevel.Warning,
        Message = "Timezone '{TimezoneId}' not found — falling back to UTC.")]
    private static partial void LogFallbackToUtc(ILogger logger, string timezoneId);

    [LoggerMessage(Level = LogLevel.Warning,
        Message = "Windows timezone ID '{WindowsId}' detected. Prefer IANA ID '{IanaId}' for cross-platform compatibility.")]
    private static partial void LogWindowsIdDetected(ILogger logger, string windowsId, string ianaId);
}
