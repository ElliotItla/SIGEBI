using Microsoft.Extensions.DependencyInjection;

namespace SIGEBI.BL;

public static class DependencyInjection
{
    // Registra los servicios de negocio. Cada módulo agrega los suyos aquí.
    public static IServiceCollection AddBL(this IServiceCollection services)
    {
        return services;
    }
}
