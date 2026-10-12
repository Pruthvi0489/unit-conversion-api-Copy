using Microsoft.AspNetCore.Mvc;
using UnitConversion.Application.Contracts;
using UnitConversion.Application.Services;

namespace UnitConversion.Api.Controllers;

[ApiController]
[Route("api/v1/categories")]
[Produces("application/json")]
public class CategoriesController : ControllerBase
{
    private readonly ICatalogService _catalogService;

    public CategoriesController(ICatalogService catalogService)
    {
        _catalogService = catalogService;
    }

    /// <summary>
    /// Lists the supported conversion categories.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<CategoryResponse>), StatusCodes.Status200OK)]
    public ActionResult<IReadOnlyList<CategoryResponse>> GetCategories()
    {
        return Ok(_catalogService.GetCategories());
    }

    /// <summary>
    /// Lists the units that belong to a category.
    /// </summary>
    /// <param name="category">The category name, for example "length".</param>
    /// <response code="404">The category does not exist.</response>
    [HttpGet("{category}/units")]
    [ProducesResponseType(typeof(IReadOnlyList<UnitResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public ActionResult<IReadOnlyList<UnitResponse>> GetUnits(string category)
    {
        var units = _catalogService.GetUnits(category);

        return units is null ? CategoryNotFound(category) : Ok(units);
    }

    /// <summary>
    /// Creates a new category together with its units.
    /// </summary>
    /// <remarks>
    /// Every factor is relative to one base unit of your choice, which should have a factor of 1.
    /// The category lives in memory and is lost when the API restarts.
    /// </remarks>
    /// <response code="201">The category was created.</response>
    /// <response code="400">The request is invalid.</response>
    /// <response code="409">A category with this name already exists, or two units share a name, symbol or alias.</response>
    [HttpPost]
    [ProducesResponseType(typeof(CategoryResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public ActionResult<CategoryResponse> CreateCategory(CreateCategoryRequest request)
    {
        var created = _catalogService.AddCategory(request);

        return CreatedAtAction(nameof(GetUnits), new { category = created.Name }, created);
    }

    /// <summary>
    /// Adds a unit to an existing category.
    /// </summary>
    /// <remarks>
    /// The factor is relative to the category's base unit (meter for length, kilogram for mass,
    /// celsius for temperature, liter for volume). The unit lives in memory and is lost when the API restarts.
    /// </remarks>
    /// <param name="category">The category to add the unit to.</param>
    /// <response code="201">The unit was added.</response>
    /// <response code="400">The request is invalid.</response>
    /// <response code="404">The category does not exist.</response>
    /// <response code="409">The name, symbol or an alias is already used in this category.</response>
    [HttpPost("{category}/units")]
    [ProducesResponseType(typeof(UnitResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public ActionResult<UnitResponse> AddUnit(string category, CreateUnitRequest request)
    {
        var created = _catalogService.AddUnit(category, request);

        return created is null
            ? CategoryNotFound(category)
            : CreatedAtAction(nameof(GetUnits), new { category }, created);
    }

    private ObjectResult CategoryNotFound(string category) =>
        Problem(
            title: "Category not found",
            detail: $"The category '{category}' is not supported.",
            statusCode: StatusCodes.Status404NotFound);
}
