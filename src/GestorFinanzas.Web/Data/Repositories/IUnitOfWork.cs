namespace GestorFinanzas.Web.Data.Repositories;

// Confirma en la base de datos todos los cambios pendientes en una sola operación atómica.
public interface IUnitOfWork
{
    Task<int> GuardarCambiosAsync(CancellationToken cancellationToken = default);
}
