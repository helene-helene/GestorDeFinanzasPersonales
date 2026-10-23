using GestorFinanzas.Web.Models.Entities;

namespace GestorFinanzas.Web.Data.Repositories;

public interface ICategoriaRepository
{
    // Todas las categorías, o solo las de un tipo (Ingreso o Gasto).
    Task<IReadOnlyList<Categoria>> ListarAsync(string? tipo = null);

    Task<Categoria?> ObtenerAsync(int id);
}
