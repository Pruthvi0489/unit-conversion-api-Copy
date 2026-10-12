using UnitConversion.Domain.Entities;

namespace UnitConversion.Application.Abstractions;

public interface ICategoryRepository
{
    IReadOnlyCollection<UnitCategory> GetAll();

    UnitCategory? FindByName(string name);

    /// <returns><c>false</c> if a category with the same name already exists.</returns>
    bool TryAdd(UnitCategory category);
}
