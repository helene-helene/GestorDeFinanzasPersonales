namespace GestorFinanzas.Web.Infrastructure;

// Error esperado que el usuario puede corregir (saldo insuficiente, categoría que no coincide, etc.).
// Los controladores lo capturan y muestran el mensaje en pantalla.
public class ReglaNegocioException : Exception
{
    public ReglaNegocioException(string mensaje) : base(mensaje)
    {
    }
}
