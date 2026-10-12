using UnitConversion.Domain.Exceptions;

namespace UnitConversion.Domain.Entities;

/// <summary>
/// A group of units that can be converted into each other, e.g. length or mass.
/// Units can be added after creation, so access to the unit list is synchronised.
/// </summary>
public sealed class UnitCategory
{
    private readonly object _sync = new();
    private readonly List<MeasurementUnit> _units = [];
    private readonly Dictionary<string, MeasurementUnit> _unitsByIdentifier =
        new(StringComparer.OrdinalIgnoreCase);

    public UnitCategory(string name, IEnumerable<MeasurementUnit> units)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new InvalidDefinitionException("A category needs a name.");
        }

        Name = name.Trim();

        foreach (var unit in units)
        {
            AddUnit(unit);
        }
    }

    public string Name { get; }

    public IReadOnlyList<MeasurementUnit> Units
    {
        get
        {
            lock (_sync)
            {
                return _units.ToArray();
            }
        }
    }

    public MeasurementUnit? FindUnit(string identifier)
    {
        lock (_sync)
        {
            return _unitsByIdentifier.TryGetValue(identifier.Trim(), out var unit) ? unit : null;
        }
    }

    /// <summary>
    /// Adds a unit, or throws if its name, symbol or any alias is already taken.
    /// Nothing is added unless every identifier is free.
    /// </summary>
    public void AddUnit(MeasurementUnit unit)
    {
        lock (_sync)
        {
            var identifiers = unit.Identifiers.ToList();

            foreach (var identifier in identifiers)
            {
                if (_unitsByIdentifier.ContainsKey(identifier))
                {
                    throw new DuplicateUnitException(identifier, Name);
                }
            }

            foreach (var identifier in identifiers)
            {
                _unitsByIdentifier[identifier] = unit;
            }

            _units.Add(unit);
        }
    }

    public double Convert(double value, MeasurementUnit from, MeasurementUnit to) =>
        to.FromBase(from.ToBase(value));
}
