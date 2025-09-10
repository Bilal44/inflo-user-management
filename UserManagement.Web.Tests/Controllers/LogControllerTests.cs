using System.Linq;

namespace UserManagement.Web.Tests.Controllers;

public class LogControllerTests
{
    private readonly ILogService _logService = A.Fake<ILogService>();
    private readonly LogController _controller;

    public LogControllerTests()
    {
        _controller = new LogController(_logService);
        _controller.TempData = new TempDataDictionary(
            new DefaultHttpContext(),
            A.Fake<ITempDataProvider>());
    }

    [Fact]
    public async Task List_ShouldReturnViewWithMappedModel()
    {
        // Arrange
        var logs = (new List<Log> { new() { Id = 1, Details = "Test log" } }, 1);

        A.CallTo(() => _logService.GetPaginatedResultsAsync(A<PaginationFilter>._))
            .Returns(logs);

        // Act
        var result = await _controller.List(null, null, null, null);

        // Assert
        var viewResult = result as ViewResult;
        viewResult.Should().NotBeNull();
        viewResult.Model.Should().BeOfType<PaginatedList<LogModel>>();
        ((PaginatedList<LogModel>)viewResult.Model!).Data.Should().ContainSingle(l => l.Id == 1);
    }

    [Fact]
    public async Task List_WithDefaultParameters_ShouldReturnViewWithPaginatedLogs()
    {
        // Arrange
        var logs = new List<Log>
        {
            new() { Id = 1, Details = "Test log 1" },
            new() { Id = 2, Details = "Test log 2" }
        };
        var paginatedResult = (logs, 1);

        A.CallTo(() => _logService.GetPaginatedResultsAsync(A<PaginationFilter>.That.Matches(f =>
                f.CurrentPage == 1 &&
                f.PageSize == 10 &&
                f.SortBy == "timestamp_desc" &&
                f.Search == null &&
                f.From == null &&
                f.To == null &&
                f.UserId == null)))
            .Returns(paginatedResult);

        // Act
        var result = await _controller.List(null, null, null, null);

        // Assert
        var viewResult = result as ViewResult;
        viewResult.Should().NotBeNull();
        viewResult.Model.Should().BeOfType<PaginatedList<LogModel>>();

        var model = (PaginatedList<LogModel>)viewResult.Model!;
        model.Data.Should().HaveCount(2);
        model.Data.Should().Contain(l => l.Id == 1);
        model.Data.Should().Contain(l => l.Id == 2);
    }

    [Fact]
    public async Task List_WithAllFiltersAndCustomPagination_ShouldReturnCorrectlyPaginatedAndSortedLogs()
    {
        // Arrange
        var search = "critical error";
        var from = DateTime.UtcNow.AddDays(-30);
        var to = DateTime.UtcNow.AddDays(-1);
        var userId = 456L;
        var page = 3;
        var limit = 15;
        var sort = "id";

        var logs = new List<Log>
        {
            new() { Id = 10, Details = "Critical error in authentication", UserId = userId, Timestamp = from.AddDays(5) },
            new() { Id = 11, Details = "Critical error in database", UserId = userId, Timestamp = from.AddDays(10) },
            new() { Id = 12, Details = "Critical error in payment", UserId = userId, Timestamp = from.AddDays(15) }
        };
        var paginatedResult = (logs, 5);

        A.CallTo(() => _logService.GetPaginatedResultsAsync(A<PaginationFilter>.That.Matches(f =>
            f.Search == search &&
            f.From == from &&
            f.To == to &&
            f.UserId == userId &&
            f.CurrentPage == page &&
            f.PageSize == limit &&
            f.SortBy == sort)))
            .Returns(paginatedResult);

        // Act
        var result = await _controller.List(search, from, to, userId, page, limit, sort);

        // Assert
        var viewResult = result as ViewResult;
        viewResult.Should().NotBeNull();
        viewResult.Model.Should().BeOfType<PaginatedList<LogModel>>();

        var model = (PaginatedList<LogModel>)viewResult.Model!;
        model.Data.Should().HaveCount(3);
        model.PaginationFilter.CurrentPage.Should().Be(page);
        model.PaginationFilter.PageSize.Should().Be(limit);
        model.PaginationFilter.SortBy.Should().Be(sort);
        model.TotalPages.Should().Be(5);
        model.Data.Should().OnlyContain(l => l.UserId == userId);
        model.Data.Should().OnlyContain(l => l.Details!.Contains("Critical error"));
    }

    [Fact]
    public async Task List_WithExactTimestampBoundaries_ShouldReturnCorrectlyFilteredLogs()
    {
        // Arrange
        var exactTime = DateTime.UtcNow.Date.AddHours(12);
        var from = exactTime;
        var to = exactTime;
        var logs = new List<Log>
        {
            new() { Id = 1, Details = "Exact timestamp log", Timestamp = exactTime }
        };
        var paginatedResult = (logs, 1);

        A.CallTo(() => _logService.GetPaginatedResultsAsync(A<PaginationFilter>.That.Matches(f =>
            f.From == from &&
            f.To == to)))
            .Returns(paginatedResult);

        // Act
        var result = await _controller.List(null, from, to, null);

        // Assert
        var viewResult = result as ViewResult;
        viewResult.Should().NotBeNull();
        viewResult.Model.Should().BeOfType<PaginatedList<LogModel>>();

        var model = (PaginatedList<LogModel>)viewResult.Model!;
        model.Data.Should().HaveCount(1);
        model.Data.Should().Contain(l => l.Id == 1);
    }

