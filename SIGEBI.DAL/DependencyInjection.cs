using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SIGEBI.DAL.Data;

namespace SIGEBI.DAL;

public static class DependencyInjection
{
    // Registra el acceso a la base de datos. Los repositorios se irán registrando aquí.
    public static IServiceCollection AddDAL(this IServiceCollection services, string cadenaConexion)
    {
        services.AddDbContext<SigebiDbContext>(opciones => opciones.UseSqlServer(cadenaConexion));

        return services;
    }
}
