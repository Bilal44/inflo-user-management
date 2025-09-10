using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using UserManagement.Data.Entities;
using UserManagement.Data.Entities.Enums;
using UserManagement.Data.Repositories.Interfaces;
using UserManagement.Services.Domain.Models;
using UserManagement.Services.Exceptions;

namespace UserManagement.Services.Tests.Services;

public class LogServiceTests
{
    private readonly IRepository<Log> _repository = A.Fake<IRepository<Log>>();
    private readonly ILogger<LogService> _logger = A.Fake<ILogger<LogService>>();
    private readonly LogService _service;
    private const int DefaultPageSize = 20;

    public LogServiceTests()
    {
        _service = new LogService(_repository, _logger);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnLog()
    {
        // Arrange
        var log = new Log { Id = 1, UserId = 2, ActionType = ActionType.AddUser, Details = "Test log" };
        A.CallTo(() => _repository.GetByIdAsync(1L)).Returns(log);

        // Act
        var result = await _service.GetByIdAsync(1L);

        // Assert
        result.Should().BeAssignableTo<Log>();
        result.Id.Should().Be(log.Id);
        result.Details.Should().Be(log.Details);
    }

    [Fact]
    public async Task AddAsync_ShouldCallRepository()
    {
        // Arrange
        var log = new Log { Id = 1, Details = "New log" };

        // Act
        await _service.AddAsync(log);

        // Assert
        A.CallTo(() => _repository.CreateAsync(log)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task GetPaginatedResultsAsync_WithDefaultSort_ShouldUseTimestamp()
    {
        // Arrange
        var filter = new PaginationFilter { SortBy = "unknown_sort", CurrentPage = 1, PageSize = 5 };

        A.CallTo(() => _repository.GetPaginatedResultsAsync(
            A<Expression<Func<Log, bool>>>._,
            A<Expression<Func<Log, object>>>.That.Matches(expr => expr.Compile().Invoke(new Log()) is DateTime),
            false,
            1,
            5))
            .Returns((new List<Log>(), 0));

        // Act
        await _service.GetPaginatedResultsAsync(filter);

        // Assert
        A.CallTo(() => _repository.GetPaginatedResultsAsync(
                A<Expression<Func<Log, bool>>>._,
                A<Expression<Func<Log, object>>>.That.Matches(expr =>
                    expr.ToString().Contains("log.Timestamp")),
                false,
                1,
                5))
            .MustHaveHappened();
    }

    [Fact]
    public async Task GetPaginatedResultsAsync_WithExcessivePageSize_RestrictResultsToDefaultPageSize()
    {
        // Arrange
        var excessivePageSize = int.MaxValue;
        var page = 1;
        var logs = new List<Log> { new() { Id = 1, Details = "Test log 1" }, new() { Id = 2, Details = "Test log 2" } };
        var paginatedResult = (logs, 1);

        A.CallTo(() => _repository.GetPaginatedResultsAsync(
                A<Expression<Func<Log, bool>>>._,
                A<Expression<Func<Log, object>>>._,
                A<bool>._,
                A<int>._,
                DefaultPageSize
            )).Returns(paginatedResult);

        // Act
        var result = await _service.GetPaginatedResultsAsync(
            new PaginationFilter { PageSize = excessivePageSize, SortBy = string.Empty, });

        // Assert
        result.Logs.Should().BeEquivalentTo(logs);
        result.TotalPages.Should().Be(page);

        A.CallTo(() => _repository.GetPaginatedResultsAsync(
                A<Expression<Func<Log, bool>>>._,
                A<Expression<Func<Log, object>>>._,
                A<bool>._,
                A<int>._,
                DefaultPageSize))
            .MustHaveHappenedOnceExactly();

        A.CallTo(() => _repository.GetPaginatedResultsAsync(
                A<Expression<Func<Log, bool>>>._,
                A<Expression<Func<Log, object>>>._,
                A<bool>._,
                A<int>._,
                excessivePageSize))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Service_WhenRepositoryThrowsException_ShouldThrowsApiException()
    {
        // Arrange
        var logId = 1L;

        A.CallTo(() => _repository.GetByIdAsync(logId))
            .Throws<System.Data.DBConcurrencyException>();

        // Act
        var act = () => _service.GetByIdAsync(logId);

        // Assert
        await act.Should().ThrowAsync<ApiException>();
    }
}
