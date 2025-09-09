using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using UserManagement.Data.Context;
using UserManagement.Data.Repositories.Interfaces;

namespace UserManagement.Data.Repositories;

public class Repository<TEntity>(UserManagementDbContext dbContext) : IRepository<TEntity>
    where TEntity : class
{
    public async Task<List<TEntity>> GetAllAsync(
        Expression<Func<TEntity, bool>>? predicate,
        CancellationToken cancellationToken
    )
    {
        var query = dbContext.Set<TEntity>().AsQueryable();
        if (predicate is not null)
            query = query.Where(predicate);

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<(List<TEntity> Results, int TotalPages)> GetPaginatedResultsAsync(
        Expression<Func<TEntity, bool>>? predicate,
        Expression<Func<TEntity, object>>? orderBy,
        bool descending,
        int currentPage,
        int pageSize
    )
    {
        var query = dbContext.Set<TEntity>().AsQueryable();
        if (predicate is not null)
            query = query.Where(predicate);

        if (orderBy is not null)
            query = descending ? query.OrderByDescending(orderBy) : query.OrderBy(orderBy);

        var totalRecords = await query.CountAsync();

        var totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);
        var queryResults = await query
            .Skip((currentPage - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (queryResults, totalPages);
    }

    public async Task<TEntity?> GetByIdAsync(object id)
        => await dbContext.Set<TEntity>().FindAsync(id);

    public async Task CreateAsync(TEntity entity)
    {
        await dbContext.AddAsync(entity);
        await dbContext.SaveChangesAsync();
    }

    public async Task UpdateAsync(TEntity entity)
    {
        dbContext.Update(entity);
        await dbContext.SaveChangesAsync();
    }

    public async Task<int> DeleteAsync(object id)
        => await dbContext.Set<TEntity>()
            .Where(e => EF.Property<object>(e, "Id") == id)
            .ExecuteDeleteAsync();
}
