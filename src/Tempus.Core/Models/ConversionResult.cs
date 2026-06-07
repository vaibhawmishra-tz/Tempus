using System.Runtime.InteropServices;

namespace Tempus.Core.Models;

/// <summary>
/// The result of converting a local <see cref="DateTime"/> to UTC, including any DST ambiguity metadata.
/// </summary>
[StructLayout(LayoutKind.Auto)]
public readonly struct ConversionResult : IEquatable<ConversionResult>
{
    /// <summary>The resolved UTC value.</summary>
    public DateTimeOffset Value { get; init; }

    /// <summary>Whether the original local time fell in a DST overlap (two valid UTC interpretations).</summary>
    public bool WasAmbiguous { get; init; }

    /// <summary>Whether the original local time fell in a DST gap (no valid local interpretation).</summary>
    public bool WasInvalid { get; init; }

    /// <summary>The name of the DST strategy that was applied to resolve ambiguity or invalidity.</summary>
    public string StrategyApplied { get; init; }

    /// <summary>Both candidate UTC values when the local time was ambiguous; <c>null</c> otherwise.</summary>
    public IReadOnlyList<DateTimeOffset>? Candidates { get; init; }

    /// <summary>Creates a <see cref="ConversionResult"/> for an unambiguous, valid conversion.</summary>
    public static ConversionResult Clean(DateTimeOffset value) =>
        new() { Value = value, StrategyApplied = "None" };

    public bool Equals(ConversionResult other) =>
        Value == other.Value && WasAmbiguous == other.WasAmbiguous && WasInvalid == other.WasInvalid;

    public override bool Equals(object? obj) => obj is ConversionResult other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(Value, WasAmbiguous, WasInvalid);
    public static bool operator ==(ConversionResult left, ConversionResult right) => left.Equals(right);
    public static bool operator !=(ConversionResult left, ConversionResult right) => !left.Equals(right);
}
