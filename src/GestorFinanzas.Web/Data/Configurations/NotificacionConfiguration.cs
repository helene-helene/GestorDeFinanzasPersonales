using GestorFinanzas.Web.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestorFinanzas.Web.Data.Configurations;

public class NotificacionConfiguration : IEntityTypeConfiguration<Notificacion>
{
    public void Configure(EntityTypeBuilder<Notificacion> builder)
    {
        builder.ToTable("Notificaciones");
        builder.HasKey(n => n.Id);

        builder.Property(n => n.Tipo).IsRequired().HasMaxLength(30);
        builder.Property(n => n.Mensaje).IsRequired().HasMaxLength(250);
        builder.Property(n => n.FechaEnvio).HasColumnType("datetime");
        builder.Property(n => n.Leida).HasDefaultValue(false);

        builder.HasOne(n => n.Usuario)
            .WithMany(u => u.Notificaciones)
            .HasForeignKey(n => n.UsuarioId)
            .OnDelete(DeleteBehavior.Cascade);

        // Para contar rápido las notificaciones sin leer de un usuario.
        builder.HasIndex(n => new { n.UsuarioId, n.Leida });
    }
}
