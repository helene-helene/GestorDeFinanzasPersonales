using System.ComponentModel.DataAnnotations;

namespace GestorFinanzas.Web.ViewModels;

// Datos del formulario de inicio de sesión.
public class LoginViewModel
{
    [Required(ErrorMessage = "Escribe tu correo.")]
    [EmailAddress(ErrorMessage = "El correo no tiene un formato válido.")]
    [StringLength(150, ErrorMessage = "El correo no puede pasar de 150 caracteres.")]
    [Display(Name = "Correo electrónico")]
    public string Correo { get; set; } = string.Empty;

    [Required(ErrorMessage = "Escribe tu contraseña.")]
    [DataType(DataType.Password)]
    [Display(Name = "Contraseña")]
    public string Contrasenia { get; set; } = string.Empty;

    [Display(Name = "Mantener la sesión iniciada")]
    public bool Recordarme { get; set; }

    // Página a la que se vuelve después de iniciar sesión (solo si es del mismo sitio).
    public string? ReturnUrl { get; set; }
}
