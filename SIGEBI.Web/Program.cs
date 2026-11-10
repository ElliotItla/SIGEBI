using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using SIGEBI.IOC;
using SIGEBI.Web;

var builder = WebApplication.CreateBuilder(args);

var cadenaConexion = builder.Configuration.GetConnectionString("Sigebi")
    ?? throw new InvalidOperationException("Falta la cadena de conexión 'Sigebi' en appsettings.json.");

// Capas: datos y negocio (IOC) y las tareas propias de la capa web
builder.Services.AddSigebi(cadenaConexion);
builder.Services.AddSigebiWeb();
builder.Services.AddControllersWithViews();

// Inicio de sesión con cookies
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(opciones =>
    {
        opciones.Cookie.Name = "SIGEBI.Sesion";
        opciones.Cookie.HttpOnly = true;
        opciones.LoginPath = "/Cuenta/Ingresar";
        opciones.LogoutPath = "/Cuenta/Salir";
        opciones.AccessDeniedPath = "/Home/AccesoDenegado";
        opciones.ExpireTimeSpan = TimeSpan.FromMinutes(30);
        opciones.SlidingExpiration = true;
    });

// Todo exige sesión abierta, salvo lo que se marque con [AllowAnonymous]
builder.Services.AddAuthorization(opciones =>
{
    opciones.FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();