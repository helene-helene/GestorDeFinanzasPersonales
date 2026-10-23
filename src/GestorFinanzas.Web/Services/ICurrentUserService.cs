namespace GestorFinanzas.Web.Services;

// Entrega el Id del usuario que está usando la aplicación en este momento.
public interface ICurrentUserService
{
    int UsuarioId { get; }
}
