using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SIGEBI.Persistence.Context;

namespace SIGEBI.Persistence;

public static class DependencyInjection
{
    // Registra el acceso a la base de datos. Los repositorios de cada módulo se agregan aquí.
    public static IServiceCollection AddPersistence(this IServiceCollection services, string cadenaConexion)
    {
        services.AddDbContext<SigebiContext>(opciones => opciones.UseSqlServer(cadenaConexion));

        return services;
    }
}