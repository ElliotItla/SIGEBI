using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SIGEBI.BL.Interfaces;
using SIGEBI.BL.Services;
using SIGEBI.Persistence.Interfaces.Catalogo;
using SIGEBI.Persistence.Interfaces.Configuracion;
using SIGEBI.Persistence.Interfaces.Prestamos;
using SIGEBI.Persistence.Interfaces.Seguridad;
using SIGEBI.Persistence.Repositories.Catalogo;
using SIGEBI.Persistence.Repositories.Configuracion;
using SIGEBI.Persistence.Repositories.Prestamos;
using SIGEBI.Persistence.Repositories.Seguridad;

namespace SIGEBI.IOC.Dependencies;

public static class PrestamoDependency
{
    public static IServiceCollection AddPrestamoDependency(this IServiceCollection services)
    {
        services.TryAddScoped<IPrestamoRepositorio, PrestamoRepositorio>();
        services.TryAddScoped<IPoliticaPrestamoRepositorio, PoliticaPrestamoRepositorio>();
        services.TryAddScoped<IPenalizacionRepositorio, PenalizacionRepositorio>();
        services.TryAddScoped<IUsuarioRepositorio, UsuarioRepositorio>();
        services.TryAddScoped<IRecursoRepositorio, RecursoRepositorio>();
        services.TryAddScoped<IEjemplarRepositorio, EjemplarRepositorio>();
        services.TryAddScoped<IHistorialEstadoEjemplarRepositorio, HistorialEstadoEjemplarRepositorio>();

        services.AddScoped<IPrestamoServicio, PrestamoServicio>();

        return services;
    }
}