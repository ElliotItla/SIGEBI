using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SIGEBI.BL.Interfaces;
using SIGEBI.BL.Services;
using SIGEBI.Persistence.Interfaces.Auditoria;
using SIGEBI.Persistence.Interfaces.Notificaciones;
using SIGEBI.Persistence.Repositories.Auditoria;
using SIGEBI.Persistence.Repositories.Notificaciones;

namespace SIGEBI.IOC.Dependencies;

// Servicios que usan todos los módulos: auditoría y notificaciones
public static class ComunDependency
{
    public static IServiceCollection AddComunDependency(this IServiceCollection services)
    {
        services.TryAddScoped<IRegistroAuditoriaRepositorio, RegistroAuditoriaRepositorio>();
        services.TryAddScoped<INotificacionRepositorio, NotificacionRepositorio>();

        services.AddScoped<IRegistradorAuditoria, RegistradorAuditoria>();
        services.AddScoped<IGeneradorNotificaciones, GeneradorNotificaciones>();

        return services;
    }
}