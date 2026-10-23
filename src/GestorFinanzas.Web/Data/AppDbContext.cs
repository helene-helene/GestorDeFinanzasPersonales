using GestorFinanzas.Web.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace GestorFinanzas.Web.Data;

// Contexto de Entity Framework Core. Las reglas de cada tabla (tipos, longitudes, relaciones,
// índices y restricciones) viven en las clases de la carpeta Data/Configurations.
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> opciones) : base(opciones)
    {
    }

    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Cuenta> Cuentas => Set<Cuenta>();
    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<Transaccion> Transacciones => Set<Transaccion>();
    public DbSet<Presupuesto> Presupuestos => Set<Presupuesto>();
    public DbSet<MetaAhorro> MetasAhorro => Set<MetaAhorro>();
    public DbSet<RecomendacionIA> RecomendacionesIA => Set<RecomendacionIA>();
    public DbSet<Notificacion> Notificaciones => Set<Notificacion>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
