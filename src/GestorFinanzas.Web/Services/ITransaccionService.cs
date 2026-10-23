using GestorFinanzas.Web.Models.Entities;
using GestorFinanzas.Web.Models.Filtros;

namespace GestorFinanzas.Web.Services;

public interface ITransaccionService
{
    Task<ResultadoPaginado<Transaccion>> ListarAsync(FiltroMovimientos filtro);
    Task<ResumenMovimientos> ResumirAsync(FiltroMovimientos filtro);
    Task<Transaccion?> ObtenerAsync(int id);

    Task<Transaccion> CrearAsync(MovimientoDatos datos);

    Task<Transaccion> ActualizarAsync(int id, MovimientoDatos datos);

    Task EliminarAsync(int id);

    Task<IReadOnlyList<Cuenta>> ListarCuentasAsync();
    Task<IReadOnlyList<Categoria>> ListarCategoriasAsync(string? tipo = null);
}
