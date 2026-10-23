using GestorFinanzas.Web.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestorFinanzas.Web.Data.Configurations;

public class RecomendacionIAConfiguration : IEntityTypeConfiguration<RecomendacionIA>
{
    public void Configure(EntityTypeBuilder<RecomendacionIA> builder)
    {
        builder.ToTable("RecomendacionesIA");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.TipoRecomendacion).IsRequired().HasMaxLength(50);
        builder.Property(r => r.Descripcion).IsRequired().HasMaxLength(300);
        builder.Property(r => r.FechaGeneracion).HasColumnType("datetime");
        builder.Property(r => r.Aplicada).HasDefaultValue(false);

        builder.HasOne(r => r.Usuario)
            .WithMany(u => u.Recomendaciones)
            .HasForeignKey(r => r.UsuarioId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
