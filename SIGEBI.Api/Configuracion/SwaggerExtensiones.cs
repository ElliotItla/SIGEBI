using System.Reflection;
using Microsoft.OpenApi.Models;

namespace SIGEBI.Api.Configuracion;

public static class SwaggerExtensiones
{
    public static IServiceCollection AddSwaggerSigebi(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();

        services.AddSwaggerGen(opciones =>
        {
            opciones.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "SIGEBI API",
                Version = "v1",
                Description = "API del Sistema de Gestión de Bibliotecas. Inicia sesión en Auth/Login, copia el accessToken y pégalo en el botón Authorize."
            });

            // Los comentarios /// de los controladores aparecen como descripción de cada endpoint
            var archivoXml = Path.Combine(AppContext.BaseDirectory, $"{Assembly.GetExecutingAssembly().GetName().Name}.xml");

            if (File.Exists(archivoXml))
            {
                opciones.IncludeXmlComments(archivoXml);
            }

            opciones.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                In = ParameterLocation.Header,
                Description = "Pega aquí el accessToken que devuelve Auth/Login (solo el token, sin la palabra Bearer)."
            });

            opciones.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } },
                    Array.Empty<string>()
                }
            });
        });

        return services;
    }

    public static WebApplication UseSwaggerSigebi(this WebApplication app)
    {
        app.UseSwagger();
        app.UseSwaggerUI(opciones => opciones.SwaggerEndpoint("/swagger/v1/swagger.json", "SIGEBI API v1"));

        return app;
    }
}