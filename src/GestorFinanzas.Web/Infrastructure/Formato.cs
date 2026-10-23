namespace GestorFinanzas.Web.Infrastructure;

// Formatos de texto compartidos. La aplicación usa cultura invariante: punto decimal y coma de miles.
public static class Formato
{
    public static string Dinero(decimal monto) =>
        monto < 0 ? "-$" + Math.Abs(monto).ToString("N2") : "$" + monto.ToString("N2");

    public static string Fecha(DateTime fecha) => fecha.ToString("dd/MM/yyyy");

    // Recorta un texto para que quepa en una columna de la base de datos.
    public static string Recortar(string texto, int maximo) =>
        texto.Length <= maximo ? texto : texto[..(maximo - 1)] + "…";
}

// Ayudas para mostrar categorías con el estilo del diseño (iniciales y color).
public static class Presentacion
{
    private static readonly string[] ClasesDeColor = { "food", "transport", "leisure", "books" };

    // Dos letras para el ícono de la categoría: «Materiales de estudio» → «ME».
    public static string Iniciales(string nombre)
    {
        var palabras = nombre.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var importantes = palabras.Where(p => p.Length > 2).ToArray();
        if (importantes.Length >= 2)
        {
            return string.Concat(importantes[0][0], importantes[1][0]).ToUpperInvariant();
        }

        var texto = palabras.Length > 0 ? palabras[0] : "?";
        return texto.Substring(0, Math.Min(2, texto.Length)).ToUpperInvariant();
    }

    public static string ClaseDeColor(int id) => ClasesDeColor[Math.Abs(id) % ClasesDeColor.Length];
}
