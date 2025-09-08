using System.Net;
using System.Net.Http.Json;
using UserManagement.Api.Authentication;
using UserManagement.Api.Tests.IntegrationTests.TestHelpers;
using UserManagement.Services.Domain.Models;

namespace UserManagement.Api.Tests.IntegrationTests;

public class UserControllerIntegrationTests : IClassFixture<TestDbFactory>
{
    private readonly HttpClient _client;

    public UserControllerIntegrationTests(TestDbFactory factory)
    {
        _client = factory.CreateClient();
        _client.DefaultRequestHeaders.Add(AuthConstants.ApiKeyHeaderName, "test-api-key");
    }

    [Fact]
    public async Task GetUsers_ShouldReturnAllUsers()
    {
        var response = await _client.GetAsync("/api/users");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var users = await response.Content.ReadFromJsonAsync<List<UserModel>>();
        users.Should().NotBeNull();
        users.Count.Should().Be(12);
    }

    [Fact]
    public async Task GetUsers_WithActiveFilter_ShouldReturnOnlyActiveUsers()
    {
        var response = await _client.GetAsync("/api/users?active=true");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var users = await response.Content.ReadFromJsonAsync<List<UserModel>>();
        users.Should().OnlyContain(u => u.IsActive);
        users!.Count.Should().Be(8);
    }

    [Fact]
    public async Task GetUser_WithValidId_ShouldReturnUser()
    {
        var response = await _client.GetAsync("/api/users/1");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var user = await response.Content.ReadFromJsonAsync<UserModel>();
        user.Should().NotBeNull();
        user!.Id.Should().Be(1);
        user.Email.Should().Be("ploew@example.com");
    }

    [Fact]
    public async Task GetUser_WithInvalidId_ShouldReturnNotFound()
    {
        var response = await _client.GetAsync("/api/users/999");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task AddUser_ShouldCreateUser()
    {
        var newUser = new UserModel
        {
            Id = 99,
            Forename = "Test",
            Surname = "User",
            Email = "testuser@example.com",
            IsActive = true
        };

        var response = await _client.PostAsJsonAsync("/api/users", newUser);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var createdUser = await response.Content.ReadFromJsonAsync<UserModel>();
        createdUser.Should().NotBeNull();
        createdUser!.Email.Should().Be("testuser@example.com");
    }

    // [Fact]
    // public async Task UpdateUser_WithMatchingId_ShouldReturnNoContent()
    // {
    //     var updatedUser = new UserModel
    //     {
    //         Id = 11,
    //         Forename = "Peter",
    //         Surname = "Loew",
    //         Email = "updated@example.com",
    //         IsActive = true
    //     };
    //
    //     var response = await _client.PutAsJsonAsync("/api/users/11", updatedUser);
    //
    //     response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    // }

    [Fact]
    public async Task UpdateUser_WithMismatchedId_ShouldReturnBadRequest()
    {
        var updatedUser = new UserModel
        {
            Id = 2,
            Forename = "Benjamin",
            Surname = "Gates",
            Email = "bfgates@example.com",
            IsActive = true
        };

        var response = await _client.PutAsJsonAsync("/api/users/1", updatedUser);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // [Fact]
    // public async Task DeleteUser_WithValidId_ShouldReturnNoContent()
    // {
    //     var response = await _client.DeleteAsync("/api/users/2");
    //
    //     response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    // }

    [Fact]
    public async Task DeleteUser_WithInvalidId_ShouldReturnNotFound()
    {
        var response = await _client.DeleteAsync("/api/users/999");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetUsers_MissingApiKey_ShouldReturnUnauthorized()
    {
        _client.DefaultRequestHeaders.Remove(AuthConstants.ApiKeyHeaderName);

        var response = await _client.GetAsync("/api/users");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
