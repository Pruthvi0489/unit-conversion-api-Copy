using Microsoft.Extensions.DependencyInjection;
using UnitConversion.Application.Abstractions;
using UnitConversion.Infrastructure.Repositories;

namespace UnitConversion.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        // The catalog is read-only, so a single shared instance is enough.
        services.AddSingleton<ICategoryRepository, InMemoryCategoryRepository>();

        return services;
    }
}
