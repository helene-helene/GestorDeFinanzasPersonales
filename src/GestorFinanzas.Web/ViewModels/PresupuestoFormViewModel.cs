using System.ComponentModel.DataAnnotations;
using GestorFinanzas.Web.Models;
using GestorFinanzas.Web.Models.Entities;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace GestorFinanzas.Web.ViewModels;

// Datos del formulario para crear un presupuesto.
public class PresupuestoFormViewModel
{
    [Required(ErrorMessage = "Selecciona una categoría.")]
    [Display(Name = "Categoría")]
    public int? CategoriaId { get; set; }

    [Required(ErrorMessage = "Ingresa el límite.")]
    [Range(typeof(decimal), "0.01", "9999999999.99", ErrorMessage = "El límite debe ser mayor que 0.")]
    [Display(Name = "Límite de gasto")]
    public decimal? MontoLimite { get; set; }

    [Required(ErrorMessage = "Selecciona el período.")]
    [Display(Name = "Período")]
    public string Periodo { get; set; } = PeriodoPresupuesto.Mensual;

    [BindNever, ValidateNever]
    public IReadOnlyList<Categoria> Categorias { get; set; } = Array.Empty<Categoria>();
}
