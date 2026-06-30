using System.Linq.Expressions;
using Horacio.Application.Common.Interfaces;
using Horacio.Domain.Common;
using Horacio.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Horacio.Persistence.Repositories;

/// <summary>
/// Implementación genérica del Repository Pattern sobre EF Core.
/// </summary>
public class GenericRepository<T> : IGenericRepository<T> where T : BaseEntity
{
    protected readonly AppDbContext Context;
    protected readonly DbSet<T> DbSet;

    public GenericRepository(AppDbContext context)
    {
        Context = context;
        DbSet = context.Set<T>();
    }

    public async Task<T?> GetByIdAsync(int id, CancellationToken ct = default)
        => await DbSet.FindAsync(new object?[] { id }, ct);

    public async Task<IReadOnlyList<T>> ListAllAsync(CancellationToken ct = default)
        => await DbSet.AsNoTracking().ToListAsync(ct);

    public async Task<IReadOnlyList<T>> ListAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default)
        => await DbSet.AsNoTracking().Where(predicate).ToListAsync(ct);

    public async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default)
        => await DbSet.FirstOrDefaultAsync(predicate, ct);

    public async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default)
        => await DbSet.AnyAsync(predicate, ct);

    public async Task<int> CountAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default)
        => await DbSet.CountAsync(predicate, ct);

    public async Task AddAsync(T entity, CancellationToken ct = default)
        => await DbSet.AddAsync(entity, ct);

    public void Update(T entity) => DbSet.Update(entity);

    public void Remove(T entity) => DbSet.Remove(entity);

    public IQueryable<T> Query() => DbSet.AsQueryable();
}
