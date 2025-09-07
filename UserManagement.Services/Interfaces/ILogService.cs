using System.Collections.Generic;
using System.Threading.Tasks;
using UserManagement.Data.Entities;
using UserManagement.Services.Domain.Models;

namespace UserManagement.Services.Interfaces;

public interface ILogService
{
    Task<(List<Log> Logs, int TotalPages)> GetPaginatedResultsAsync(PaginationFilter filter);
    Task<Log?> GetByIdAsync(long id);
    Task AddAsync(Log log);
}
