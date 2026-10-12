using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace UnitConversion.Tests.Integration;

public class CatalogEndpointTests : ApiTestBase
{
    public CatalogEndpointTests(WebApplicationFactory<Program> factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetCategories_ReturnsBuiltInCategories()
    {
        var categories = await Client.GetFromJsonAsync<List<CategoryItem>>("/api/v1/categories");

        Assert.NotNull(categories);
        Assert.Contains(categories, c => c.Name == "length");
        Assert.Contains(categories, c => c.Name == "temperature");
        Assert.Contains(categories, c => c.Name == "mass");
        Assert.All(categories, c => Assert.True(c.UnitCount > 0));
    }

    [Fact]
    public async Task GetUnits_KnownCategory_ReturnsUnits()
    {
        var units = await Client.GetFromJsonAsync<List<UnitItem>>("/api/v1/categories/length/units");

        Assert.NotNull(units);
        Assert.Contains(units, u => u.Name == "foot" && u.Aliases.Contains("feet"));
    }

    [Fact]
    public async Task GetUnits_UnknownCategory_ReturnsNotFound()
    {
        var response = await Client.GetAsync("/api/v1/categories/nonsense/units");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task AddUnit_ThenConvert_UsesTheNewUnit()
    {
        var name = Unique("furlong");
        var symbol = Unique("f");

        var created = await Client.PostAsJsonAsync("/api/v1/categories/length/units", new
        {
            name,
            symbol,
            toBaseFactor = 201.168,
            aliases = new[] { Unique("alias") }
        });

        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        Assert.NotNull(created.Headers.Location);

        var response = await Convert("length", name, "meter", 1);
        var body = await response.Content.ReadFromJsonAsync<ConversionResult>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(201.168, body!.Result, precision: 6);

        var units = await Client.GetFromJsonAsync<List<UnitItem>>("/api/v1/categories/length/units");
        Assert.Contains(units!, u => u.Name == name);
    }

    [Fact]
    public async Task AddUnit_WithOffset_ConvertsCorrectly()
    {
        // Rankine: same size as Fahrenheit but zero sits at absolute zero.
        var name = Unique("rankine");

        var created = await Client.PostAsJsonAsync("/api/v1/categories/temperature/units", new
        {
            name,
            symbol = Unique("r"),
            toBaseFactor = 5.0 / 9.0,
            toBaseOffset = -273.15
        });

        Assert.Equal(HttpStatusCode.Created, created.StatusCode);

        var response = await Convert("temperature", name, "kelvin", 491.67);
        var body = await response.Content.ReadFromJsonAsync<ConversionResult>();

        Assert.Equal(273.15, body!.Result, precision: 6);
    }

    [Fact]
    public async Task AddUnit_DuplicateSymbol_ReturnsConflict()
    {
        var response = await Client.PostAsJsonAsync("/api/v1/categories/length/units", new
        {
            name = Unique("other"),
            symbol = "m",
            toBaseFactor = 1
        });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ProblemResponse>();
        Assert.Equal(409, problem!.Status);
    }

    [Fact]
    public async Task AddUnit_UnknownCategory_ReturnsNotFound()
    {
        var response = await Client.PostAsJsonAsync("/api/v1/categories/nonsense/units", new
        {
            name = Unique("unit"),
            symbol = Unique("u"),
            toBaseFactor = 1
        });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task AddUnit_ZeroFactor_ReturnsBadRequest()
    {
        var response = await Client.PostAsJsonAsync("/api/v1/categories/length/units", new
        {
            name = Unique("unit"),
            symbol = Unique("u"),
            toBaseFactor = 0
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task AddUnit_MissingName_ReturnsBadRequest()
    {
        var response = await Client.PostAsJsonAsync("/api/v1/categories/length/units", new
        {
            symbol = Unique("u"),
            toBaseFactor = 1
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateCategory_ThenConvert_Works()
    {
        var category = Unique("speed");

        var created = await Client.PostAsJsonAsync("/api/v1/categories", new
        {
            name = category,
            units = new[]
            {
                new { name = "meter per second", symbol = "mps", toBaseFactor = 1.0 },
                new { name = "kilometer per hour", symbol = "kph", toBaseFactor = 1000.0 / 3600.0 }
            }
        });

        Assert.Equal(HttpStatusCode.Created, created.StatusCode);

        var response = await Convert(category, "kph", "mps", 36);
        var body = await response.Content.ReadFromJsonAsync<ConversionResult>();

        Assert.Equal(10, body!.Result, precision: 6);

        var categories = await Client.GetFromJsonAsync<List<CategoryItem>>("/api/v1/categories");
        Assert.Contains(categories!, c => c.Name == category && c.UnitCount == 2);
    }

    [Fact]
    public async Task CreateCategory_ExistingName_ReturnsConflict()
    {
        var response = await Client.PostAsJsonAsync("/api/v1/categories", new
        {
            name = "length",
            units = new[] { new { name = "x", symbol = "x", toBaseFactor = 1.0 } }
        });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task CreateCategory_WithoutUnits_ReturnsBadRequest()
    {
        var response = await Client.PostAsJsonAsync("/api/v1/categories", new
        {
            name = Unique("empty"),
            units = Array.Empty<object>()
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateCategory_InvalidName_ReturnsBadRequest()
    {
        var response = await Client.PostAsJsonAsync("/api/v1/categories", new
        {
            name = "bad/name",
            units = new[] { new { name = "x", symbol = "x", toBaseFactor = 1.0 } }
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
