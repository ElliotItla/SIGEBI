using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SIGEBI.BL.Interfaces;
using SIGEBI.BL.Services;
using SIGEBI.Persistence.Interfaces.Prestamos;
using SIGEBI.Persistence.Interfaces.Seguridad;
using SIGEBI.Persistence.Repositories.Prestamos;
using SIGEBI.Persistence.Repositories.Seguridad;

namespace SIGEBI.IOC.Dependencies;

public static class PenalizacionDependency
{
    public static IServiceCollection AddPenalizacionDependency(this IServiceCollection services)
    {
        services.TryAddScoped<IPenalizacionRepositorio, PenalizacionRepositorio>();
        services.TryAddScoped<IPrestamoRepositorio, PrestamoRepositorio>();
        services.TryAddScoped<IUsuarioRepositorio, UsuarioRepositorio>();

        services.AddScoped<IPenalizacionServicio, PenalizacionServicio>();

        return services;
    }
}