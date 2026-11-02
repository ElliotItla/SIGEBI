using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SIGEBI.BL.Interfaces;
using SIGEBI.BL.Services;
using SIGEBI.Persistence.Interfaces.Catalogo;
using SIGEBI.Persistence.Interfaces.Seguridad;
using SIGEBI.Persistence.Repositories.Catalogo;
using SIGEBI.Persistence.Repositories.Seguridad;

namespace SIGEBI.IOC.Dependencies;

public static class CatalogoDependency
{
    public static IServiceCollection AddCatalogoDependency(this IServiceCollection services)
    {
        services.TryAddScoped<IRecursoRepositorio, RecursoRepositorio>();
        services.TryAddScoped<IEjemplarRepositorio, EjemplarRepositorio>();
        services.TryAddScoped<IHistorialEstadoEjemplarRepositorio, HistorialEstadoEjemplarRepositorio>();
        services.TryAddScoped<IUsuarioRepositorio, UsuarioRepositorio>();

        services.AddScoped<IRecursoServicio, RecursoServicio>();

        return services;
    }
}