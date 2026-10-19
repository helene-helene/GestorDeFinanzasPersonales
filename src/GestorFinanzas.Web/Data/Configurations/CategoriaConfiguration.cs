using GestorFinanzas.Web.Data.Seed;
using GestorFinanzas.Web.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestorFinanzas.Web.Data.Configurations;

public class CategoriaConfiguration : IEntityTypeConfiguration<Categoria>
{
    public void Configure(EntityTypeBuilder<Categoria> builder)
    {
        builder.ToTable("Categorias", t =>
        {
            t.HasCheckConstraint("CK_Categorias_Tipo", "[Tipo] IN ('Ingreso','Gasto')");
        });
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Nombre).IsRequired().HasMaxLength(60);
        builder.Property(c => c.Tipo).IsRequired().HasMaxLength(20);
        builder.Property(c => c.Icono).HasMaxLength(50);

        builder.HasIndex(c => new { c.Nombre, c.Tipo }).IsUnique();

        // Categorías iniciales: se insertan con la migración, no con código en tiempo de ejecución.
        builder.HasData(CategoriasIniciales.Todas);
    }
}
