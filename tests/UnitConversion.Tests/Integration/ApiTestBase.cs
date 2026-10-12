using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace UnitConversion.Tests.Integration;

/// <summary>
/// Starts the real API in memory. Each test class gets its own app instance,
/// so units added by one class never leak into another.
/// </summary>
public abstract class ApiTestBase : IClassFixture<WebApplicationFactory<Program>>
{
    protected ApiTestBase(WebApplicationFactory<Program> factory)
    {
        Client = factory.CreateClient();
    }

    protected HttpClient Client { get; }

    protected Task<HttpResponseMessage> Convert(string category, string from, string to, double value) =>
        Client.PostAsJsonAsync("/api/v1/conversions", new { category, fromUnit = from, toUnit = to, value });

    /// <summary>
    /// A short, unique, route-safe name so tests that add data cannot clash with each other.
    /// </summary>
    protected static string Unique(string prefix) => $"{prefix}{Guid.NewGuid():N}"[..14];
}

public sealed record ConversionResult(string Category, string FromUnit, string ToUnit, double InputValue, double Result);

public sealed record CategoryItem(string Name, int UnitCount);

public sealed record UnitItem(string Name, string Symbol, double ToBaseFactor, double ToBaseOffset, string[] Aliases);

public sealed record ProblemResponse(string? Title, int? Status, string? Detail);
