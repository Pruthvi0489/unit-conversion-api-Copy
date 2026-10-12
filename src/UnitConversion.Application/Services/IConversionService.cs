using UnitConversion.Application.Contracts;

namespace UnitConversion.Application.Services;

public interface IConversionService
{
    ConversionResponse Convert(ConversionRequest request);
}
