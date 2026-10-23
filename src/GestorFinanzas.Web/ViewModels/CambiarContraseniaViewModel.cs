using System.ComponentModel.DataAnnotations;

namespace GestorFinanzas.Web.ViewModels;

public class CambiarContraseniaViewModel
{
    [Required(ErrorMessage = "Escribe tu contraseña actual.")]
    [DataType(DataType.Password)]
    [Display(Name = "Contraseña actual")]
    public string ContraseniaActual { get; set; } = string.Empty;

    [Required(ErrorMessage = "Escribe la nueva contraseña.")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "La contraseña debe tener entre 8 y 100 caracteres.")]
    [DataType(DataType.Password)]
    [Display(Name = "Nueva contraseña")]
    public string ContraseniaNueva { get; set; } = string.Empty;

    [Required(ErrorMessage = "Confirma la nueva contraseña.")]
    [Compare(nameof(ContraseniaNueva), ErrorMessage = "Las contraseñas no coinciden.")]
    [DataType(DataType.Password)]
    [Display(Name = "Confirmar nueva contraseña")]
    public string ConfirmarContrasenia { get; set; } = string.Empty;
}