    [Fact]
    public async Task List_WithFiltersReturningNoResults_ShouldReturnViewWithEmptyLogList()
    {
        // Arrange
        var search = "nonexistent";
        var from = DateTime.UtcNow.AddDays(-7);
        var to = DateTime.UtcNow;
        var emptyLogs = (new List<Log>(), 0);

        A.CallTo(() => _logService.GetPaginatedResultsAsync(A<PaginationFilter>._))
            .Returns(emptyLogs);

        // Act
        var result = await _controller.List(search, from, to, null);

        // Assert
        var viewResult = result as ViewResult;
        viewResult.Should().NotBeNull();
        viewResult.Model.Should().BeOfType<PaginatedList<LogModel>>();

        var model = (PaginatedList<LogModel>)viewResult.Model!;
        model.Data.Should().BeEmpty();
        model.TotalPages.Should().Be(0);
    }

    [Fact]
    public async Task List_WhenFromIsInFuture_ShouldSetWarningMessage()
    {
        // Arrange
        var from = DateTime.UtcNow.AddDays(1);
        var to = from.AddDays(1);
        var logs = (new List<Log>(), 1);

        A.CallTo(() => _logService.GetPaginatedResultsAsync(A<PaginationFilter>._))
            .Returns(logs);

        // Act
        var result = await _controller.List(null, from, to, null);

        // Assert
        _controller.TempData["warning"].Should().Be("The <b>from</b> timestamp is in the future, it may cause no data to return.");
        result.Should().BeOfType<ViewResult>();
    }

    [Fact]
    public async Task List_WhenFromIsAfterTo_ShouldSetErrorMessage()
    {
        // Arrange
        var from = DateTime.UtcNow;
        var to = from.AddDays(-1);
        var logs = (new List<Log>(), 1);

        A.CallTo(() => _logService.GetPaginatedResultsAsync(A<PaginationFilter>._))
            .Returns(logs);

        // Act
        var result = await _controller.List(null, from, to, null);

        // Assert
        _controller.TempData["error"].Should().Be("The <b>from</b> timestamp should not be after <b>to</b> timestamp.");
        result.Should().BeOfType<ViewResult>();
    }

    [Fact]
    public async Task List_WhenUnexpectedExceptionOccurs_ShouldHandleGracefully()
    {
        // Arrange
        A.CallTo(() => _logService.GetPaginatedResultsAsync(A<PaginationFilter>._))
            .Throws<Exception>();

        // Act
        var result = await _controller.List("",null,null,null,0,0, "");

        // Assert
        result.Should().BeOfType<RedirectToActionResult>()
            .Which.ActionName.Should().Be(nameof(_controller.List));
        _controller.TempData["error"].As<string>()
            .Should().Contain("unexpected error");
        A.CallTo(() => _logService.GetPaginatedResultsAsync(A<PaginationFilter>._))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task View_WithValidId_ReturnsLogDetails()
    {
        // Arrange
        var logId = 5L;
        var log = new Log { Id = logId, Details = "Detailed log entry", Timestamp = DateTime.UtcNow };
        A.CallTo(() => _logService.GetByIdAsync(logId)).Returns(log);

        // Act
        var result = await _controller.View(logId);

        // Assert
        var viewResult = result as ViewResult;
        viewResult.Should().NotBeNull();
        viewResult.Model.Should().BeOfType<LogModel>();

        var model = (LogModel)viewResult.Model!;
        model.Id.Should().Be(logId);
        model.Details.Should().Be("Detailed log entry");
    }

    [Fact]
    public async Task View_WhenIdIsNull_ShouldReturnNotFound()
    {
        // Act
        var result = await _controller.View(null);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task View_WhenLogNotFound_ShouldReturnNotFound()
    {
        // Arrange
        A.CallTo(() => _logService.GetByIdAsync(99)).Returns<Log?>(null);

        // Act
        var result = await _controller.View(99);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task View_WithNonExistentId_ReturnsNotFound()
    {
        // Arrange
        var nonExistentId = 999L;
        A.CallTo(() => _logService.GetByIdAsync(nonExistentId)).Returns<Log?>(null);

        // Act
        var result = await _controller.View(nonExistentId);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
        A.CallTo(() => _logService.GetByIdAsync(nonExistentId)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task View_WhenUnexpectedExceptionOccurs_ShouldHandleGracefully()
    {
        // Arrange
        A.CallTo(() => _logService.GetByIdAsync(1L))
            .Throws<Exception>();

        // Act
        var result = await _controller.View(1L);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>()
            .Which.ActionName.Should().Be(nameof(_controller.List));
        _controller.TempData["error"].As<string>()
            .Should().Contain("unexpected error");
        A.CallTo(() => _logService.GetByIdAsync(1L))
            .MustHaveHappenedOnceExactly();
    }
}
