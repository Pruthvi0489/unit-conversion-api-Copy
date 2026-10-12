using Microsoft.Extensions.DependencyInjection;
using UnitConversion.Application.Services;

namespace UnitConversion.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IConversionService, ConversionService>();
        services.AddScoped<ICatalogService, CatalogService>();

        return services;
    }
}
