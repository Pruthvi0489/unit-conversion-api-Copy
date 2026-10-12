using UnitConversion.Domain.Entities;

namespace UnitConversion.Infrastructure.Repositories;

/// <summary>
/// The built-in units. To add a unit, add one line to the right category;
/// to add a category, add another method and list it in <see cref="Create"/>.
/// </summary>
internal static class UnitCatalogSeed
{
    public static IReadOnlyList<UnitCategory> Create() =>
    [
        Length(),
        Mass(),
        Temperature(),
        Volume()
    ];

    // Base unit: meter
    private static UnitCategory Length() => new("length",
    [
        Linear("meter", "m", 1, "meters", "metre", "metres"),
        Linear("kilometer", "km", 1000, "kilometers", "kilometre", "kilometres"),
        Linear("centimeter", "cm", 0.01, "centimeters", "centimetre", "centimetres"),
        Linear("millimeter", "mm", 0.001, "millimeters", "millimetre", "millimetres"),
        Linear("mile", "mi", 1609.344, "miles"),
        Linear("yard", "yd", 0.9144, "yards"),
        Linear("foot", "ft", 0.3048, "feet"),
        Linear("inch", "in", 0.0254, "inches")
    ]);

    // Base unit: kilogram
    private static UnitCategory Mass() => new("mass",
    [
        Linear("kilogram", "kg", 1, "kilograms", "kilo", "kilos"),
        Linear("gram", "g", 0.001, "grams"),
        Linear("milligram", "mg", 0.000001, "milligrams"),
        Linear("tonne", "t", 1000, "tonnes", "metric ton"),
        Linear("pound", "lb", 0.45359237, "pounds", "lbs"),
        Linear("ounce", "oz", 0.028349523125, "ounces"),
        Linear("stone", "st", 6.35029318, "stones")
    ]);

    // Base unit: celsius
    private static UnitCategory Temperature() => new("temperature",
    [
        new MeasurementUnit("celsius", "C", 1, 0, ["centigrade"]),
        new MeasurementUnit("fahrenheit", "F", 5.0 / 9.0, -160.0 / 9.0),
        new MeasurementUnit("kelvin", "K", 1, -273.15)
    ]);

    // Base unit: liter (US customary units)
    private static UnitCategory Volume() => new("volume",
    [
        Linear("liter", "l", 1, "liters", "litre", "litres"),
        Linear("milliliter", "ml", 0.001, "milliliters", "millilitre", "millilitres"),
        Linear("gallon", "gal", 3.785411784, "gallons"),
        Linear("quart", "qt", 0.946352946, "quarts"),
        Linear("cup", "cup", 0.2365882365, "cups"),
        Linear("fluid ounce", "fl oz", 0.0295735295625, "fluid ounces")
    ]);

    private static MeasurementUnit Linear(string name, string symbol, double factor, params string[] aliases) =>
        new(name, symbol, factor, 0, aliases);
}
