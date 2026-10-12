using Microsoft.AspNetCore.Mvc;
using UnitConversion.Application.Contracts;
using UnitConversion.Application.Services;

namespace UnitConversion.Api.Controllers;

[ApiController]
[Route("api/v1/conversions")]
[Produces("application/json")]
public class ConversionsController : ControllerBase
{
    private readonly IConversionService _conversionService;

    public ConversionsController(IConversionService conversionService)
    {
        _conversionService = conversionService;
    }

    /// <summary>
    /// Converts a value from one unit to another.
    /// </summary>
    /// <remarks>
    /// Units can be given by name, symbol or common alias and are not case sensitive,
    /// so "meters", "meter" and "m" all mean the same thing.
    /// Use the categories endpoints to see what is available.
    /// </remarks>
    /// <response code="200">The converted value.</response>
    /// <response code="400">The request is malformed, or the category or a unit is not supported.</response>
    [HttpPost]
    [ProducesResponseType(typeof(ConversionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public ActionResult<ConversionResponse> ConvertUnits(ConversionRequest request)
    {
        return Ok(_conversionService.Convert(request));
    }
}
