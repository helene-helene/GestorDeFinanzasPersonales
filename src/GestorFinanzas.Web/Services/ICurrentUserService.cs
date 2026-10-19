namespace GestorFinanzas.Web.Services;

/// <summary>Entrega el Id del usuario que está usando la aplicación en este momento.</summary>
public interface ICurrentUserService
{
    int UsuarioId { get; }
}
