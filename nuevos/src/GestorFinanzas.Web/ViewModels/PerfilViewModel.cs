using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace GestorFinanzas.Web.ViewModels;

/// <summary>
/// Datos de la pantalla "Mi perfil". A propósito no tiene Id ni contraseña: el usuario
/// solo puede cambiar su nombre y su correo, y siempre los suyos.
/// </summary>
public class PerfilViewModel
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

    // Solo se muestra; no se recibe desde el formulario.
    [BindNever]
    [Display(Name = "Miembro desde")]
    public DateTime FechaRegistro { get; set; }
}
