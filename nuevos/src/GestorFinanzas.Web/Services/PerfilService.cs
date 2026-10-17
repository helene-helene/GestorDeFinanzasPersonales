using GestorFinanzas.Web.Data.Repositories;
using GestorFinanzas.Web.Infrastructure;
using GestorFinanzas.Web.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace GestorFinanzas.Web.Services;

public class PerfilService : IPerfilService
{
    private const int LongitudMaximaNombre = 100;
    private const int LongitudMaximaCorreo = 150;

    private readonly IUsuarioRepository _usuarios;
    private readonly IUnitOfWork _unidadDeTrabajo;
    private readonly IPasswordHasher<Usuario> _hasher;
    private readonly ICurrentUserService _usuarioActual;
    private readonly ILogger<PerfilService> _logger;

    public PerfilService(
        IUsuarioRepository usuarios,
        IUnitOfWork unidadDeTrabajo,
        IPasswordHasher<Usuario> hasher,
        ICurrentUserService usuarioActual,
        ILogger<PerfilService> logger)
    {
        _usuarios = usuarios;
        _unidadDeTrabajo = unidadDeTrabajo;
        _hasher = hasher;
        _usuarioActual = usuarioActual;
        _logger = logger;
    }

    public Task<Usuario?> ObtenerAsync(CancellationToken cancellationToken = default) =>
        _usuarios.ObtenerPorIdAsync(_usuarioActual.UsuarioId, cancellationToken: cancellationToken);

    public async Task<Usuario> ActualizarAsync(string nombre, string correo, CancellationToken cancellationToken = default)
    {
        var nombreLimpio = (nombre ?? string.Empty).Trim();
        var correoNormalizado = ReglasUsuario.NormalizarCorreo(correo);

        if (nombreLimpio.Length == 0 || nombreLimpio.Length > LongitudMaximaNombre)
        {
            throw new ReglaNegocioException($"El nombre es obligatorio y no puede pasar de {LongitudMaximaNombre} caracteres.");
        }

        if (correoNormalizado.Length == 0 || correoNormalizado.Length > LongitudMaximaCorreo)
        {
            throw new ReglaNegocioException($"El correo es obligatorio y no puede pasar de {LongitudMaximaCorreo} caracteres.");
        }

        var usuarioId = _usuarioActual.UsuarioId;
        var usuario = await ObtenerParaModificarAsync(usuarioId, cancellationToken);

        if (await _usuarios.ExisteCorreoAsync(correoNormalizado, usuarioId, cancellationToken))
        {
            throw new ReglaNegocioException("Ese correo ya está registrado por otro usuario.");
        }

        usuario.Nombre = nombreLimpio;
        usuario.Correo = correoNormalizado;

        await GuardarAsync(cancellationToken);
        _logger.LogInformation("El usuario {UsuarioId} actualizó su perfil.", usuarioId);
        return usuario;
    }

    public async Task CambiarContraseniaAsync(string contraseniaActual, string contraseniaNueva, CancellationToken cancellationToken = default)
    {
        var usuarioId = _usuarioActual.UsuarioId;
        var usuario = await ObtenerParaModificarAsync(usuarioId, cancellationToken);

        var resultado = _hasher.VerifyHashedPassword(usuario, usuario.Contrasenia, contraseniaActual ?? string.Empty);
        if (resultado == PasswordVerificationResult.Failed)
        {
            throw new ReglaNegocioException("La contraseña actual no es correcta.");
        }

        ReglasUsuario.ValidarContrasenia(contraseniaNueva);

        if (contraseniaNueva == contraseniaActual)
        {
            throw new ReglaNegocioException("La nueva contraseña debe ser diferente de la actual.");
        }

        usuario.Contrasenia = _hasher.HashPassword(usuario, contraseniaNueva);

        await GuardarAsync(cancellationToken);
        _logger.LogInformation("El usuario {UsuarioId} cambió su contraseña.", usuarioId);
    }

    private async Task<Usuario> ObtenerParaModificarAsync(int usuarioId, CancellationToken cancellationToken) =>
        await _usuarios.ObtenerPorIdAsync(usuarioId, conSeguimiento: true, cancellationToken)
            ?? throw new ReglaNegocioException("No se encontró tu perfil. Vuelve a iniciar sesión.");

    private async Task GuardarAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _unidadDeTrabajo.GuardarCambiosAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            // El detalle técnico va al registro del servidor; al usuario solo se le muestra un mensaje claro.
            _logger.LogError(ex, "Error al guardar el perfil del usuario {UsuarioId}.", _usuarioActual.UsuarioId);
            throw new ReglaNegocioException("No se pudieron guardar los cambios. Inténtalo de nuevo.");
        }
    }
}
