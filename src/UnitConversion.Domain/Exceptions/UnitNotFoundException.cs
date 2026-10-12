namespace UnitConversion.Domain.Exceptions;

public sealed class UnitNotFoundException : DomainException
{
    public UnitNotFoundException(string unit, string category)
        : base($"The unit '{unit}' is not supported in the '{category}' category.")
    {
    }
}
