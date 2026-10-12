namespace UnitConversion.Domain.Exceptions;

public sealed class CategoryNotFoundException : DomainException
{
    public CategoryNotFoundException(string category)
        : base($"The category '{category}' is not supported.")
    {
    }
}
