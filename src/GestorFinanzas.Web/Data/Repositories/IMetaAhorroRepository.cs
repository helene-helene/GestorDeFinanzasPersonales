using GestorFinanzas.Web.Models.Entities;

namespace GestorFinanzas.Web.Data.Repositories;

public interface IMetaAhorroRepository
{
    Task<IReadOnlyList<MetaAhorro>> ListarAsync(int usuarioId);
    Task<MetaAhorro?> ObtenerAsync(int id, int usuarioId);
    Task AgregarAsync(MetaAhorro meta);
    void Eliminar(MetaAhorro meta);
}
