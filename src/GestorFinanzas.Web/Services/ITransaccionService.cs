using GestorFinanzas.Web.Models.Entities;
using GestorFinanzas.Web.Models.Filtros;

namespace GestorFinanzas.Web.Services;

public interface ITransaccionService
{
    Task<ResultadoPaginado<Transaccion>> ListarAsync(FiltroMovimientos filtro);
    Task<ResumenMovimientos> ResumirAsync(FiltroMovimientos filtro);
    Task<Transaccion?> ObtenerAsync(int id);

    /// <exception cref="Infrastructure.ReglaNegocioException">Si algún dato incumple una regla de negocio.</exception>
    Task<Transaccion> CrearAsync(MovimientoDatos datos);

    /// <exception cref="Infrastructure.ReglaNegocioException">Si algún dato incumple una regla de negocio.</exception>
    Task<Transaccion> ActualizarAsync(int id, MovimientoDatos datos);

    /// <exception cref="Infrastructure.ReglaNegocioException">Si el movimiento no existe o el saldo no lo permite.</exception>
    Task EliminarAsync(int id);

    Task<IReadOnlyList<Cuenta>> ListarCuentasAsync();
    Task<IReadOnlyList<Categoria>> ListarCategoriasAsync(string? tipo = null);
}
