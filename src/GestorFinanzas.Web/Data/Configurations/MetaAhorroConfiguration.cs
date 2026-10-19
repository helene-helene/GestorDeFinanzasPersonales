using GestorFinanzas.Web.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestorFinanzas.Web.Data.Configurations;

public class MetaAhorroConfiguration : IEntityTypeConfiguration<MetaAhorro>
{
    public void Configure(EntityTypeBuilder<MetaAhorro> builder)
    {
        builder.ToTable("MetasAhorro", t =>
        {
            t.HasCheckConstraint("CK_MetasAhorro_MontoObjetivo", "[MontoObjetivo] > 0");
            t.HasCheckConstraint("CK_MetasAhorro_MontoActual", "[MontoActual] >= 0");
        });
        builder.HasKey(m => m.Id);

        builder.Property(m => m.NombreMeta).IsRequired().HasMaxLength(100);
        builder.Property(m => m.MontoObjetivo).HasColumnType("decimal(12,2)");
        builder.Property(m => m.MontoActual).HasColumnType("decimal(12,2)").HasDefaultValue(0m);
        builder.Property(m => m.FechaLimite).HasColumnType("date");

        builder.HasOne(m => m.Usuario)
            .WithMany(u => u.MetasAhorro)
            .HasForeignKey(m => m.UsuarioId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
