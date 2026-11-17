using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIGEBI.Api.Configuracion;
using SIGEBI.DAL.Base;
using SIGEBI.IOC;

var builder = WebApplication.CreateBuilder(args);

var cadenaConexion = builder.Configuration.GetConnectionString("Sigebi")
    ?? throw new InvalidOperationException("Falta la cadena de conexión 'Sigebi' en appsettings.json.");

// Capas: datos y negocio (IOC)
builder.Services.AddSigebi(cadenaConexion);

builder.Services
    .AddControllers()
    .ConfigureApiBehaviorOptions(opciones =>
    {
        // Los datos inválidos del cuerpo de la petición se responden con la misma forma que el resto de la API
        opciones.InvalidModelStateResponseFactory = contexto =>
        {
            var mensaje = contexto.ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .FirstOrDefault(m => !string.IsNullOrWhiteSpace(m)) ?? "Los datos enviados no son válidos.";

            return new BadRequestObjectResult(new ResultadoOperacion { Exito = false, Mensaje = mensaje });
        };
    });

// Token de portador: el usuario inicia sesión en Auth/Login y envía el token en cada petición
var minutosToken = builder.Configuration.GetValue("Api:MinutosToken", 60);

builder.Services
    .AddAuthentication(BearerTokenDefaults.AuthenticationScheme)
    .AddBearerToken(opciones => opciones.BearerTokenExpiration = TimeSpan.FromMinutes(Math.Max(1, minutosToken)));

// Todo exige token, salvo lo que se marque con [AllowAnonymous]
builder.Services.AddAuthorization(opciones =>
{
    opciones.FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
});

builder.Services.AddSwaggerSigebi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwaggerSigebi();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();