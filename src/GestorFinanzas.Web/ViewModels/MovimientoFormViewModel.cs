using System.ComponentModel.DataAnnotations;
using GestorFinanzas.Web.Models;
using GestorFinanzas.Web.Models.Entities;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace GestorFinanzas.Web.ViewModels;

// Datos del formulario para registrar o editar un movimiento.
public class MovimientoFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Indica si el movimiento es un ingreso o un gasto.")]
    [Display(Name = "Tipo")]
    public string Tipo { get; set; } = TipoMovimiento.Gasto;

    [Required(ErrorMessage = "Selecciona una cuenta.")]
    [Display(Name = "Cuenta")]
    public int? CuentaId { get; set; }

    [Required(ErrorMessage = "Selecciona una categoría.")]
    [Display(Name = "Categoría")]
    public int? CategoriaId { get; set; }

    [Required(ErrorMessage = "Ingresa el monto.")]
    [Range(typeof(decimal), "0.01", "9999999999.99", ErrorMessage = "El monto debe ser mayor que 0.")]
    [Display(Name = "Monto (USD)")]
    public decimal? Monto { get; set; }

    [Required(ErrorMessage = "Selecciona la fecha del movimiento.")]
    [DataType(DataType.Date)]
    [Display(Name = "Fecha")]
    public DateTime? Fecha { get; set; }

    [StringLength(200, ErrorMessage = "La descripción no puede pasar de 200 caracteres.")]
    [Display(Name = "Descripción (opcional)")]
    public string? Descripcion { get; set; }

    // Listas para los selectores: las llena el controlador, no vienen del formulario.
    [BindNever, ValidateNever]
    public IReadOnlyList<Cuenta> Cuentas { get; set; } = Array.Empty<Cuenta>();

    [BindNever, ValidateNever]
    public IReadOnlyList<Categoria> Categorias { get; set; } = Array.Empty<Categoria>();
}
