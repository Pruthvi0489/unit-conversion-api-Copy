namespace UnitConversion.Domain.Exceptions;

/// <summary>
/// Thrown when a unit or category is defined with values that cannot work.
/// </summary>
public sealed class InvalidDefinitionException : DomainException
{
    public InvalidDefinitionException(string message) : base(message)
    {
    }
}
