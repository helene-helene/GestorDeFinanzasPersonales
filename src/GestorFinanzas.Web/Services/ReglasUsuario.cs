using GestorFinanzas.Web.Infrastructure;

namespace GestorFinanzas.Web.Services;

// Reglas que comparten el login, el registro y Mi perfil.
public static class ReglasUsuario
{
    public const int LongitudMinimaContrasenia = 8;
    public const int LongitudMaximaContrasenia = 100;

    // Los correos se guardan sin espacios y en minúsculas, así "Ana@UDB.edu.sv" y "ana@udb.edu.sv"
    // se tratan como el mismo usuario.
    public static string NormalizarCorreo(string? correo) =>
        (correo ?? string.Empty).Trim().ToLowerInvariant();

    // La contraseña debe tener al menos 8 caracteres, una letra y un número.
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
