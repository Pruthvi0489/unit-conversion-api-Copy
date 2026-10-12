namespace UnitConversion.Application.Contracts;

public sealed record ConversionResponse(
    string Category,
    string FromUnit,
    string ToUnit,
    double InputValue,
    double Result);
