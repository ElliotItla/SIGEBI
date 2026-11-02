using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SIGEBI.DAL.Repository;
using SIGEBI.Persistence.Base;
using SIGEBI.Persistence.Context;

namespace SIGEBI.Persistence;

public static class DependencyInjection
{
    // Registra el acceso a la base de datos. Los repositorios de cada módulo se registran en la capa IOC.
    public static IServiceCollection AddPersistence(this IServiceCollection services, string cadenaConexion)
    {
        services.AddDbContext<SigebiContext>(opciones => opciones.UseSqlServer(cadenaConexion));
        services.AddScoped<IUnidadDeTrabajo, UnidadDeTrabajo>();

        return services;
    }
}