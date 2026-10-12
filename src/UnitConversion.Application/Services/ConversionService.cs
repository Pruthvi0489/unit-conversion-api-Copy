using Microsoft.Extensions.Logging;
using UnitConversion.Application.Abstractions;
using UnitConversion.Application.Contracts;
using UnitConversion.Domain.Entities;
using UnitConversion.Domain.Exceptions;

namespace UnitConversion.Application.Services;

public sealed class ConversionService : IConversionService
{
    // Double arithmetic leaves noise like 212.00000000000003, so results are
    // trimmed to a fixed number of significant digits before they leave the service.
    private const int SignificantDigits = 12;

    private readonly ICategoryRepository _categories;
    private readonly ILogger<ConversionService> _logger;

    public ConversionService(ICategoryRepository categories, ILogger<ConversionService> logger)
    {
        _categories = categories;
        _logger = logger;
    }

    public ConversionResponse Convert(ConversionRequest request)
    {
        var category = _categories.FindByName(request.Category)
            ?? throw new CategoryNotFoundException(request.Category);

        var from = category.FindUnit(request.FromUnit)
            ?? throw new UnitNotFoundException(request.FromUnit, category.Name);

        var to = category.FindUnit(request.ToUnit)
            ?? throw new UnitNotFoundException(request.ToUnit, category.Name);

        var result = category.Convert(request.Value, from, to);

        if (!double.IsFinite(result))
        {
            throw new ConversionOutOfRangeException();
        }

        if (UsesOffset(from, to))
        {
            result = SnapNearZero(result, request.Value);
        }

        _logger.LogDebug(
            "Converted {Value} {From} to {To} in category {Category}",
            request.Value, from.Name, to.Name, category.Name);

        return new ConversionResponse(
            category.Name,
            from.Name,
            to.Name,
            request.Value,
            RoundToSignificantDigits(result));
    }

    private static bool UsesOffset(MeasurementUnit from, MeasurementUnit to) =>
        from.ToBaseOffset != 0 || to.ToBaseOffset != 0;

    // Offset scales can leave a residue like 3.5e-15 where the answer is really 0
    // (32 F to C), and significant-digit rounding alone won't clean that up.
    private static double SnapNearZero(double result, double input) =>
        Math.Abs(result) < 1e-12 * Math.Max(1, Math.Abs(input)) ? 0 : result;

    private static double RoundToSignificantDigits(double value)
    {
        if (value == 0)
        {
            return 0;
        }

        var magnitude = Math.Floor(Math.Log10(Math.Abs(value)));
        var scale = Math.Pow(10, SignificantDigits - 1 - magnitude);

        // Extremely small or large values would overflow the scale, so leave them alone.
        return double.IsFinite(scale) && scale != 0
            ? Math.Round(value * scale) / scale
            : value;
    }
}
