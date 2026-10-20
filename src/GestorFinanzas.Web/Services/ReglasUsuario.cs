using GestorFinanzas.Web.Infrastructure;

namespace GestorFinanzas.Web.Services;

/// <summary>Reglas que comparten el login, el registro y Mi perfil.</summary>
public static class ReglasUsuario
{
    public const int LongitudMinimaContrasenia = 8;
    public const int LongitudMaximaContrasenia = 100;

    /// <summary>
    /// Los correos se guardan sin espacios y en minúsculas, así "Ana@UDB.edu.sv" y "ana@udb.edu.sv"
    /// se tratan como el mismo usuario.
    /// </summary>
    public static string NormalizarCorreo(string? correo) =>
        (correo ?? string.Empty).Trim().ToLowerInvariant();

    /// <summary>La contraseña debe tener al menos 8 caracteres, una letra y un número.</summary>
    public static void ValidarContrasenia(string? contrasenia)
    {
        if (string.IsNullOrWhiteSpace(contrasenia) ||
            contrasenia.Length < LongitudMinimaContrasenia ||
            contrasenia.Length > LongitudMaximaContrasenia)
        {
            throw new ReglaNegocioException(
                $"La contraseña debe tener entre {LongitudMinimaContrasenia} y {LongitudMaximaContrasenia} caracteres.");
        }

        if (!contrasenia.Any(char.IsLetter) || !contrasenia.Any(char.IsDigit))
        {
            throw new ReglaNegocioException("La contraseña debe incluir al menos una letra y un número.");
        }
    }
}
