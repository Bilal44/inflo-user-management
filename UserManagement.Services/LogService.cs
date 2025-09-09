using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using UserManagement.Data.Entities;
using UserManagement.Data.Repositories.Interfaces;
using UserManagement.Services.Domain.Models;
using UserManagement.Services.Exceptions;
using UserManagement.Services.Interfaces;

namespace UserManagement.Services;

public class LogService(
    IRepository<Log> repository,
    ILogger<LogService> logger) : ILogService
{
    private const string GenericErrorMessage = "An unexpected error occurred, please try again. If the problem persists, please contact our support team.";
    private const int DefaultPageSize = 20;

    public async Task<(List<Log> Logs, int TotalPages)> GetPaginatedResultsAsync(PaginationFilter filter)
    {
        filter.CurrentPage = Math.Max(filter.CurrentPage, 1);
        var adjustedPageSize = Math.Max((int)Math.Round(filter.PageSize / 5.0) * 5, 5);
        filter.PageSize = Math.Min(adjustedPageSize, DefaultPageSize);

        try
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
        catch (Exception e)
        {
            logger.LogError(e,
                "Encountered an error while retrieving log entries");
            throw new ApiException(HttpStatusCode.InternalServerError, GenericErrorMessage);
        }
    }

    public async Task<Log?> GetByIdAsync(long id)
    {
        try
        {
            return await repository.GetByIdAsync(id);
        }
        catch (Exception e)
        {
            logger.LogError(e,
                "Encountered an error while retrieving log entry for id [{LogId}]", id);
            throw new ApiException(HttpStatusCode.InternalServerError, GenericErrorMessage);
        }
    }

    public async Task AddAsync(Log log)
    {
        try
        {
            await repository.CreateAsync(log);
        }
        catch (Exception e)
        {
            logger.LogError(e,
                "Encountered an error while adding a new log entry for user id [{UserId}]", log.UserId);
            throw new ApiException(HttpStatusCode.InternalServerError, GenericErrorMessage);
        }
    }

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
