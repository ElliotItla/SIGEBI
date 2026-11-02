using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using SIGEBI.Persistence;

namespace SIGEBI.IOC;

public static class DependencyContainer
{
    // Punto único de registro: la base de datos y las dependencias de cada módulo
    public static IServiceCollection AddSigebi(this IServiceCollection services, string cadenaConexion)
    {
        services.AddPersistence(cadenaConexion);

        // Cada módulo tiene su archivo en la carpeta Dependencies, con un método AddXDependency.
        // Se ejecutan todos solos, así un módulo nuevo no obliga a editar este archivo.
        var metodos = typeof(DependencyContainer).Assembly.GetTypes()
            .Where(t => t.IsClass && t.IsAbstract && t.IsSealed && t.Namespace == "SIGEBI.IOC.Dependencies")
            .SelectMany(t => t.GetMethods(BindingFlags.Public | BindingFlags.Static))
            .Where(m => m.Name.StartsWith("Add") && m.Name.EndsWith("Dependency") && m.GetParameters().Length == 1);

        foreach (var metodo in metodos)
        {
            metodo.Invoke(null, new object[] { services });
        }

        return services;
    }
}