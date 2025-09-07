using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace UserManagement.Data.Repositories.Interfaces;

public interface IRepository<TEntity> where TEntity : class
{
    /// <summary>
    /// Get a list of items
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <returns></returns>
    Task<List<TEntity>> GetAllAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get a paginated list of items
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <returns></returns>
    Task<(List<TEntity> Results, int TotalPages)> GetPaginatedResultsAsync(
        Expression<Func<TEntity, bool>>? predicate,
        Expression<Func<TEntity, object>>? orderBy,
        bool ascending,
        int offset,
        int limit);

    /// <summary>
    ///
    /// </summary>
    /// <param name="id">Primary identification key of the entity</param>
    /// <returns></returns>
    Task<TEntity?> GetByIdAsync(object id);

    /// <summary>
    /// Create a new item
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <param name="entity"></param>
    /// <returns></returns>
    Task CreateAsync(TEntity entity);

    /// <summary>
    /// Update an existing item matching the ID
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <param name="entity"></param>
    /// <returns></returns>
    Task UpdateAsync(TEntity entity);

    Task<int> DeleteAsync(object id);
}
