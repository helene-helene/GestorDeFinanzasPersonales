using GestorFinanzas.Web.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestorFinanzas.Web.Data.Configurations;

public class TransaccionConfiguration : IEntityTypeConfiguration<Transaccion>
{
    public void Configure(EntityTypeBuilder<Transaccion> builder)
    {
        builder.ToTable("Transacciones", t =>
        {
            t.HasCheckConstraint("CK_Transacciones_Monto", "[Monto] > 0");
            t.HasCheckConstraint("CK_Transacciones_Tipo", "[Tipo] IN ('Ingreso','Gasto')");
        });
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Monto).HasColumnType("decimal(12,2)");
        builder.Property(t => t.Tipo).IsRequired().HasMaxLength(20);
        builder.Property(t => t.Fecha).HasColumnType("datetime");
        builder.Property(t => t.Descripcion).HasMaxLength(200);

        // Si se elimina una cuenta, también se eliminan sus movimientos.
        builder.HasOne(t => t.Cuenta)
            .WithMany(c => c.Transacciones)
            .HasForeignKey(t => t.CuentaId)
            .OnDelete(DeleteBehavior.Cascade);

        // Una categoría con movimientos no se puede borrar.
        builder.HasOne(t => t.Categoria)
            .WithMany(c => c.Transacciones)
            .HasForeignKey(t => t.CategoriaId)
            .OnDelete(DeleteBehavior.Restrict);

        // Acelera el historial: movimientos de una cuenta ordenados por fecha.
        builder.HasIndex(t => new { t.CuentaId, t.Fecha });
    }
}
