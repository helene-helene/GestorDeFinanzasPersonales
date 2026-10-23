using GestorFinanzas.Web.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestorFinanzas.Web.Data.Configurations;

public class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.ToTable("Usuarios");
        builder.HasKey(u => u.Id);

        builder.Property(u => u.Nombre).IsRequired().HasMaxLength(100);
        builder.Property(u => u.Correo).IsRequired().HasMaxLength(150);
        builder.Property(u => u.Contrasenia).IsRequired().HasMaxLength(256);
        builder.Property(u => u.FechaRegistro).HasColumnType("datetime").HasDefaultValueSql("GETDATE()");

        // El correo identifica al usuario al iniciar sesión, por eso no se puede repetir.
        builder.HasIndex(u => u.Correo).IsUnique();
    }
}
