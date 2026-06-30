using System.Linq.Expressions;
using Horacio.Domain.Common;

namespace Horacio.Application.Common.Interfaces;

/// <summary>
/// Repositorio genérico (Repository Pattern) para entidades del dominio.
/// </summary>
public interface IGenericRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<T>> ListAllAsync(CancellationToken ct = default);
    Task<IReadOnlyList<T>> ListAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default);
    Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default);
    Task<bool> AnyAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default);
    Task<int> CountAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default);
    Task AddAsync(T entity, CancellationToken ct = default);
    void Update(T entity);
    void Remove(T entity);

    /// <summary>Consulta IQueryable para proyecciones y filtros del lado de lectura (CQRS).</summary>
    IQueryable<T> Query();
}
