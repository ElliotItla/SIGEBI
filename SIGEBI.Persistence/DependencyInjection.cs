using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SIGEBI.Persistence.Context;

namespace SIGEBI.Persistence;

public static class DependencyInjection
{
    // Registra el acceso a la base de datos y todos los repositorios.
    public static IServiceCollection AddPersistence(this IServiceCollection services, string cadenaConexion)
    {
        services.AddDbContext<SigebiContext>(opciones => opciones.UseSqlServer(cadenaConexion));

        // Cada repositorio se registra solo: IUsuarioRepositorio -> UsuarioRepositorio.
        // Así cada módulo agrega sus archivos sin editar este y las ramas no chocan al unirse.
        var repositorios = typeof(DependencyInjection).Assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && t.Name.EndsWith("Repositorio"));

        foreach (var repositorio in repositorios)
        {
            var interfaz = repositorio.GetInterfaces().FirstOrDefault(i => i.Name == "I" + repositorio.Name);

            if (interfaz != null)
            {
                services.AddScoped(interfaz, repositorio);
            }
        }

        return services;
    }
}