using UnitConversion.Domain.Entities;
using UnitConversion.Domain.Exceptions;

namespace UnitConversion.Tests;

public class UnitCategoryTests
{
    [Fact]
    public void Constructor_DuplicateIdentifier_Throws()
    {
        var units = new[]
        {
            new MeasurementUnit("meter", "m", 1),
            new MeasurementUnit("minute", "m", 60)
        };

        Assert.Throws<DuplicateUnitException>(() => new UnitCategory("broken", units));
    }

    [Fact]
    public void Constructor_BlankName_Throws()
    {
        Assert.Throws<InvalidDefinitionException>(() => new UnitCategory("  ", []));
    }

    [Fact]
    public void FindUnit_MatchesNameSymbolAndAliasIgnoringCase()
    {
        var category = new UnitCategory("length", [new MeasurementUnit("foot", "ft", 0.3048, 0, ["feet"])]);

        Assert.NotNull(category.FindUnit("FOOT"));
        Assert.NotNull(category.FindUnit(" ft "));
        Assert.NotNull(category.FindUnit("Feet"));
        Assert.Null(category.FindUnit("meter"));
    }

    [Fact]
    public void AddUnit_NewUnit_CanBeFound()
    {
        var category = new UnitCategory("length", [new MeasurementUnit("meter", "m", 1)]);

        category.AddUnit(new MeasurementUnit("furlong", "fur", 201.168));

        Assert.Equal(2, category.Units.Count);
        Assert.NotNull(category.FindUnit("furlong"));
    }

    [Fact]
    public void AddUnit_DuplicateAlias_ThrowsAndAddsNothing()
    {
        var category = new UnitCategory("length", [new MeasurementUnit("meter", "m", 1, 0, ["metre"])]);
        var clashing = new MeasurementUnit("furlong", "fur", 201.168, 0, ["metre"]);

        Assert.Throws<DuplicateUnitException>(() => category.AddUnit(clashing));

        Assert.Single(category.Units);
        Assert.Null(category.FindUnit("furlong"));
        Assert.Null(category.FindUnit("fur"));
    }

    [Fact]
    public void Convert_UsesFactorAndOffset()
    {
        var celsius = new MeasurementUnit("celsius", "C", 1);
        var fahrenheit = new MeasurementUnit("fahrenheit", "F", 5.0 / 9.0, -160.0 / 9.0);
        var category = new UnitCategory("temperature", [celsius, fahrenheit]);

        Assert.Equal(212, category.Convert(100, celsius, fahrenheit), precision: 9);
        Assert.Equal(100, category.Convert(212, fahrenheit, celsius), precision: 9);
    }

    [Theory]
    [InlineData("", "m", 1)]
    [InlineData("meter", " ", 1)]
    [InlineData("meter", "m", 0)]
    [InlineData("meter", "m", double.NaN)]
    [InlineData("meter", "m", double.PositiveInfinity)]
    public void MeasurementUnit_InvalidDefinition_Throws(string name, string symbol, double factor)
    {
        Assert.Throws<InvalidDefinitionException>(() => new MeasurementUnit(name, symbol, factor));
    }
}
