using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SIGEBI.BL.Interfaces;
using SIGEBI.BL.Services;
using SIGEBI.Persistence.Interfaces.Configuracion;
using SIGEBI.Persistence.Interfaces.Prestamos;
using SIGEBI.Persistence.Interfaces.Seguridad;
using SIGEBI.Persistence.Repositories.Configuracion;
using SIGEBI.Persistence.Repositories.Prestamos;
using SIGEBI.Persistence.Repositories.Seguridad;

namespace SIGEBI.IOC.Dependencies;

public static class UsuarioDependency
{
    public static IServiceCollection AddUsuarioDependency(this IServiceCollection services)
    {
        services.TryAddScoped<IUsuarioRepositorio, UsuarioRepositorio>();
        services.TryAddScoped<IParametroSistemaRepositorio, ParametroSistemaRepositorio>();
        services.TryAddScoped<IPenalizacionRepositorio, PenalizacionRepositorio>();
        services.TryAddScoped<IPrestamoRepositorio, PrestamoRepositorio>();

        services.AddScoped<IServicioContrasena, ServicioContrasena>();
        services.AddScoped<IUsuarioServicio, UsuarioServicio>();

        return services;
    }
}