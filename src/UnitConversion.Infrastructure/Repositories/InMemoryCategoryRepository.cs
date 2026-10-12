using System.Collections.Concurrent;
using UnitConversion.Application.Abstractions;
using UnitConversion.Domain.Entities;

namespace UnitConversion.Infrastructure.Repositories;

/// <summary>
/// Keeps the catalog in memory, starting from the built-in units. Categories and units added
/// at runtime live only as long as the process does. A database-backed implementation of
/// <see cref="ICategoryRepository"/> can replace this one without touching any other layer.
/// </summary>
public sealed class InMemoryCategoryRepository : ICategoryRepository
{
    private readonly ConcurrentDictionary<string, UnitCategory> _categories =
        new(StringComparer.OrdinalIgnoreCase);

    public InMemoryCategoryRepository()
    {
        foreach (var category in UnitCatalogSeed.Create())
        {
            _categories[category.Name] = category;
        }
    }

    public IReadOnlyCollection<UnitCategory> GetAll() => _categories.Values.ToList();

    public UnitCategory? FindByName(string name) =>
        _categories.TryGetValue(name.Trim(), out var category) ? category : null;

    public bool TryAdd(UnitCategory category) => _categories.TryAdd(category.Name, category);
}
