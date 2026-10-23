using System.ComponentModel.DataAnnotations;

namespace GestorFinanzas.Web.ViewModels;

// Datos del formulario para crear una meta de ahorro.
public class MetaFormViewModel
{
    [Required(ErrorMessage = "Escribe el nombre de la meta.")]
    [StringLength(100, ErrorMessage = "El nombre no puede pasar de 100 caracteres.")]
    [Display(Name = "Nombre de la meta")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ingresa el monto objetivo.")]
    [Range(typeof(decimal), "0.01", "9999999999.99", ErrorMessage = "El monto objetivo debe ser mayor que 0.")]
    [Display(Name = "Monto objetivo")]
    public decimal? MontoObjetivo { get; set; }

    [Range(typeof(decimal), "0", "9999999999.99", ErrorMessage = "Lo ahorrado no puede ser negativo.")]
    [Display(Name = "Ahorrado hasta hoy")]
    public decimal? MontoActual { get; set; } = 0m;

    [DataType(DataType.Date)]
    [Display(Name = "Fecha límite")]
    public DateTime? FechaLimite { get; set; }
}
