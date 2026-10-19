using GestorFinanzas.Web.Data;
using GestorFinanzas.Web.Data.Seed;
using Microsoft.EntityFrameworkCore;

namespace GestorFinanzas.Web.Services;

/// <summary>
/// Implementación TEMPORAL: siempre devuelve el usuario de demostración.
/// Se reemplaza cuando se integre el login (leyendo el Id desde los claims del usuario autenticado).
/// </summary>
public class UsuarioDemoService : ICurrentUserService
{
    private readonly AppDbContext _contexto;
    private int? _usuarioId;

    public UsuarioDemoService(AppDbContext contexto)
    {
        _contexto = contexto;
    }

    public int UsuarioId => _usuarioId ??= BuscarUsuarioDemo();

    private int BuscarUsuarioDemo()
    {
        var id = _contexto.Usuarios
            .AsNoTracking()
            .Where(u => u.Correo == DatosDemo.Correo)
            .Select(u => u.Id)
            .FirstOrDefault();

        if (id == 0)
        {
            throw new InvalidOperationException(
                "No existe el usuario de demostración. Activa BaseDeDatos:CargarDatosDemo en appsettings.json " +
                "o ejecuta database/02_datos_demo.sql.");
        }

        return id;
    }
}
