using Microsoft.Extensions.Logging.Abstractions;
using UnitConversion.Application.Contracts;
using UnitConversion.Application.Services;
using UnitConversion.Domain.Exceptions;
using UnitConversion.Infrastructure.Repositories;

namespace UnitConversion.Tests;

public class ConversionServiceTests
{
    private readonly ConversionService _service =
        new(new InMemoryCategoryRepository(), NullLogger<ConversionService>.Instance);

    [Theory]
    [InlineData("length", "meters", "feet", 1, 3.280839895)]
    [InlineData("length", "mile", "km", 1, 1.609344)]
    [InlineData("length", "inch", "centimeter", 10, 25.4)]
    [InlineData("mass", "kg", "lb", 1, 2.204622622)]
    [InlineData("mass", "pound", "ounce", 1, 16)]
    [InlineData("volume", "gallon", "liter", 1, 3.785411784)]
    public void Convert_LinearUnits_ReturnsExpectedValue(
        string category, string from, string to, double value, double expected)
    {
        var response = _service.Convert(Request(category, from, to, value));

        Assert.Equal(expected, response.Result, precision: 6);
    }

    [Theory]
    [InlineData("celsius", "fahrenheit", 100, 212)]
    [InlineData("fahrenheit", "celsius", 32, 0)]
    [InlineData("celsius", "kelvin", 0, 273.15)]
    [InlineData("kelvin", "fahrenheit", 0, -459.67)]
    [InlineData("F", "C", -40, -40)]
    public void Convert_Temperature_HandlesOffsets(string from, string to, double value, double expected)
    {
        var response = _service.Convert(Request("temperature", from, to, value));

        Assert.Equal(expected, response.Result, precision: 6);
    }

    [Fact]
    public void Convert_IgnoresCaseAndWhitespace()
    {
        var response = _service.Convert(Request(" Length ", " METERS ", "Feet", 1));

        Assert.Equal("length", response.Category);
        Assert.Equal("meter", response.FromUnit);
        Assert.Equal("foot", response.ToUnit);
    }

    [Fact]
    public void Convert_SameUnit_ReturnsInputValue()
    {
        var response = _service.Convert(Request("mass", "gram", "gram", 42.5));

        Assert.Equal(42.5, response.Result, precision: 9);
    }

    [Fact]
    public void Convert_UnknownCategory_Throws()
    {
        Assert.Throws<CategoryNotFoundException>(
            () => _service.Convert(Request("speed", "mph", "kph", 1)));
    }

    [Fact]
    public void Convert_UnknownUnit_Throws()
    {
        Assert.Throws<UnitNotFoundException>(
            () => _service.Convert(Request("length", "meters", "parsecs", 1)));
    }

    [Fact]
    public void Convert_UnitFromAnotherCategory_Throws()
    {
        Assert.Throws<UnitNotFoundException>(
            () => _service.Convert(Request("length", "meters", "kilograms", 1)));
    }

    [Fact]
    public void Convert_ResultTooLarge_Throws()
    {
        Assert.Throws<ConversionOutOfRangeException>(
            () => _service.Convert(Request("length", "kilometer", "millimeter", double.MaxValue)));
    }

    private static ConversionRequest Request(string category, string from, string to, double value) => new()
    {
        Category = category,
        FromUnit = from,
        ToUnit = to,
        Value = value
    };
}
