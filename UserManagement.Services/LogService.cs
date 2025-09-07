using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using UserManagement.Data.Entities;
using UserManagement.Data.Repositories.Interfaces;
using UserManagement.Services.Domain.Models;
using UserManagement.Services.Interfaces;

namespace UserManagement.Services;

public class LogService(IRepository<Log> repository) : ILogService
{
    public async Task<(List<Log> Logs, int TotalPages)> GetPaginatedResultsAsync(PaginationFilter filter)
    {
        Expression<Func<Log, bool>> predicate = _ => true;
        if (!string.IsNullOrWhiteSpace(filter.Search))
            predicate = CombinePredicates(
                predicate,
                l => l.Details != null && l.Details.ToLower().Contains(filter.Search.Trim().ToLower()));

        if (filter.From.HasValue)
            predicate = CombinePredicates(predicate, l => l.Timestamp >= filter.From.Value);

        if (filter.To.HasValue)
            predicate = CombinePredicates(predicate, l => l.Timestamp <= filter.To.Value);

        if (filter.UserId.HasValue)
            predicate = CombinePredicates(predicate, l => l.UserId == filter.UserId);

        var descending = filter.SortBy!.Split('_').Last() is "desc";

        return await repository.GetPaginatedResultsAsync(
            predicate,
            GetSortBy(filter.SortBy!),
            descending,
            filter.CurrentPage,
            filter.PageSize);
    }

    public Task<Log?> GetByIdAsync(long id) =>
        repository.GetByIdAsync(id);

    public Task AddAsync(Log log) =>
        repository.CreateAsync(log);

    private static Expression<Func<T, bool>> CombinePredicates<T>(
        Expression<Func<T, bool>> first,
        Expression<Func<T, bool>> second)
    {
        var parameter = Expression.Parameter(typeof(T));

        var body = Expression.AndAlso(
            Expression.Invoke(first, parameter),
            Expression.Invoke(second, parameter));

        return Expression.Lambda<Func<T, bool>>(body, parameter);
    }

    private static Expression<Func<Log, object>> GetSortBy(string sort)
        => sort switch
        {
            "id" or "id_desc" => log => log.Id,
            "user" or "user_desc" => log => log.UserId,
            "timestamp" or "timestamp_desc" => log => log.Timestamp,
            "action" or "action_desc" => log => log.ActionType,
            _ => log => log.Timestamp
        };
}
