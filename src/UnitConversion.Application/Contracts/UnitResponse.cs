namespace UnitConversion.Application.Contracts;

public sealed record UnitResponse(
    string Name,
    string Symbol,
    double ToBaseFactor,
    double ToBaseOffset,
    IReadOnlyList<string> Aliases);
