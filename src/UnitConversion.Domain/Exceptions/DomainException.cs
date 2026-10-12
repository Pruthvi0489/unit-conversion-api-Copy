namespace UnitConversion.Domain.Exceptions;

/// <summary>
/// Base type for errors caused by an invalid conversion request, as opposed to a bug.
/// </summary>
public abstract class DomainException : Exception
{
    protected DomainException(string message) : base(message)
    {
    }
}
