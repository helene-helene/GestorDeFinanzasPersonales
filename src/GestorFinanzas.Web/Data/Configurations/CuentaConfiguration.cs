using GestorFinanzas.Web.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestorFinanzas.Web.Data.Configurations;

public class CuentaConfiguration : IEntityTypeConfiguration<Cuenta>
{
    public void Configure(EntityTypeBuilder<Cuenta> builder)
    {
        builder.ToTable("Cuentas", t =>
        {
            t.HasCheckConstraint("CK_Cuentas_TipoCuenta", "[TipoCuenta] IN ('Ahorro','Corriente','Efectivo','Tarjeta')");
        });
        builder.HasKey(c => c.Id);

        builder.Property(c => c.NombreCuenta).IsRequired().HasMaxLength(100);
        builder.Property(c => c.TipoCuenta).IsRequired().HasMaxLength(30);
        builder.Property(c => c.Moneda).IsRequired().HasMaxLength(10);

        // El saldo también funciona como token de concurrencia: si dos operaciones intentan
        // modificar el saldo a la vez, la segunda falla en lugar de pisar a la primera.
        builder.Property(c => c.SaldoActual)
            .HasColumnType("decimal(12,2)")
            .HasDefaultValue(0m)
            .IsConcurrencyToken();

        builder.HasOne(c => c.Usuario)
            .WithMany(u => u.Cuentas)
            .HasForeignKey(c => c.UsuarioId)
            .OnDelete(DeleteBehavior.Cascade);

        // Un usuario no puede tener dos cuentas con el mismo nombre.
        builder.HasIndex(c => new { c.UsuarioId, c.NombreCuenta }).IsUnique();
    }
}
