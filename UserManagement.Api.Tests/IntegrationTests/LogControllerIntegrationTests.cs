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
        var response = await _client.GetAsync("/api/logs");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<PaginatedList<LogModel>>();
        result.Should().NotBeNull();
        result!.Data.Should().NotBeEmpty();
    }

    [Theory]
    [MemberData(nameof(LogFilterTestCases))]
    public async Task GetLogs_WithVariousFilters_ShouldReturnExpectedResults(
        string? search, long? userId, DateTime? from, DateTime? to, int page, int limit, string sort)
    {
        var query = new List<string>();
        if (search is not null) query.Add($"search={search}");
        if (userId is not null) query.Add($"userId={userId}");
        if (from is not null) query.Add($"from={from:o}");
        if (to is not null) query.Add($"to={to:o}");
        query.Add($"page={page}");
        query.Add($"limit={limit}");
        query.Add($"sort={sort}");

        var url = "/api/logs?" + string.Join("&", query);
        var response = await _client.GetAsync(url);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<PaginatedList<LogModel>>();
        result.Should().NotBeNull();

        if (result!.Data.Count > 0)
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
        var from = DateTime.UtcNow;
        var to = DateTime.UtcNow.AddDays(-1);

        var response = await _client.GetAsync($"/api/logs?from={from:o}&to={to:o}");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetLogs_WithFutureFromDate_ShouldReturnBadRequest()
    {
        var future = DateTime.UtcNow.AddDays(1);

        var response = await _client.GetAsync($"/api/logs?from={future:o}");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetLog_WithValidId_ShouldReturnLog()
    {
        var response = await _client.GetAsync("/api/logs/33");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var log = await response.Content.ReadFromJsonAsync<LogModel>();
        log.Should().NotBeNull();
        log.Id.Should().Be(33);
    }

    [Fact]
    public async Task GetLog_WithInvalidId_ShouldReturnNotFound()
    {
        var response = await _client.GetAsync("/api/logs/999");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetLog_WithNullId_ShouldReturnNotFound()
    {
        var response = await _client.GetAsync("/api/logs/null");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetLogs_MissingApiKey_ShouldReturnUnauthorized()
    {
        _client.DefaultRequestHeaders.Remove(AuthConstants.ApiKeyHeaderName);

        var response = await _client.GetAsync("/api/logs");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    public static IEnumerable<object[]> LogFilterTestCases =>
        new List<object[]>
        {
            // Basic search and user filter
            new object[] { "login", 1L, null, null, 1, 10, "timestamp_desc" },

            // Search with date range
            new object[] { "error", 2L, DateTime.UtcNow.AddDays(-7), DateTime.UtcNow, 1, 5, "id" },

            // Search only
            new object[] { "update", null, null, null, 1, 10, "user_desc" },

            // User only with date range
            new object[] { null, 3L, DateTime.UtcNow.AddDays(-30), DateTime.UtcNow.AddDays(-1), 1, 10, "action" },

            // No filters, default pagination
            new object[] { null, null, null, null, 1, 10, "timestamp" },

            // Invalid sort key (should fallback or error)
            new object[] { "view", 10L, null, null, 1, 10, "invalid_sort" },

            // High page number (likely empty)
            new object[] { null, null, null, null, 999, 10, "timestamp_desc" },
        };
}
