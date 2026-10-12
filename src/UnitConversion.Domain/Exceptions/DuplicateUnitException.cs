namespace UnitConversion.Domain.Exceptions;

public sealed class DuplicateUnitException : DomainException
{
    public DuplicateUnitException(string identifier, string category)
        : base($"'{identifier}' is already used by another unit in the '{category}' category.")
    {
    }
}
