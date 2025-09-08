using Microsoft.AspNetCore.Mvc;
using UserManagement.Api.Controllers;
using UserManagement.Data.Entities;
using UserManagement.Services.Domain.Models;
using UserManagement.Services.Interfaces;

namespace UserManagement.Api.Tests.Controllers;

public class LogControllerTests
{
    private readonly ILogService _logService = A.Fake<ILogService>();
    private readonly LogController _controller;

    public LogControllerTests()
    {
        _controller = new LogController(_logService);
    }

    [Fact]
    public async Task GetLogs_WithDefaultParameters_ReturnsPaginatedLogsList()
    {
        // Arrange
        var logsWithTotalPages = (
            new List<Log> { new() { Id = 1, Details = "Message", Timestamp = DateTime.UtcNow } },
            1);

        A.CallTo(() => _logService.GetPaginatedResultsAsync(A<PaginationFilter>._)).Returns(logsWithTotalPages);

        // Act
        var result = await _controller.GetLogs(null, null, null, null, 1, 10, "timestamp_desc");

        // Assert
        var okResult = result as OkObjectResult;
        okResult.Should().NotBeNull();
        A.CallTo(() => _logService.GetPaginatedResultsAsync(A<PaginationFilter>.That.Matches(f =>
                f.CurrentPage == 1 && f.PageSize == 10 && f.SortBy == "timestamp_desc")))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task GetLogs_WithSearchAndDateFilters_ReturnsFilteredLogs()
    {
        // Arrange
        var from = DateTime.UtcNow.AddDays(-7);
        var to = DateTime.UtcNow;
        var search = "message";
        var timeStamp = DateTime.UtcNow;
        var logsWithTotalPages = (
            new List<Log> { new() { Id = 1, Details = "Message", Timestamp = timeStamp } },
            1);

        var expectedResult = new PaginatedList<LogModel>
        {
            PaginationFilter =
                new PaginationFilter
                {
                    Search = search,
                    From = from,
                    To = to,
                    SortBy = "timestamp_desc",
                    CurrentPage = 1,
                    PageSize = 10
                },
            Data = [new LogModel { Id = 1, Details = "Message", Timestamp = timeStamp }],
            TotalPages = 1
        };

        A.CallTo(() => _logService.GetPaginatedResultsAsync(A<PaginationFilter>._)).Returns(logsWithTotalPages);

        // Act
        var result = await _controller.GetLogs(search, from, to, null, 1, 10, "timestamp_desc");

        // Assert
        var okResult = result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult.Value.Should().BeOfType<PaginatedList<LogModel>>();
        okResult.Value.Should().BeEquivalentTo(expectedResult);

        A.CallTo(() => _logService.GetPaginatedResultsAsync(A<PaginationFilter>.That.Matches(f =>
                f.Search == search && f.From == from && f.To == to)))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task View_WithValidId_ReturnsLogEntry()
    {
        // Arrange
        var log = new Log { Id = 1, Details = "Message", Timestamp = DateTime.UtcNow };
        A.CallTo(() => _logService.GetByIdAsync(1)).Returns(log);

        // Act
        var result = await _controller.View(1);

        // Assert
        var okResult = result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult.Value.Should().BeOfType<LogModel>();

        A.CallTo(() => _logService.GetByIdAsync(1))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task GetLogs_WithFromAfterTo_ReturnsBadRequest()
    {
        // Arrange
        var from = DateTime.UtcNow;
        var to = DateTime.UtcNow.AddDays(-1);

        // Act
        var result = await _controller.GetLogs(null, from, to, null, 1, 10, "timestamp_desc");

        // Assert
        var badRequestResult = result as BadRequestObjectResult;
        badRequestResult.Should().NotBeNull();
        badRequestResult!.Value.Should().Be("The `from` timestamp should not be after `to` timestamp.");
    }

    [Fact]
    public async Task GetLogs_WithFutureFromTimestamp_ReturnsBadRequest()
    {
        // Arrange
        var from = DateTime.UtcNow.AddDays(1);

        // Act
        var result = await _controller.GetLogs(null, from, null, null, 1, 10, "timestamp_desc");

        // Assert
        var badRequestResult = result as BadRequestObjectResult;
        badRequestResult.Should().NotBeNull();
        badRequestResult!.Value.Should().Be("The `from` timestamp is in the future, it may cause no data to return.");
    }

    [Fact]
    public async Task View_WithNonExistentId_ReturnsNotFound()
    {
        // Arrange
        A.CallTo(() => _logService.GetByIdAsync(99))
            .Returns<Log?>(null);

        // Act
        var result = await _controller.View(99);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }
}
