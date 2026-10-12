using UnitConversion.Domain.Exceptions;

namespace UnitConversion.Domain.Entities;

/// <summary>
/// A single unit of measurement, described relative to the base unit of its category.
/// </summary>
/// <remarks>
/// Every unit is expressed as <c>baseValue = value * ToBaseFactor + ToBaseOffset</c>.
/// Most units only need a factor. The offset exists for scales like temperature,
/// where zero is not at the same place on every scale.
/// </remarks>
public sealed class MeasurementUnit
{
    public MeasurementUnit(
        string name,
        string symbol,
        double toBaseFactor,
        double toBaseOffset = 0,
        IEnumerable<string>? aliases = null)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new InvalidDefinitionException("A unit needs a name.");
        }

        if (string.IsNullOrWhiteSpace(symbol))
        {
            throw new InvalidDefinitionException("A unit needs a symbol.");
        }

        if (toBaseFactor == 0 || !double.IsFinite(toBaseFactor))
        {
            throw new InvalidDefinitionException("The conversion factor must be a non-zero, finite number.");
        }

        if (!double.IsFinite(toBaseOffset))
        {
            throw new InvalidDefinitionException("The conversion offset must be a finite number.");
        }

        Name = name.Trim();
        Symbol = symbol.Trim();
        ToBaseFactor = toBaseFactor;
        ToBaseOffset = toBaseOffset;
        Aliases = (aliases ?? []).Select(a => a.Trim()).Where(a => a.Length > 0).ToList();
    }

    public string Name { get; }

    public string Symbol { get; }

    public double ToBaseFactor { get; }

    public double ToBaseOffset { get; }

    public IReadOnlyList<string> Aliases { get; }

    /// <summary>
    /// Every string that should resolve to this unit (name, symbol and aliases).
    /// </summary>
    public IEnumerable<string> Identifiers =>
        Aliases.Prepend(Symbol).Prepend(Name).Distinct(StringComparer.OrdinalIgnoreCase);

    public double ToBase(double value) => value * ToBaseFactor + ToBaseOffset;

    public double FromBase(double baseValue) => (baseValue - ToBaseOffset) / ToBaseFactor;
}
