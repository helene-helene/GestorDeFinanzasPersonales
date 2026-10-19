namespace GestorFinanzas.Web.Data.Repositories;

/// <summary>Confirma en la base de datos todos los cambios pendientes en una sola operación atómica.</summary>
public interface IUnitOfWork
{
    Task<int> GuardarCambiosAsync(CancellationToken cancellationToken = default);
}
