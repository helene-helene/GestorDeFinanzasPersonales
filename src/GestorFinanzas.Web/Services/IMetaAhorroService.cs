using GestorFinanzas.Web.Models.Resumenes;

namespace GestorFinanzas.Web.Services;

public interface IMetaAhorroService
{
    Task<IReadOnlyList<AvanceMeta>> ListarAsync();

    Task<AvanceMeta> CrearAsync(MetaDatos datos);

    Task<AvanceMeta> AportarAsync(int id, decimal monto);

    Task EliminarAsync(int id);
}

// Datos para crear una meta de ahorro.
public class MetaDatos
{
    public string Nombre { get; set; } = string.Empty;
    public decimal MontoObjetivo { get; set; }
    public decimal MontoActual { get; set; }
    public DateTime? FechaLimite { get; set; }
}
