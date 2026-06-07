namespace Tempus.Business.Abstractions;

/// <summary>
/// Creates <see cref="ISlaTimer"/> instances bound to the calendar's business-hour rules.
/// </summary>
public interface ISlaTimerFactory
{
    /// <summary>Creates an SLA timer that started at <paramref name="startedAt"/> with the given <paramref name="target"/> business-time budget.</summary>
    ISlaTimer Create(DateTimeOffset startedAt, TimeSpan target);

    /// <summary>Creates an SLA timer starting at the current instant with the given <paramref name="target"/> business-time budget.</summary>
    ISlaTimer CreateNow(TimeSpan target);
}
