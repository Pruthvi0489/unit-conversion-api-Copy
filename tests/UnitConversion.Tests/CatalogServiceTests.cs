using Microsoft.Extensions.Logging.Abstractions;
using UnitConversion.Application.Contracts;
using UnitConversion.Application.Services;
using UnitConversion.Domain.Exceptions;
using UnitConversion.Infrastructure.Repositories;

namespace UnitConversion.Tests;

public class CatalogServiceTests
{
    private readonly InMemoryCategoryRepository _repository = new();
    private readonly CatalogService _catalog;
    private readonly ConversionService _conversions;

    public CatalogServiceTests()
    {
        _catalog = new CatalogService(_repository);
        _conversions = new ConversionService(_repository, NullLogger<ConversionService>.Instance);
    }

    [Fact]
    public void GetCategories_IncludesRequiredCategories()
    {
        var names = _catalog.GetCategories().Select(c => c.Name).ToList();

        Assert.Contains("length", names);
        Assert.Contains("temperature", names);
        Assert.Contains("mass", names);
    }

    [Fact]
    public void GetCategories_ReportsUnitCounts()
    {
        var length = _catalog.GetCategories().Single(c => c.Name == "length");

        Assert.Equal(8, length.UnitCount);
    }

    [Fact]
    public void GetUnits_KnownCategory_ReturnsUnits()
    {
        var units = _catalog.GetUnits("Length");

        Assert.NotNull(units);
        Assert.Contains(units, u => u.Name == "meter" && u.Symbol == "m");
    }

    [Fact]
    public void GetUnits_UnknownCategory_ReturnsNull()
    {
        Assert.Null(_catalog.GetUnits("nonsense"));
    }

    [Fact]
    public void AddUnit_MakesUnitAvailableForConversion()
    {
        var added = _catalog.AddUnit("length", NewUnit("furlong", "fur", 201.168));

        Assert.NotNull(added);

        var result = _conversions.Convert(new ConversionRequest
        {
            Category = "length",
            FromUnit = "furlong",
            ToUnit = "meter",
            Value = 2
        });

        Assert.Equal(402.336, result.Result, precision: 6);
        Assert.Equal(9, _catalog.GetCategories().Single(c => c.Name == "length").UnitCount);
    }

    [Fact]
    public void AddUnit_UnknownCategory_ReturnsNull()
    {
        Assert.Null(_catalog.AddUnit("nonsense", NewUnit("furlong", "fur", 201.168)));
    }

    [Fact]
    public void AddUnit_ExistingSymbol_Throws()
    {
        Assert.Throws<DuplicateUnitException>(
            () => _catalog.AddUnit("length", NewUnit("minute", "m", 60)));
    }

    [Fact]
    public void AddUnit_ZeroFactor_Throws()
    {
        Assert.Throws<InvalidDefinitionException>(
            () => _catalog.AddUnit("length", NewUnit("nothing", "nil", 0)));
    }

    [Fact]
    public void AddCategory_NewCategory_CanBeUsedForConversion()
    {
        var created = _catalog.AddCategory(new CreateCategoryRequest
        {
            Name = "speed",
            Units =
            [
                NewUnit("meter per second", "mps", 1),
                NewUnit("kilometer per hour", "kph", 1000.0 / 3600.0)
            ]
        });

        Assert.Equal("speed", created.Name);
        Assert.Equal(2, created.UnitCount);

        var result = _conversions.Convert(new ConversionRequest
        {
            Category = "speed",
            FromUnit = "kph",
            ToUnit = "mps",
            Value = 36
        });

        Assert.Equal(10, result.Result, precision: 6);
    }

    [Fact]
    public void AddCategory_ExistingName_Throws()
    {
        var request = new CreateCategoryRequest { Name = "LENGTH", Units = [NewUnit("x", "x", 1)] };

        Assert.Throws<DuplicateCategoryException>(() => _catalog.AddCategory(request));
    }

    private static CreateUnitRequest NewUnit(string name, string symbol, double factor) => new()
    {
        Name = name,
        Symbol = symbol,
        ToBaseFactor = factor
    };
}
