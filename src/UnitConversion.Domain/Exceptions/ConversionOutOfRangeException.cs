namespace UnitConversion.Domain.Exceptions;

public sealed class ConversionOutOfRangeException : DomainException
{
    public ConversionOutOfRangeException()
        : base("The result is too large to be represented.")
    {
    }
}
