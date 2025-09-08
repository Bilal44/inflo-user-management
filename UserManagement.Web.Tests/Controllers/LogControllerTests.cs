using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using UserManagement.Data.Entities;
using UserManagement.Services.Domain.Models;
using UserManagement.Services.Interfaces;
using UserManagement.Web.Controllers;

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
    public async Task List_WhenFromIsAfterTo_ShouldSetErrorTempData()
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
        _controller.TempData["error"].Should().NotBeNull();
        result.Should().BeOfType<ViewResult>();
    }

    [Fact]
    public async Task List_WhenFromIsInFuture_ShouldSetWarningTempData()
    {
        // Arrange
        var from = DateTime.UtcNow.AddDays(1);
        var to = from.AddDays(1);
        var logs = (new List<Log>(), 1);

        A.CallTo(() => _logService.GetPaginatedResultsAsync(A<PaginationFilter>._))
            .Returns(Task.FromResult(logs));

        // Act
        var result = await _controller.List(null, from, to, null);

        // Assert
        _controller.TempData["warning"].Should().NotBeNull();
        result.Should().BeOfType<ViewResult>();
    }

    [Fact]
    public async Task List_ShouldReturnViewWithMappedModel()
    {
        // Arrange
        var logs = (new List<Log> { new() { Id = 1, Details = "Test log" } }, 1);

        A.CallTo(() => _logService.GetPaginatedResultsAsync(A<PaginationFilter>._))
            .Returns(Task.FromResult(logs));

        // Act
        var result = await _controller.List(null, null, null, null);

        // Assert
        var viewResult = result as ViewResult;
        viewResult.Should().NotBeNull();
        viewResult!.Model.Should().BeOfType<PaginatedList<LogModel>>();
        ((PaginatedList<LogModel>)viewResult.Model!).Data.Should().ContainSingle(l => l.Id == 1);
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
    public async Task View_WhenLogExists_ShouldReturnViewWithModel()
    {
        // Arrange
        var log = new Log { Id = 1, Details = "Found log" };
        A.CallTo(() => _logService.GetByIdAsync(1)).Returns(log);

        // Act
        var result = await _controller.View(1);

        // Assert
        var viewResult = result as ViewResult;
        viewResult.Should().NotBeNull();
        viewResult!.Model.Should().BeOfType<LogModel>();
        ((LogModel)viewResult.Model!).Id.Should().Be(1);
    }
}
