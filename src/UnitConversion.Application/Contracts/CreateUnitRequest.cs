using System.ComponentModel.DataAnnotations;

namespace UnitConversion.Application.Contracts;

public sealed class CreateUnitRequest
{
    /// <example>furlong</example>
    [Required]
    public required string Name { get; init; }

    /// <example>fur</example>
    [Required]
    public required string Symbol { get; init; }

    /// <summary>
    /// How many base units one of this unit is worth. For length the base unit is the meter,
    /// so a furlong (201.168 m) has a factor of 201.168.
    /// </summary>
    /// <example>201.168</example>
    public required double ToBaseFactor { get; init; }

    /// <summary>
    /// Added after applying the factor. Only needed for scales that do not share a zero, like temperature.
    /// </summary>
    /// <example>0</example>
    public double ToBaseOffset { get; init; }

    /// <example>["furlongs"]</example>
    public List<string>? Aliases { get; init; }
}
