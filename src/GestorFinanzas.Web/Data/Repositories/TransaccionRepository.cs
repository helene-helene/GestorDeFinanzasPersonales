using GestorFinanzas.Web.Models;
using GestorFinanzas.Web.Models.Entities;
using GestorFinanzas.Web.Models.Filtros;
using Microsoft.EntityFrameworkCore;

namespace GestorFinanzas.Web.Data.Repositories;

public class TransaccionRepository : ITransaccionRepository
{
    private readonly AppDbContext _contexto;

    public TransaccionRepository(AppDbContext contexto)
    {
        _contexto = contexto;
    }

    public async Task<Transaccion?> ObtenerAsync(int id, int usuarioId, bool conSeguimiento = false)
    {
        IQueryable<Transaccion> consulta = _contexto.Transacciones
            .Include(t => t.Cuenta)
            .Include(t => t.Categoria)
            .Where(t => t.Id == id && t.Cuenta.UsuarioId == usuarioId);

        if (!conSeguimiento)
        {
            consulta = consulta.AsNoTracking();
        }

        return await consulta.FirstOrDefaultAsync();
    }

    public async Task<ResultadoPaginado<Transaccion>> BuscarAsync(int usuarioId, FiltroMovimientos filtro)
    {
        var consulta = Filtrar(usuarioId, filtro);

        var total = await consulta.CountAsync();

        var items = await consulta
            .Include(t => t.Cuenta)
            .Include(t => t.Categoria)
            .OrderByDescending(t => t.Fecha)
            .ThenByDescending(t => t.Id)
            .Skip((filtro.Pagina - 1) * filtro.TamanioPagina)
            .Take(filtro.TamanioPagina)
            .AsNoTracking()
            .ToListAsync();

        return new ResultadoPaginado<Transaccion>
        {
            Items = items,
            Total = total,
            Pagina = filtro.Pagina,
            TamanioPagina = filtro.TamanioPagina
        };
    }

    public async Task<ResumenMovimientos> ResumirAsync(int usuarioId, FiltroMovimientos filtro)
    {
        var consulta = Filtrar(usuarioId, filtro);

        var ingresos = await consulta
            .Where(t => t.Tipo == TipoMovimiento.Ingreso)
            .SumAsync(t => (decimal?)t.Monto) ?? 0m;

        var gastos = await consulta
            .Where(t => t.Tipo == TipoMovimiento.Gasto)
            .SumAsync(t => (decimal?)t.Monto) ?? 0m;

        return new ResumenMovimientos { TotalIngresos = ingresos, TotalGastos = gastos };
    }

    public async Task AgregarAsync(Transaccion transaccion) =>
        await _contexto.Transacciones.AddAsync(transaccion);

    public void Eliminar(Transaccion transaccion) =>
        _contexto.Transacciones.Remove(transaccion);

    // Aplica los criterios del filtro. Siempre limita a los movimientos del usuario.
    private IQueryable<Transaccion> Filtrar(int usuarioId, FiltroMovimientos filtro)
    {
        var consulta = _contexto.Transacciones
            .Where(t => t.Cuenta.UsuarioId == usuarioId);

        if (filtro.CuentaId.HasValue)
        {
            var cuentaId = filtro.CuentaId.Value;
            consulta = consulta.Where(t => t.CuentaId == cuentaId);
        }

        if (!string.IsNullOrWhiteSpace(filtro.Tipo))
        {
            var tipo = filtro.Tipo;
            consulta = consulta.Where(t => t.Tipo == tipo);
        }

        if (filtro.CategoriaId.HasValue)
        {
            var categoriaId = filtro.CategoriaId.Value;
            consulta = consulta.Where(t => t.CategoriaId == categoriaId);
        }

        if (filtro.Desde.HasValue)
        {
            var desde = filtro.Desde.Value.Date;
            consulta = consulta.Where(t => t.Fecha >= desde);
        }

        if (filtro.Hasta.HasValue)
        {
            // "Hasta" incluye todo el día elegido.
            var hastaExclusivo = filtro.Hasta.Value.Date.AddDays(1);
            consulta = consulta.Where(t => t.Fecha < hastaExclusivo);
        }

        if (!string.IsNullOrWhiteSpace(filtro.Texto))
        {
            var texto = filtro.Texto.Trim();
            consulta = consulta.Where(t =>
                (t.Descripcion != null && t.Descripcion.Contains(texto)) ||
                t.Categoria.Nombre.Contains(texto));
        }

        return consulta;
    }
}
