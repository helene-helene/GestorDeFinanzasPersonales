using System.ComponentModel.DataAnnotations;
using GestorFinanzas.Web.Models;
using GestorFinanzas.Web.Models.Entities;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace GestorFinanzas.Web.ViewModels;

/// <summary>Datos del formulario para crear o editar un presupuesto.</summary>
public class PresupuestoFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Selecciona una categoría.")]
    [Display(Name = "Categoría")]
    public int? CategoriaId { get; set; }

    [Required(ErrorMessage = "Ingresa el monto límite.")]
    [Range(typeof(decimal), "0.01", "9999999999.99", ErrorMessage = "El monto límite debe ser mayor que 0.")]
    [Display(Name = "Monto límite (USD)")]
    public decimal? MontoLimite { get; set; }

    [Required(ErrorMessage = "Selecciona el período.")]
    [Display(Name = "Período")]
    public string Periodo { get; set; } = PeriodoPresupuesto.Mensual;

    [Required(ErrorMessage = "Selecciona la fecha de inicio.")]
    [DataType(DataType.Date)]
    [Display(Name = "Fecha de inicio")]
    public DateTime? FechaInicio { get; set; }

    // la llena el controlador, no viene del formulario
    [BindNever, ValidateNever]
    public IReadOnlyList<Categoria> Categorias { get; set; } = Array.Empty<Categoria>();
}
