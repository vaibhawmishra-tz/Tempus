namespace Tempus.Core.Configuration;

public sealed class DstOptions
{
    public AmbiguousTimeStrategy OnAmbiguousTime { get; set; } = AmbiguousTimeStrategy.PreferStandard;
    public InvalidTimeStrategy OnInvalidTime { get; set; } = InvalidTimeStrategy.AdjustForward;

    /// <summary>
    /// Throws on any DST ambiguity regardless of strategy — for financial or legal apps.
    /// </summary>
    public bool StrictMode { get; set; }
}
