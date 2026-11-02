using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SIGEBI.BL.Interfaces;
using SIGEBI.BL.Services;
using SIGEBI.Persistence.Interfaces.Configuracion;
using SIGEBI.Persistence.Interfaces.Notificaciones;
using SIGEBI.Persistence.Interfaces.Prestamos;
using SIGEBI.Persistence.Interfaces.Seguridad;
using SIGEBI.Persistence.Repositories.Configuracion;
using SIGEBI.Persistence.Repositories.Notificaciones;
using SIGEBI.Persistence.Repositories.Prestamos;
using SIGEBI.Persistence.Repositories.Seguridad;

namespace SIGEBI.IOC.Dependencies;

public static class NotificacionDependency
{
    public static IServiceCollection AddNotificacionDependency(this IServiceCollection services)
    {
        services.TryAddScoped<INotificacionRepositorio, NotificacionRepositorio>();
        services.TryAddScoped<IPrestamoRepositorio, PrestamoRepositorio>();
        services.TryAddScoped<IParametroSistemaRepositorio, ParametroSistemaRepositorio>();
        services.TryAddScoped<IUsuarioRepositorio, UsuarioRepositorio>();

        services.AddScoped<INotificacionServicio, NotificacionServicio>();

        return services;
    }
}