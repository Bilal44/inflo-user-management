using System.Net;
using System.Net.Http.Json;
using UserManagement.Api.Authentication;
using UserManagement.Api.Tests.IntegrationTests.TestHelpers;
using UserManagement.Services.Domain.Models;

namespace UserManagement.Api.Tests.IntegrationTests;

public class LogControllerIntegrationTests : IClassFixture<TestDbFactory>
{
    private readonly HttpClient _client;

    public LogControllerIntegrationTests(TestDbFactory factory)
    {
        _client = factory.CreateClient();
        _client.DefaultRequestHeaders.Add(AuthConstants.ApiKeyHeaderName, "test-api-key");
    }

    [Fact]
    public async Task GetLogs_ShouldReturnPaginatedLogs()
    {
        // Act
        var response = await _client.GetAsync("/api/logs");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<PaginatedList<LogModel>>();
        result.Should().NotBeNull();
        result.Data.Should().HaveCount(10);
        result.PaginationFilter.CurrentPage.Should().Be(1);
        result.TotalPages.Should().Be(4);
    }

    [Theory]
    [MemberData(nameof(LogFilterTestCases))]
    public async Task GetLogs_WithVariousFilters_ShouldReturnExpectedResults(
        string? search, long? userId, DateTime? from, DateTime? to, int page, int limit, string sort)
    {
        // Arrange
        var query = new List<string>();
        if (search is not null) query.Add($"search={search}");
        if (userId is not null) query.Add($"userId={userId}");
        if (from is not null) query.Add($"from={from:o}");
        if (to is not null) query.Add($"to={to:o}");
        query.Add($"page={page}");
        query.Add($"limit={limit}");
        query.Add($"sort={sort}");

        // Act
        var url = "/api/logs?" + string.Join("&", query);
        var response = await _client.GetAsync(url);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<PaginatedList<LogModel>>();
        result.Should().NotBeNull();

        if (result.Data.Count > 0)
        {
            if (search is not null)
                result.Data.Should()
                    .OnlyContain(log => log.Details!.Contains(search, StringComparison.OrdinalIgnoreCase));
            if (userId is not null)
                result.Data.Should().OnlyContain(log => log.UserId == userId);
            if (from is not null)
                result.Data.Should().OnlyContain(log => log.Timestamp >= from);
            if (to is not null)
                result.Data.Should().OnlyContain(log => log.Timestamp <= to);
        }
    }

    [Fact]
    public async Task GetLogs_WithInvalidDateRange_ShouldReturnBadRequest()
    {
        // Arrange
        var from = DateTime.UtcNow;
        var to = DateTime.UtcNow.AddDays(-1);

        // Act
        var response = await _client.GetAsync($"/api/logs?from={from:o}&to={to:o}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetLogs_WithFutureFromDate_ShouldReturnBadRequest()
    {
        // Arrange
        var future = DateTime.UtcNow.AddDays(1);

        // Act
        var response = await _client.GetAsync($"/api/logs?from={future:o}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetLog_WithValidId_ShouldReturnLog()
    {
        // Arrange
        var response = await _client.GetAsync("/api/logs/33");

        // Act
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Assert
        var log = await response.Content.ReadFromJsonAsync<LogModel>();
        log.Should().NotBeNull();
        log.Id.Should().Be(33);
    }

    [Fact]
    public async Task GetLog_WithInvalidId_ShouldReturnNotFound()
    {
        // Act
        var response = await _client.GetAsync("/api/logs/999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetLog_WithNullId_ShouldReturnNotFound()
    {
        // Act
        var response = await _client.GetAsync("/api/logs/null");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetLogs_MissingApiKey_ShouldReturnUnauthorized()
    {
        // Arrange
        _client.DefaultRequestHeaders.Remove(AuthConstants.ApiKeyHeaderName);

        // Act
        var response = await _client.GetAsync("/api/logs");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    public static IEnumerable<object[]> LogFilterTestCases =>
        new List<object[]>
        {
            // Basic search and user filter
            new object[] { "tes", 1L, null, null, 1, 10, "timestamp_desc" },

            // Search with date range
            new object[] { "xt", 2L, DateTime.UtcNow.AddDays(-7), DateTime.UtcNow, 1, 5, "id" },

            // Search only
            new object[] { "2", null, null, null, 1, 10, "user_desc" },

            // User only with date range
            new object[] { null, 3L, DateTime.UtcNow.AddDays(-3000), DateTime.UtcNow.AddDays(-1), 1, 10, "action" },

            // No filters, default pagination
            new object[] { null, null, null, null, 1, 10, "timestamp" },

            // Invalid sort key (should fallback or error)
            new object[] { "view", 10L, null, null, 1, 10, "invalid_sort" },

            // High page number (likely empty)
            new object[] { null, null, null, null, 999, 10, "timestamp_desc" },
        };
}
