using GestorFinanzas.Web.Models.Entities;
using GestorFinanzas.Web.Models.Filtros;

namespace GestorFinanzas.Web.Data.Repositories;

public interface ITransaccionRepository
{
    // Busca un movimiento que pertenezca al usuario (a través de su cuenta).
    // Con conSeguimiento en true la entidad queda rastreada para poder editarla o borrarla.
    Task<Transaccion?> ObtenerAsync(int id, int usuarioId, bool conSeguimiento = false);

    Task<ResultadoPaginado<Transaccion>> BuscarAsync(int usuarioId, FiltroMovimientos filtro);

    // Suma de ingresos y de gastos de todos los movimientos que cumplen el filtro (sin paginar).
    Task<ResumenMovimientos> ResumirAsync(int usuarioId, FiltroMovimientos filtro);

    Task AgregarAsync(Transaccion transaccion);

    void Eliminar(Transaccion transaccion);
}
