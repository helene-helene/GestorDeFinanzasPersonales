using System.Globalization;
using GestorFinanzas.Web.Data;
using GestorFinanzas.Web.Data.Repositories;
using GestorFinanzas.Web.Services;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// La cadena de conexión se puede sobrescribir con la variable de entorno
// ConnectionStrings__Default (así la usa Docker sin tocar el código).
var cadenaConexion = builder.Configuration.GetConnectionString("Default")
    ?? throw new InvalidOperationException(
        "Falta la cadena de conexión 'Default'. Configúrala en appsettings.json o con la variable de entorno ConnectionStrings__Default.");

builder.Services.AddDbContext<AppDbContext>(opciones => opciones.UseSqlServer(cadenaConexion));

// Capa de acceso a datos
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<ICuentaRepository, CuentaRepository>();
builder.Services.AddScoped<ICategoriaRepository, CategoriaRepository>();
builder.Services.AddScoped<ITransaccionRepository, TransaccionRepository>();

// Capa de lógica de negocio
builder.Services.AddScoped<ITransaccionService, TransaccionService>();

// Usuario actual. Mientras no esté integrado el login se usa el usuario de demostración.
// Cuando el módulo de autenticación esté listo, solo hay que cambiar esta línea por una
// implementación que lea el Id desde los claims del usuario autenticado.
builder.Services.AddScoped<ICurrentUserService, UsuarioDemoService>();

builder.Services.AddControllersWithViews();

var app = builder.Build();

// Aplica las migraciones y (opcionalmente) carga los datos de demostración.
await app.InicializarBaseDeDatosAsync();

// Formato numérico con punto decimal (1,234.56), igual que en El Salvador.
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(CultureInfo.InvariantCulture),
    SupportedCultures = new List<CultureInfo> { CultureInfo.InvariantCulture },
    SupportedUICultures = new List<CultureInfo> { CultureInfo.InvariantCulture }
});

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
