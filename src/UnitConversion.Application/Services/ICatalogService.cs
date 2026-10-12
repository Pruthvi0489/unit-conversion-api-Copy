using UnitConversion.Application.Contracts;

namespace UnitConversion.Application.Services;

public interface ICatalogService
{
    IReadOnlyList<CategoryResponse> GetCategories();

    /// <returns>The units of the category, or <c>null</c> if the category does not exist.</returns>
    IReadOnlyList<UnitResponse>? GetUnits(string category);

    CategoryResponse AddCategory(CreateCategoryRequest request);

    /// <returns>The new unit, or <c>null</c> if the category does not exist.</returns>
    UnitResponse? AddUnit(string category, CreateUnitRequest request);
}
