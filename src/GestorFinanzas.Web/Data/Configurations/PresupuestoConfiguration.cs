using GestorFinanzas.Web.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestorFinanzas.Web.Data.Configurations;

public class PresupuestoConfiguration : IEntityTypeConfiguration<Presupuesto>
{
    public void Configure(EntityTypeBuilder<Presupuesto> builder)
    {
        builder.ToTable("Presupuestos", t =>
        {
            t.HasCheckConstraint("CK_Presupuestos_MontoLimite", "[MontoLimite] > 0");
            t.HasCheckConstraint("CK_Presupuestos_Fechas", "[FechaFin] >= [FechaInicio]");
        });
        builder.HasKey(p => p.Id);

        builder.Property(p => p.MontoLimite).HasColumnType("decimal(12,2)");
        builder.Property(p => p.Periodo).IsRequired().HasMaxLength(20);
        builder.Property(p => p.FechaInicio).HasColumnType("date");
        builder.Property(p => p.FechaFin).HasColumnType("date");

        builder.HasOne(p => p.Usuario)
            .WithMany(u => u.Presupuestos)
            .HasForeignKey(p => p.UsuarioId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(p => p.Categoria)
            .WithMany(c => c.Presupuestos)
            .HasForeignKey(p => p.CategoriaId)
            .OnDelete(DeleteBehavior.Restrict);

        // No puede haber dos presupuestos de la misma categoría que empiecen el mismo día.
        builder.HasIndex(p => new { p.UsuarioId, p.CategoriaId, p.FechaInicio }).IsUnique();
    }
}
