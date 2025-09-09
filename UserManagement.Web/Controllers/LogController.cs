using System;
using System.Threading.Tasks;
using UserManagement.Services.Domain.Models;
using UserManagement.Services.Interfaces;
using UserManagement.Services.Mapping;

namespace UserManagement.Web.Controllers
{
    public class LogController(ILogService logService) : Controller
    {
        // Return a filtered and paginated list of log entries.
        public async Task<IActionResult> List(
            string? search,
            DateTime? from,
            DateTime? to,
            long? userId,
            int page = 1,
            int limit = 10,
            string sort = "timestamp_desc"
        )
        {
            if (from > to)
                TempData["error"] = "The `<b>from</b> timestamp should not be after <b>to</b> timestamp.";

            if (from.HasValue && DateTime.UtcNow < from)
                TempData["warning"] = "The <b>from</b> timestamp is in the future, it may cause no data to return.";

            var filter = new PaginationFilter
            {
                UserId = userId,
                Search = search,
                From = from,
                To = to,
                CurrentPage = page,
                PageSize = limit,
                SortBy = sort
            };
            var logs = await logService.GetPaginatedResultsAsync(filter);

            return View(LogMapper.MapToPaginatedLogModelList(filter, logs.TotalPages, logs.Logs));
        }

        // Return a single log entry associated with the provided id.
        public async Task<IActionResult> View(long? id)
        {
            if (id is null)
                return NotFound();

            var log = await logService.GetByIdAsync(id.Value);
            if (log is null)
                return NotFound();

            return View(LogMapper.MapToLogModel(log));
        }
    }
}
