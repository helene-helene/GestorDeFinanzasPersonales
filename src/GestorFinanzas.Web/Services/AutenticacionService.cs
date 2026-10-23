using GestorFinanzas.Web.Data.Repositories;
using GestorFinanzas.Web.Infrastructure;
using GestorFinanzas.Web.Models;
using GestorFinanzas.Web.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace GestorFinanzas.Web.Services;

// Inicio de sesión y registro de estudiantes.
// Las contraseñas se guardan y se comparan con PasswordHasher (hash PBKDF2 con sal),
// nunca en texto plano.
public class AutenticacionService : IAutenticacionService
{
    private readonly IUsuarioRepository _usuarios;
    private readonly IUnitOfWork _unidadDeTrabajo;
    private readonly IPasswordHasher<Usuario> _hasher;
    private readonly ILogger<AutenticacionService> _logger;

    public AutenticacionService(
        IUsuarioRepository usuarios,
        IUnitOfWork unidadDeTrabajo,
        IPasswordHasher<Usuario> hasher,
        ILogger<AutenticacionService> logger)
    {
        _usuarios = usuarios;
        _unidadDeTrabajo = unidadDeTrabajo;
        _hasher = hasher;
        _logger = logger;
    }

    public async Task<Usuario?> ValidarCredencialesAsync(string correo, string contrasenia, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(correo) || string.IsNullOrEmpty(contrasenia))
        {
            return null;
        }

        var usuario = await _usuarios.ObtenerPorCorreoAsync(ReglasUsuario.NormalizarCorreo(correo), cancellationToken);
        if (usuario is null)
        {
            _logger.LogInformation("Inicio de sesión rechazado: credenciales incorrectas.");
            return null;
        }

        PasswordVerificationResult resultado;
        try
        {
            resultado = _hasher.VerifyHashedPassword(usuario, usuario.Contrasenia, contrasenia);
        }
        catch (FormatException)
        {
            // La contraseña guardada no tiene formato de hash (por ejemplo, se insertó a mano en texto plano).
            _logger.LogWarning("El usuario {UsuarioId} tiene una contraseña guardada con un formato no válido.", usuario.Id);
            return null;
        }

        if (resultado == PasswordVerificationResult.Failed)
        {
            _logger.LogInformation("Inicio de sesión rechazado: credenciales incorrectas.");
            return null;
        }

        // Si el hash se creó con una configuración más antigua, se vuelve a generar con la actual.
        if (resultado == PasswordVerificationResult.SuccessRehashNeeded)
        {
            usuario.Contrasenia = _hasher.HashPassword(usuario, contrasenia);
            await _unidadDeTrabajo.GuardarCambiosAsync(cancellationToken);
        }

        _logger.LogInformation("El usuario {UsuarioId} inició sesión.", usuario.Id);
        return usuario;
    }

    public async Task<Usuario> RegistrarAsync(string nombre, string correo, string contrasenia, CancellationToken cancellationToken = default)
    {
        var nombreLimpio = (nombre ?? string.Empty).Trim();
        var correoNormalizado = ReglasUsuario.NormalizarCorreo(correo);

        if (nombreLimpio.Length == 0)
        {
            throw new ReglaNegocioException("Escribe tu nombre.");
        }

        ReglasUsuario.ValidarContrasenia(contrasenia);

        if (await _usuarios.ExisteCorreoAsync(correoNormalizado, cancellationToken: cancellationToken))
        {
            throw new ReglaNegocioException("Ya existe una cuenta registrada con ese correo.");
        }

        var usuario = new Usuario
        {
            Nombre = nombreLimpio,
            Correo = correoNormalizado,
            FechaRegistro = DateTime.Now
        };
        usuario.Contrasenia = _hasher.HashPassword(usuario, contrasenia);

        // Cada estudiante empieza con una cuenta de efectivo en $0.00 para poder registrar movimientos.
        usuario.Cuentas.Add(new Cuenta
        {
            NombreCuenta = "Efectivo",
            TipoCuenta = TipoCuenta.Efectivo,
            SaldoActual = 0m,
            Moneda = "USD"
        });

        await _usuarios.AgregarAsync(usuario, cancellationToken);

        try
        {
            await _unidadDeTrabajo.GuardarCambiosAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            // Puede pasar si dos personas se registran con el mismo correo al mismo tiempo (índice único).
            _logger.LogError(ex, "No se pudo registrar un usuario nuevo.");
            throw new ReglaNegocioException("No se pudo completar el registro. Revisa los datos e inténtalo de nuevo.");
        }

        _logger.LogInformation("Se registró el usuario {UsuarioId}.", usuario.Id);
        return usuario;
    }
}
