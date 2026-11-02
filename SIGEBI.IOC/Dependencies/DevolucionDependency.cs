using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SIGEBI.BL.Interfaces;
using SIGEBI.BL.Services;
using SIGEBI.Persistence.Interfaces.Catalogo;
using SIGEBI.Persistence.Interfaces.Prestamos;
using SIGEBI.Persistence.Interfaces.Seguridad;
using SIGEBI.Persistence.Repositories.Catalogo;
using SIGEBI.Persistence.Repositories.Prestamos;
using SIGEBI.Persistence.Repositories.Seguridad;

namespace SIGEBI.IOC.Dependencies;

public static class DevolucionDependency
{
    public static IServiceCollection AddDevolucionDependency(this IServiceCollection services)
    {
        services.TryAddScoped<IPrestamoRepositorio, PrestamoRepositorio>();
        services.TryAddScoped<IPenalizacionRepositorio, PenalizacionRepositorio>();
        services.TryAddScoped<IEjemplarRepositorio, EjemplarRepositorio>();
        services.TryAddScoped<IRecursoRepositorio, RecursoRepositorio>();
        services.TryAddScoped<IUsuarioRepositorio, UsuarioRepositorio>();
        services.TryAddScoped<IHistorialEstadoEjemplarRepositorio, HistorialEstadoEjemplarRepositorio>();

        services.AddScoped<IDevolucionServicio, DevolucionServicio>();

        return services;
    }
}