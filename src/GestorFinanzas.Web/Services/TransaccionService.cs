using GestorFinanzas.Web.Data.Repositories;
using GestorFinanzas.Web.Infrastructure;
using GestorFinanzas.Web.Models;
using GestorFinanzas.Web.Models.Entities;
using GestorFinanzas.Web.Models.Filtros;
using Microsoft.EntityFrameworkCore;

namespace GestorFinanzas.Web.Services;

/// <summary>
/// Reglas de negocio de los movimientos (ingresos y gastos):
///  1. El monto es mayor que 0, con máximo 2 decimales.
///  2. La fecha no puede ser futura.
///  3. La cuenta tiene que ser del usuario que está haciendo la operación.
///  4. La categoría debe ser del mismo tipo que el movimiento (un gasto no usa una categoría de ingreso).
///  5. Cada movimiento actualiza el saldo de su cuenta: un ingreso suma y un gasto resta.
///  6. Las cuentas que no son de tarjeta no pueden quedar con saldo negativo.
/// </summary>
public class TransaccionService : ITransaccionService
{
    private const decimal MontoMaximo = 9_999_999_999.99m;
    private const int LongitudMaximaDescripcion = 200;
    private const int TamanioPaginaMinimo = 5;
    private const int TamanioPaginaMaximo = 50;

    private readonly ITransaccionRepository _transacciones;
    private readonly ICuentaRepository _cuentas;
    private readonly ICategoriaRepository _categorias;
    private readonly IUnitOfWork _unidadDeTrabajo;
    private readonly ICurrentUserService _usuarioActual;

    public TransaccionService(
        ITransaccionRepository transacciones,
        ICuentaRepository cuentas,
        ICategoriaRepository categorias,
        IUnitOfWork unidadDeTrabajo,
        ICurrentUserService usuarioActual)
    {
        _transacciones = transacciones;
        _cuentas = cuentas;
        _categorias = categorias;
        _unidadDeTrabajo = unidadDeTrabajo;
        _usuarioActual = usuarioActual;
    }

    // ---------------------------------------------------------------- Consultas

    public Task<ResultadoPaginado<Transaccion>> ListarAsync(FiltroMovimientos filtro)
    {
        Normalizar(filtro);
        return _transacciones.BuscarAsync(_usuarioActual.UsuarioId, filtro);
    }

    public Task<ResumenMovimientos> ResumirAsync(FiltroMovimientos filtro)
    {
        Normalizar(filtro);
        return _transacciones.ResumirAsync(_usuarioActual.UsuarioId, filtro);
    }

    public Task<Transaccion?> ObtenerAsync(int id) =>
        _transacciones.ObtenerAsync(id, _usuarioActual.UsuarioId);

    public Task<IReadOnlyList<Cuenta>> ListarCuentasAsync() =>
        _cuentas.ListarPorUsuarioAsync(_usuarioActual.UsuarioId);

    public Task<IReadOnlyList<Categoria>> ListarCategoriasAsync(string? tipo = null) =>
        _categorias.ListarAsync(tipo);

    // ---------------------------------------------------------------- Crear

    public async Task<Transaccion> CrearAsync(MovimientoDatos datos)
    {
        Validar(datos);
        var usuarioId = _usuarioActual.UsuarioId;

        var cuenta = await _cuentas.ObtenerAsync(datos.CuentaId, usuarioId)
            ?? throw new ReglaNegocioException("La cuenta seleccionada no existe.");
        await ValidarCategoriaAsync(datos);

        AplicarEfectoEnSaldo(cuenta, datos.Tipo, datos.Monto, +1);
        ValidarSaldo(cuenta);

        var transaccion = new Transaccion
        {
            CuentaId = cuenta.Id,
            CategoriaId = datos.CategoriaId,
            Monto = datos.Monto,
            Tipo = datos.Tipo,
            Fecha = datos.Fecha.Date,
            Descripcion = LimpiarTexto(datos.Descripcion)
        };

        await _transacciones.AgregarAsync(transaccion);
        await GuardarAsync();
        return transaccion;
    }

    // ---------------------------------------------------------------- Actualizar

