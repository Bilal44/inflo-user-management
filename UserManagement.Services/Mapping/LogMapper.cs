using System.Collections.Generic;
using System.Linq;
using UserManagement.Data.Entities;
using UserManagement.Services.Domain.Models;

namespace UserManagement.Services.Mapping;

public static class LogMapper
{
    public static LogModel MapToLogModel(Log log)
        => new LogModel
        {
            Id = log.Id,
            UserId = log.UserId,
            Timestamp = log.Timestamp,
            ActionType = log.ActionType,
            Details = log.Details
        };

    public static PaginatedList<LogModel> MapToPaginatedLogModelList(
        PaginationFilter filter,
        int totalPages,
        List<Log> logs)
        => new PaginatedList<LogModel>
        {
            PaginationFilter = filter,
            TotalPages = totalPages,
            Data = logs.Select(l =>
                new LogModel
                {
                    Id = l.Id,
                    UserId = l.UserId,
                    Timestamp = l.Timestamp,
                    ActionType = l.ActionType,
                    Details = l.Details
                })
            .ToList()
        };

    public static Log MapToLogEntity(LogModel model)
        => new Log
        {
            Id = model.Id,
            UserId = model.UserId,
            Timestamp = model.Timestamp,
            ActionType = model.ActionType,
            Details = model.Details,
        };
}
