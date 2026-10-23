namespace GestorFinanzas.Web.Models.Filtros;

// Una página de resultados junto con los datos necesarios para paginar.
public class ResultadoPaginado<T>
{
    public IReadOnlyList<T> Items { get; init; } = Array.Empty<T>();
    public int Total { get; init; }
    public int Pagina { get; init; }
    public int TamanioPagina { get; init; }

    public int TotalPaginas =>
        TamanioPagina <= 0 ? 0 : (int)Math.Ceiling(Total / (double)TamanioPagina);
}
