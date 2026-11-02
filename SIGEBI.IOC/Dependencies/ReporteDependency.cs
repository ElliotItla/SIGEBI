using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SIGEBI.BL.Interfaces;
using SIGEBI.BL.Services;
using SIGEBI.Persistence.Interfaces.Auditoria;
using SIGEBI.Persistence.Interfaces.Catalogo;
using SIGEBI.Persistence.Interfaces.Prestamos;
using SIGEBI.Persistence.Interfaces.Seguridad;
using SIGEBI.Persistence.Repositories.Auditoria;
using SIGEBI.Persistence.Repositories.Catalogo;
using SIGEBI.Persistence.Repositories.Prestamos;
using SIGEBI.Persistence.Repositories.Seguridad;

namespace SIGEBI.IOC.Dependencies;

public static class ReporteDependency
{
    public static IServiceCollection AddReporteDependency(this IServiceCollection services)
    {
        services.TryAddScoped<IRegistroAuditoriaRepositorio, RegistroAuditoriaRepositorio>();
        services.TryAddScoped<IPrestamoRepositorio, PrestamoRepositorio>();
        services.TryAddScoped<IPenalizacionRepositorio, PenalizacionRepositorio>();
        services.TryAddScoped<IEjemplarRepositorio, EjemplarRepositorio>();
        services.TryAddScoped<IRecursoRepositorio, RecursoRepositorio>();
        services.TryAddScoped<IUsuarioRepositorio, UsuarioRepositorio>();

        services.AddScoped<IAuditoriaServicio, AuditoriaServicio>();
        services.AddScoped<IReporteServicio, ReporteServicio>();

        return services;
    }
}