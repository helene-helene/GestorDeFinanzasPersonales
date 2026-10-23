using GestorFinanzas.Web.Models.Entities;
using GestorFinanzas.Web.Models.Resumenes;

namespace GestorFinanzas.Web.Services;

public interface IPresupuestoService
{
    // Presupuestos vigentes hoy con lo gastado en cada uno.
    Task<ResumenPresupuesto> ObtenerResumenAsync();
    Task<IReadOnlyList<Categoria>> ListarCategoriasDeGastoAsync();

    Task<Presupuesto> CrearAsync(PresupuestoDatos datos);

    Task EliminarAsync(int id);
}

// Datos para crear un presupuesto. Las fechas las calcula el servicio según el período.
public class PresupuestoDatos
{
    public int CategoriaId { get; set; }
    public decimal MontoLimite { get; set; }
    public string Periodo { get; set; } = string.Empty;
}
