namespace GestorFinanzas.Web.Data.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _contexto;

    public UnitOfWork(AppDbContext contexto)
    {
        _contexto = contexto;
    }

    public Task<int> GuardarCambiosAsync(CancellationToken cancellationToken = default) =>
        _contexto.SaveChangesAsync(cancellationToken);
}
