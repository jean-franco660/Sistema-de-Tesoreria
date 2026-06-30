using System.Collections.Concurrent;
using Horacio.Application.Common.Interfaces;
using Horacio.Domain.Common;
using Horacio.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Horacio.Persistence.Repositories;

/// <summary>
/// Unit Of Work: comparte el mismo DbContext entre repositorios y centraliza
/// el guardado y las transacciones.
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;
    private readonly ConcurrentDictionary<Type, object> _repositories = new();

    public UnitOfWork(AppDbContext context) => _context = context;

    public IGenericRepository<T> Repository<T>() where T : BaseEntity
        => (IGenericRepository<T>)_repositories.GetOrAdd(
            typeof(T), _ => new GenericRepository<T>(_context));

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
        => _context.SaveChangesAsync(ct);

    public async Task<TResult> ExecuteInTransactionAsync<TResult>(
        Func<CancellationToken, Task<TResult>> action, CancellationToken ct = default)
    {
        // El proveedor InMemory (usado en tests) no soporta transacciones:
        // en ese caso se ejecuta la acción directamente.
        if (!_context.Database.IsRelational())
            return await action(ct);

        await using var tx = await _context.Database.BeginTransactionAsync(ct);
        try
        {
            var result = await action(ct);
            await tx.CommitAsync(ct);
            return result;
        }
        catch
        {
            await tx.RollbackAsync(ct);
            throw;
        }
    }
}
