using System.Globalization;
using GestorFinanzas.Web.Data;
using GestorFinanzas.Web.Data.Repositories;
using GestorFinanzas.Web.Models.Entities;
using GestorFinanzas.Web.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
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
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();

// Capa de lógica de negocio
builder.Services.AddScoped<ITransaccionService, TransaccionService>();
builder.Services.AddScoped<IAutenticacionService, AutenticacionService>();
builder.Services.AddScoped<IPerfilService, PerfilService>();
builder.Services.AddScoped<IPasswordHasher<Usuario>, PasswordHasher<Usuario>>();

// Usuario actual: se obtiene de la cookie de sesión (claims del usuario que inició sesión).
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

// Autenticación con cookies. La cookie solo la puede leer el servidor (HttpOnly)
// y se marca como segura cuando la petición llega por HTTPS.
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(opciones =>
    {
        opciones.LoginPath = "/Acceso/Login";
        opciones.LogoutPath = "/Acceso/Logout";
        opciones.AccessDeniedPath = "/Acceso/Login";
        opciones.Cookie.Name = "GestorFinanzas.Sesion";
        opciones.Cookie.HttpOnly = true;
        opciones.Cookie.SameSite = SameSiteMode.Lax;
        opciones.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        opciones.ExpireTimeSpan = TimeSpan.FromMinutes(60);
        opciones.SlidingExpiration = true;
    });

// Todas las páginas piden sesión iniciada, salvo las marcadas con [AllowAnonymous].
// Así los módulos nuevos del equipo quedan protegidos aunque olviden poner [Authorize].
builder.Services.AddAuthorization(opciones =>
{
    opciones.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

// Todos los formularios POST deben traer el token antifalsificación.
builder.Services.AddControllersWithViews(opciones =>
{
    opciones.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
});

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
    // Fuera de desarrollo nunca se muestra el detalle técnico del error, solo una página amigable.
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Páginas amigables para 404 (no existe), 400 (petición inválida), etc.
app.UseStatusCodePagesWithReExecute("/Home/Estado/{0}");

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
