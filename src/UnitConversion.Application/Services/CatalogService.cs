using UnitConversion.Application.Abstractions;
using UnitConversion.Application.Contracts;
using UnitConversion.Domain.Entities;
using UnitConversion.Domain.Exceptions;

namespace UnitConversion.Application.Services;

public sealed class CatalogService : ICatalogService
{
    private readonly ICategoryRepository _categories;

    public CatalogService(ICategoryRepository categories)
    {
        _categories = categories;
    }

    public IReadOnlyList<CategoryResponse> GetCategories() =>
        _categories.GetAll()
            .OrderBy(c => c.Name)
            .Select(c => new CategoryResponse(c.Name, c.Units.Count))
            .ToList();

    public IReadOnlyList<UnitResponse>? GetUnits(string category)
    {
        var found = _categories.FindByName(category);

        return found?.Units.Select(ToResponse).ToList();
    }

    public CategoryResponse AddCategory(CreateCategoryRequest request)
    {
        var category = new UnitCategory(request.Name, request.Units.Select(ToDomain));

        if (!_categories.TryAdd(category))
        {
            throw new DuplicateCategoryException(category.Name);
        }

        return new CategoryResponse(category.Name, category.Units.Count);
    }

    public UnitResponse? AddUnit(string category, CreateUnitRequest request)
    {
        var found = _categories.FindByName(category);

        if (found is null)
        {
            return null;
        }

        var unit = ToDomain(request);
        found.AddUnit(unit);

        return ToResponse(unit);
    }

    private static MeasurementUnit ToDomain(CreateUnitRequest request) =>
        new(request.Name, request.Symbol, request.ToBaseFactor, request.ToBaseOffset, request.Aliases);

    private static UnitResponse ToResponse(MeasurementUnit unit) =>
        new(unit.Name, unit.Symbol, unit.ToBaseFactor, unit.ToBaseOffset, unit.Aliases);
}
