using Horacio.Domain.Common;

namespace Horacio.Application.Common.Interfaces;

/// <summary>
/// Unidad de trabajo (Unit Of Work): coordina repositorios y persiste cambios
/// en una sola transacción lógica.
/// </summary>
public interface IUnitOfWork
{
    IGenericRepository<T> Repository<T>() where T : BaseEntity;

    Task<int> SaveChangesAsync(CancellationToken ct = default);

    /// <summary>Ejecuta una operación dentro de una transacción de base de datos.</summary>
    Task<TResult> ExecuteInTransactionAsync<TResult>(
        Func<CancellationToken, Task<TResult>> action, CancellationToken ct = default);
}
