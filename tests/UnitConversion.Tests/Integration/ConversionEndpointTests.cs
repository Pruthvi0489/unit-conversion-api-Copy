using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace UnitConversion.Tests.Integration;

public class ConversionEndpointTests : ApiTestBase
{
    public ConversionEndpointTests(WebApplicationFactory<Program> factory) : base(factory)
    {
    }

    [Fact]
    public async Task Convert_MetersToFeet_ReturnsOk()
    {
        var response = await Convert("length", "meters", "feet", 100);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<ConversionResult>();
        Assert.NotNull(body);
        Assert.Equal("meter", body.FromUnit);
        Assert.Equal("foot", body.ToUnit);
        Assert.Equal(328.0839895, body.Result, precision: 6);
    }

    [Fact]
    public async Task Convert_CelsiusToFahrenheit_ReturnsCleanValue()
    {
        var response = await Convert("temperature", "celsius", "fahrenheit", 100);

        var body = await response.Content.ReadFromJsonAsync<ConversionResult>();
        Assert.Equal(212, body!.Result);
    }

    [Fact]
    public async Task Convert_FreezingPoint_ReturnsExactlyZero()
    {
        var response = await Convert("temperature", "F", "C", 32);

        var body = await response.Content.ReadFromJsonAsync<ConversionResult>();
        Assert.Equal(0, body!.Result);
    }

    [Fact]
    public async Task Convert_KilogramsToPounds_ReturnsOk()
    {
        var response = await Convert("mass", "kg", "lb", 10);

        var body = await response.Content.ReadFromJsonAsync<ConversionResult>();
        Assert.Equal(22.04622622, body!.Result, precision: 6);
    }

    [Fact]
    public async Task Convert_UnknownUnit_ReturnsBadRequestWithProblemDetails()
    {
        var response = await Convert("length", "meters", "parsecs", 1);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ProblemResponse>();
        Assert.Equal(400, problem!.Status);
        Assert.Contains("parsecs", problem.Detail);
    }

    [Fact]
    public async Task Convert_UnknownCategory_ReturnsBadRequest()
    {
        var response = await Convert("speed-of-light", "m", "ft", 1);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Convert_UnitFromDifferentCategory_ReturnsBadRequest()
    {
        var response = await Convert("length", "meters", "kilograms", 1);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Convert_MissingFields_ReturnsBadRequest()
    {
        var response = await Client.PostAsJsonAsync("/api/v1/conversions", new { category = "length" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Convert_MissingValue_ReturnsBadRequest()
    {
        var response = await Client.PostAsJsonAsync(
            "/api/v1/conversions",
            new { category = "length", fromUnit = "m", toUnit = "ft" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Convert_ValueIsNotANumber_ReturnsBadRequest()
    {
        var response = await Client.PostAsJsonAsync(
            "/api/v1/conversions",
            new { category = "length", fromUnit = "m", toUnit = "ft", value = "abc" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Convert_ResultTooLarge_ReturnsBadRequest()
    {
        var response = await Convert("length", "kilometer", "millimeter", double.MaxValue);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Swagger_DocumentIsServed()
    {
        var response = await Client.GetAsync("/swagger/v1/swagger.json");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("/api/v1/conversions", await response.Content.ReadAsStringAsync());
    }
}
