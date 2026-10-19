using GestorFinanzas.Web.Models.Entities;
using GestorFinanzas.Web.Models.Filtros;

namespace GestorFinanzas.Web.Data.Repositories;

public interface ITransaccionRepository
{
    /// <summary>
    /// Busca un movimiento que pertenezca al usuario (a través de su cuenta).
    /// Con <paramref name="conSeguimiento"/> en true la entidad queda rastreada para poder editarla o borrarla.
    /// </summary>
    Task<Transaccion?> ObtenerAsync(int id, int usuarioId, bool conSeguimiento = false);

    Task<ResultadoPaginado<Transaccion>> BuscarAsync(int usuarioId, FiltroMovimientos filtro);

    /// <summary>Suma de ingresos y de gastos de todos los movimientos que cumplen el filtro (sin paginar).</summary>
    Task<ResumenMovimientos> ResumirAsync(int usuarioId, FiltroMovimientos filtro);

    Task AgregarAsync(Transaccion transaccion);

    void Eliminar(Transaccion transaccion);
}