    public async Task<Transaccion> ActualizarAsync(int id, MovimientoDatos datos)
    {
        Validar(datos);
        var usuarioId = _usuarioActual.UsuarioId;

        var transaccion = await _transacciones.ObtenerAsync(id, usuarioId, conSeguimiento: true)
            ?? throw new ReglaNegocioException("El movimiento que intentas modificar no existe.");

        // Si la cuenta no cambió, EF devuelve la misma instancia que ya tiene la transacción.
        var cuentaNueva = await _cuentas.ObtenerAsync(datos.CuentaId, usuarioId)
            ?? throw new ReglaNegocioException("La cuenta seleccionada no existe.");
        await ValidarCategoriaAsync(datos);

        var cuentaAnterior = transaccion.Cuenta;

        // Se deshace el efecto del movimiento original y se aplica el nuevo.
        AplicarEfectoEnSaldo(cuentaAnterior, transaccion.Tipo, transaccion.Monto, -1);
        AplicarEfectoEnSaldo(cuentaNueva, datos.Tipo, datos.Monto, +1);

        ValidarSaldo(cuentaAnterior);
        if (!ReferenceEquals(cuentaAnterior, cuentaNueva))
        {
            ValidarSaldo(cuentaNueva);
        }

        transaccion.CuentaId = cuentaNueva.Id;
        transaccion.CategoriaId = datos.CategoriaId;
        transaccion.Monto = datos.Monto;
        transaccion.Tipo = datos.Tipo;
        transaccion.Fecha = datos.Fecha.Date;
        transaccion.Descripcion = LimpiarTexto(datos.Descripcion);

        await GuardarAsync();
        return transaccion;
    }

    // ---------------------------------------------------------------- Eliminar

    public async Task EliminarAsync(int id)
    {
        var transaccion = await _transacciones.ObtenerAsync(id, _usuarioActual.UsuarioId, conSeguimiento: true)
            ?? throw new ReglaNegocioException("El movimiento que intentas eliminar no existe.");

        AplicarEfectoEnSaldo(transaccion.Cuenta, transaccion.Tipo, transaccion.Monto, -1);
        ValidarSaldo(transaccion.Cuenta);

        _transacciones.Eliminar(transaccion);
        await GuardarAsync();
    }

    // ---------------------------------------------------------------- Reglas internas

    private static void Validar(MovimientoDatos datos)
    {
        if (!TipoMovimiento.Todos.Contains(datos.Tipo))
        {
            throw new ReglaNegocioException("El tipo de movimiento debe ser Ingreso o Gasto.");
        }

        if (datos.Monto <= 0)
        {
            throw new ReglaNegocioException("El monto debe ser mayor que 0.");
        }

        if (datos.Monto > MontoMaximo)
        {
            throw new ReglaNegocioException("El monto es demasiado grande.");
        }

        if (decimal.Round(datos.Monto, 2) != datos.Monto)
        {
            throw new ReglaNegocioException("El monto admite como máximo 2 decimales.");
        }

        if (datos.Fecha.Date > DateTime.Today)
        {
            throw new ReglaNegocioException("La fecha del movimiento no puede ser futura.");
        }

        if (datos.Descripcion is { Length: > LongitudMaximaDescripcion })
        {
            throw new ReglaNegocioException($"La descripción no puede pasar de {LongitudMaximaDescripcion} caracteres.");
        }
    }

    private async Task ValidarCategoriaAsync(MovimientoDatos datos)
    {
        var categoria = await _categorias.ObtenerAsync(datos.CategoriaId)
            ?? throw new ReglaNegocioException("La categoría seleccionada no existe.");

        if (categoria.Tipo != datos.Tipo)
        {
            throw new ReglaNegocioException(
                $"La categoría '{categoria.Nombre}' es de tipo {categoria.Tipo} y no se puede usar en un movimiento de tipo {datos.Tipo}.");
        }
    }

    /// <summary>Un ingreso suma al saldo y un gasto resta. El signo -1 deshace el efecto.</summary>
    private static void AplicarEfectoEnSaldo(Cuenta cuenta, string tipo, decimal monto, int signo)
    {
        var variacion = tipo == TipoMovimiento.Ingreso ? monto : -monto;
        cuenta.SaldoActual += variacion * signo;
    }

    private static void ValidarSaldo(Cuenta cuenta)
    {
        if (cuenta.TipoCuenta != TipoCuenta.Tarjeta && cuenta.SaldoActual < 0)
        {
            throw new ReglaNegocioException(
                $"La cuenta '{cuenta.NombreCuenta}' no tiene saldo suficiente para esta operación.");
        }
    }

    private async Task GuardarAsync()
    {
        try
        {
            await _unidadDeTrabajo.GuardarCambiosAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ReglaNegocioException(
                "El saldo de la cuenta cambió mientras guardabas el movimiento. Vuelve a intentarlo.");
        }
    }

    private static void Normalizar(FiltroMovimientos filtro)
    {
        if (filtro.Pagina < 1)
        {
            filtro.Pagina = 1;
        }

        filtro.TamanioPagina = Math.Clamp(filtro.TamanioPagina, TamanioPaginaMinimo, TamanioPaginaMaximo);

        if (!string.IsNullOrWhiteSpace(filtro.Tipo) && !TipoMovimiento.Todos.Contains(filtro.Tipo))
        {
            filtro.Tipo = null;
        }
    }

    private static string? LimpiarTexto(string? texto) =>
        string.IsNullOrWhiteSpace(texto) ? null : texto.Trim();
}
