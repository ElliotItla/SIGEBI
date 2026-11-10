namespace SIGEBI.Web;

public static class WebDependency
{
    // Registra las tareas en segundo plano de la carpeta Servicios, así un módulo nuevo no obliga a editar Program.cs
    public static IServiceCollection AddSigebiWeb(this IServiceCollection services)
    {
        var tareas = typeof(WebDependency).Assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && typeof(IHostedService).IsAssignableFrom(t) && t.Namespace == "SIGEBI.Web.Servicios");

        foreach (var tarea in tareas)
        {
            services.AddSingleton(typeof(IHostedService), tarea);
        }

        return services;
    }
}