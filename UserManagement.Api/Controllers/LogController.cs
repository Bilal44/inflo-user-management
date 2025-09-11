using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using UserManagement.Services.Domain.Models;
using UserManagement.Services.Filters;
using UserManagement.Services.Interfaces;
using UserManagement.Services.Mapping;
using static Microsoft.AspNetCore.Http.StatusCodes;

namespace UserManagement.Api.Controllers
{
    [Produces(MediaTypeNames.Application.Json)]
    [Route("api/logs")]
    [ServiceFilter(typeof(ApiExceptionFilter))]
    [EnableRateLimiting("sliding")]
    [ApiController]
    public class LogController(ILogService logService) : ControllerBase
    {
        /// <summary>
        /// Returns a paginated list of logs, supports filtering and sorting.
        /// </summary>
        /// <param name="search">Search term that partially matches the log message.</param>
        /// <param name="from">Start timestamp of the logs.</param>
        /// <param name="to">End timestamps of the logs.</param>
        /// <param name="userId">Filter the logs by a specified user id.</param>
        /// <param name="page">Current page number.</param>
        /// <param name="limit">Number of log entries displayed per page.</param>
        /// <param name="sort">The sorting method applied on the return logs.</param>
        /// <remarks>The default sorting is `timestamp_desc`, which returns logs in descending
        /// timestamp order. Other available methods are `timestamp`, `id`, `id_desc`, `user`,
        /// `user_desc`, `action` and `action_desc`.
        /// The limit (or page size) is always a multiple of 5. Values not divisible by 5 will
        /// be rounded to the nearest multiple. Valid limits are: 5, 10, 15, and 20 (max).</remarks>
        /// <returns>A sorted, filtered and paginated list of logs.</returns>
        [ProducesResponseType(typeof(List<LogModel>), Status200OK)]
        [ProducesResponseType(Status401Unauthorized)]
        [ProducesResponseType(Status500InternalServerError)]
        [HttpGet]
        public async Task<IActionResult> GetLogs(
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
                return BadRequest("The `from` timestamp should not be after `to` timestamp.");

            if (from.HasValue && DateTime.UtcNow < from)
                return BadRequest("The `from` timestamp is in the future, it may cause no data to return.");

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

            return Ok(LogMapper.MapToPaginatedLogModelList(filter, logs.TotalPages, logs.Logs));
        }

        /// <summary>
        /// Returns a single log entry by its unique ID.
        /// </summary>
        /// <param name="id">The id of the log entry.</param>
        /// <returns>The log entry matching the provided ID.</returns>
        [ProducesResponseType(typeof(LogModel), Status200OK)]
        [ProducesResponseType(Status401Unauthorized)]
        [ProducesResponseType(Status404NotFound)]
        [ProducesResponseType(Status500InternalServerError)]
        [HttpGet("{id:long}")]
        public async Task<IActionResult> GetLog(long? id)
        {
            if (id is null)
                return NotFound();

            var log = await logService.GetByIdAsync(id.Value);
            if (log is null)
                return NotFound();

            return Ok(LogMapper.MapToLogModel(log));
        }
    }
}
