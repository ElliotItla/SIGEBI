using SIGEBI.BL;
using SIGEBI.DAL;

var builder = WebApplication.CreateBuilder(args);

var cadenaConexion = builder.Configuration.GetConnectionString("Sigebi")
    ?? throw new InvalidOperationException("Falta la cadena de conexión 'Sigebi' en appsettings.json.");

// Capas: datos, negocio y presentación
builder.Services.AddDAL(cadenaConexion);
builder.Services.AddBL();
builder.Services.AddControllersWithViews();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
