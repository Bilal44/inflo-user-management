using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using UserManagement.Data.Entities;
using UserManagement.Data.Repositories.Interfaces;
using UserManagement.Services.Exceptions;
using Exception = System.Exception;

namespace UserManagement.Services.Tests.Services;

public class UserServiceTests
{
    private readonly IRepository<User> _repository = A.Fake<IRepository<User>>();
    private readonly ILogger<UserService> _logger = A.Fake<ILogger<UserService>>();
    private readonly UserService _service;

    public UserServiceTests()
    {
        _service = new UserService(_repository, _logger);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnUsers()
    {
        // Arrange
        var expectedUsers = new List<User> { new() { Id = 1, Email = "test@example.com" } };
        A.CallTo(() => _repository.GetAllAsync(null, A<CancellationToken>._)).Returns(expectedUsers);

        // Act
        var result = await _service.GetAllAsync(CancellationToken.None);

        // Assert
        result.Should().BeEquivalentTo(expectedUsers);
    }

    [Fact]
    public async Task GetAllAsync_WhenExceptionThrown_ShouldLogAndThrowApiException()
    {
        A.CallTo(() => _repository.GetAllAsync(null, A<CancellationToken>._)).Throws<Exception>();

        Func<Task> act = async () => await _service.GetAllAsync(CancellationToken.None);

        await act.Should().ThrowAsync<ApiException>()
            .Where(e => e.StatusCode == HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnUser()
    {
        var user = new User { Id = 1 };
        A.CallTo(() => _repository.GetByIdAsync(1L)).Returns(user);

        var result = await _service.GetByIdAsync(1L);

        result.Should().Be(user);
    }

    [Fact]
    public async Task GetByIdAsync_WhenExceptionThrown_ShouldThrowApiException()
    {
        A.CallTo(() => _repository.GetByIdAsync(1L)).Throws<Exception>();

        var act = () => _service.GetByIdAsync(1L);

        await act.Should().ThrowAsync<ApiException>()
            .Where(e => e.StatusCode == HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task FilterByActiveAsync_ShouldReturnFilteredUsers()
    {
        var users = new List<User> { new() { IsActive = true } };
        A.CallTo(() => _repository.GetAllAsync(A<Expression<Func<User, bool>>>._, A<CancellationToken>._)).Returns(users);

        var result = await _service.FilterByActiveAsync(true, CancellationToken.None);

        result.Should().OnlyContain(u => u.IsActive);
    }

    [Fact]
    public async Task FilterByActiveAsync_WhenExceptionThrown_ShouldThrowApiException()
    {
        A.CallTo(() => _repository.GetAllAsync(A<Expression<Func<User, bool>>>._, A<CancellationToken>._)).Throws<Exception>();

        Func<Task> act = async () => await _service.FilterByActiveAsync(true, CancellationToken.None);

        await act.Should().ThrowAsync<ApiException>()
            .Where(e => e.StatusCode == HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task AddAsync_ShouldAddUser_WhenEmailIsUnique()
    {
        var newUser = new User { Id = 0, Email = "unique@example.com" };
        A.CallTo(() => _repository.GetAllAsync(A<Expression<Func<User, bool>>>._, A<CancellationToken>._))
            .Returns(new List<User>());

        await _service.Invoking(s => s.AddAsync(newUser))
            .Should().NotThrowAsync();

        A.CallTo(() => _repository.CreateAsync(newUser)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task AddAsync_ShouldThrowConflict_WhenEmailAlreadyExists()
    {
        var newUser = new User { Id = 0, Email = "duplicate@example.com" };
        var existingUser = new User { Id = 1, Email = "duplicate@example.com" };

        A.CallTo(() => _repository.GetAllAsync(A<Expression<Func<User, bool>>>._, A<CancellationToken>._))
            .Returns(new List<User> { existingUser });

        var act = () => _service.AddAsync(newUser);

        await act.Should().ThrowAsync<ApiException>()
            .Where(e => e.StatusCode == HttpStatusCode.Conflict);

        A.CallTo(() => _repository.CreateAsync(A<User>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task AddAsync_WhenExceptionThrown_ShouldThrowApiException()
    {
        var user = new User { Id = 1 };
        A.CallTo(() => _repository.CreateAsync(user)).Throws<Exception>();

        Func<Task> act = async () => await _service.AddAsync(user);

        await act.Should().ThrowAsync<ApiException>()
            .Where(e => e.StatusCode == HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateUser_WhenEmailIsUnique()
    {
        var user = new User { Id = 2, Email = "unique@example.com" };
        A.CallTo(() => _repository.GetAllAsync(A<Expression<Func<User, bool>>>._, A<CancellationToken>._))
            .Returns(new List<User>());

        await _service.Invoking(s => s.UpdateAsync(user))
            .Should().NotThrowAsync();

        A.CallTo(() => _repository.UpdateAsync(user)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrowConflict_WhenEmailBelongsToAnotherUser()
    {
        var user = new User { Id = 2, Email = "duplicate@example.com" };
        var otherUser = new User { Id = 1, Email = "duplicate@example.com" };

        A.CallTo(() => _repository.GetAllAsync(A<Expression<Func<User, bool>>>._, A<CancellationToken>._))
            .Returns(new List<User> { otherUser });

        var act = () => _service.UpdateAsync(user);

        await act.Should().ThrowAsync<ApiException>()
            .Where(e => e.StatusCode == HttpStatusCode.Conflict);

        A.CallTo(() => _repository.UpdateAsync(A<User>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task UpdateAsync_WhenExceptionThrown_ShouldThrowApiException()
    {
        var user = new User { Id = 1 };
        A.CallTo(() => _repository.UpdateAsync(user)).Throws<Exception>();

        var act = async () => await _service.UpdateAsync(user);

        await act.Should().ThrowAsync<ApiException>()
            .Where(e => e.StatusCode == HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnDeleteCount()
    {
        A.CallTo(() => _repository.DeleteAsync(1L)).Returns(1);

        var result = await _service.DeleteAsync(1L);

        result.Should().Be(1);
    }

    [Fact]
    public async Task DeleteAsync_WhenExceptionThrown_ShouldThrowApiException()
    {
        A.CallTo(() => _repository.DeleteAsync(1L)).Throws<Exception>();

        var act = async () => await _service.DeleteAsync(1L);

        await act.Should().ThrowAsync<ApiException>()
            .Where(e => e.StatusCode == HttpStatusCode.InternalServerError);
    }
}
