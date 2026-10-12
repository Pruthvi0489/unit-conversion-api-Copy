using System.ComponentModel.DataAnnotations;

namespace UnitConversion.Application.Contracts;

public sealed class CreateCategoryRequest
{
    /// <example>speed</example>
    [Required]
    [RegularExpression(@"^[A-Za-z0-9 _-]+$", ErrorMessage = "Use only letters, digits, spaces, hyphens and underscores.")]
    public required string Name { get; init; }

    /// <summary>
    /// The units of the category. Factors are relative to one base unit, which should have a factor of 1.
    /// </summary>
    [Required]
    [MinLength(1)]
    public required List<CreateUnitRequest> Units { get; init; }
}
