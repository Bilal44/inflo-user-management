using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using UserManagement.Data.Entities;
using UserManagement.Data.Entities.Enums;
using UserManagement.Data.Repositories.Interfaces;
using UserManagement.Services.Domain.Models;

namespace UserManagement.Services.Tests.Services;

public class LogServiceTests
{
    private readonly IRepository<Log> _repository = A.Fake<IRepository<Log>>();
    private readonly LogService _service;

    public LogServiceTests()
    {
        _service = new LogService(_repository);
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
    public async Task GetPaginatedResultsAsync_WithSearchAndDateFilters_ShouldApplyCorrectPredicate()
    {
        // Arrange
        var filter = new PaginationFilter
        {
            Search = "error",
            From = new DateTime(2023, 1, 1),
            To = new DateTime(2023, 12, 31),
            UserId = 42,
            SortBy = "timestamp_desc",
            CurrentPage = 1,
            PageSize = 10
        };

        var expectedLogs = new List<Log>
        {
            new Log { Id = 1, Details = "error occurred", Timestamp = new DateTime(2023, 6, 1), UserId = 42 }
        };

        A.CallTo(() => _repository.GetPaginatedResultsAsync(
            A<Expression<Func<Log, bool>>>._,
            A<Expression<Func<Log, object>>>._,
            true,
            1,
            10)).Returns((expectedLogs, 1));

        // Act
        var (logs, totalPages) = await _service.GetPaginatedResultsAsync(filter);

        // Assert
        logs.Should().BeEquivalentTo(expectedLogs);
        totalPages.Should().Be(1);
    }

    [Fact]
    public async Task GetPaginatedResultsAsync_WithDefaultSort_ShouldUseTimestamp()
    {
        // Arrange
        var filter = new PaginationFilter
        {
            SortBy = "unknown_sort",
            CurrentPage = 1,
            PageSize = 5
        };

        A.CallTo(() => _repository.GetPaginatedResultsAsync(
            A<Expression<Func<Log, bool>>>._,
            A<Expression<Func<Log, object>>>.That.Matches(expr => expr.Compile().Invoke(new Log()) is DateTime),
            false,
            1,
            5)).Returns((new List<Log>(), 0));

        // Act
        var (_, _) = await _service.GetPaginatedResultsAsync(filter);

        // Assert
        A.CallTo(() => _repository.GetPaginatedResultsAsync(
            A<Expression<Func<Log, bool>>>._,
            A<Expression<Func<Log, object>>>._,
            false,
            1,
            5)).MustHaveHappened();
    }
}
