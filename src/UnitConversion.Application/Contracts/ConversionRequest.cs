using System.ComponentModel.DataAnnotations;

namespace UnitConversion.Application.Contracts;

public sealed class ConversionRequest
{
    /// <example>length</example>
    [Required]
    public required string Category { get; init; }

    /// <example>meters</example>
    [Required]
    public required string FromUnit { get; init; }

    /// <example>feet</example>
    [Required]
    public required string ToUnit { get; init; }

    /// <example>100</example>
    public required double Value { get; init; }
}
