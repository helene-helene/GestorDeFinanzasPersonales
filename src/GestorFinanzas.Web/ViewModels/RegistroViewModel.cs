using System.ComponentModel.DataAnnotations;

namespace GestorFinanzas.Web.ViewModels;

// Datos del formulario para crear una cuenta nueva.
public class RegistroViewModel
{
    [Required(ErrorMessage = "Escribe tu nombre.")]
    [StringLength(100, ErrorMessage = "El nombre no puede pasar de 100 caracteres.")]
    [Display(Name = "Nombre completo")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "Escribe tu correo.")]
    [EmailAddress(ErrorMessage = "El correo no tiene un formato válido.")]
    [StringLength(150, ErrorMessage = "El correo no puede pasar de 150 caracteres.")]
    [Display(Name = "Correo electrónico")]
    public string Correo { get; set; } = string.Empty;

    [Required(ErrorMessage = "Escribe una contraseña.")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "La contraseña debe tener entre 8 y 100 caracteres.")]
    [DataType(DataType.Password)]
    [Display(Name = "Contraseña")]
    public string Contrasenia { get; set; } = string.Empty;

    [Required(ErrorMessage = "Confirma la contraseña.")]
    [Compare(nameof(Contrasenia), ErrorMessage = "Las contraseñas no coinciden.")]
    [DataType(DataType.Password)]
    [Display(Name = "Confirmar contraseña")]
    public string ConfirmarContrasenia { get; set; } = string.Empty;
}
