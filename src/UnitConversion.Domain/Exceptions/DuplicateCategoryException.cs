namespace UnitConversion.Domain.Exceptions;

public sealed class DuplicateCategoryException : DomainException
{
    public DuplicateCategoryException(string category)
        : base($"The category '{category}' already exists.")
    {
    }
}
