using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.Extensions.Logging;
using UserManagement.Data.Entities;
using UserManagement.Data.Repositories.Interfaces;
using UserManagement.Services.Exceptions;
using Exception = System.Exception;

namespace UserManagement.Services.Tests.Services;

public class UserServiceTests
{
    private readonly IRepository<User> _userRepository = A.Fake<IRepository<User>>();
    private readonly IRepository<Log> _logRepository = A.Fake<IRepository<Log>>();
    private readonly IBackgroundJobClient _backgroundJob = A.Fake<IBackgroundJobClient>();
    private readonly ILogger<UserService> _logger = A.Fake<ILogger<UserService>>();
    private readonly UserService _service;

    public UserServiceTests()
    {
        _service = new UserService(_userRepository, _logRepository, _backgroundJob, _logger);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnUsers()
    {
        // Arrange
        var expectedUsers = new List<User> { new() { Id = 1, Email = "test@example.com" } };
        A.CallTo(
                () => _userRepository.GetAllAsync(null, A<CancellationToken>._))
            .Returns(expectedUsers);

        // Act
        var result = await _service.GetAllAsync(CancellationToken.None);

        // Assert
        result.Should().BeEquivalentTo(expectedUsers);
    }

    [Fact]
    public async Task GetAllAsync_WhenExceptionThrown_ShouldLogAndThrowApiException()
    {
        // Arrange
        A.CallTo(() => _userRepository.GetAllAsync(null, A<CancellationToken>._)).Throws<Exception>();

        // Act
        var act = async () => await _service.GetAllAsync(CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ApiException>()
            .Where(e => e.StatusCode == HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnUser()
    {
        // Act
        var user = new User { Id = 1 };
        A.CallTo(() => _userRepository.GetByIdAsync(1L)).Returns(user);

        // Arrange
        var result = await _service.GetByIdAsync(1L);

        // Assert
        result.Should().Be(user);
    }

    [Fact]
    public async Task GetByIdAsync_WhenExceptionThrown_ShouldThrowApiException()
    {
        // Arrange
        A.CallTo(() => _userRepository.GetByIdAsync(1L)).Throws<Exception>();

        // Act
        var act = () => _service.GetByIdAsync(1L);

        // Assert
        await act.Should().ThrowAsync<ApiException>()
            .Where(e => e.StatusCode == HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task FilterByActiveAsync_ShouldReturnFilteredUsers()
    {
        // Arrange
        var users = new List<User> { new() { IsActive = true } };
        A.CallTo(() => _userRepository.GetAllAsync(A<Expression<Func<User, bool>>>._, A<CancellationToken>._)).Returns(users);

        // Act
        var result = await _service.FilterByActiveAsync(true, CancellationToken.None);

        // Assert
        result.Should().OnlyContain(u => u.IsActive);
    }

    [Fact]
    public async Task FilterByActiveAsync_WhenExceptionThrown_ShouldThrowApiException()
    {
        // Arrange
        A.CallTo(() => _userRepository.GetAllAsync(A<Expression<Func<User, bool>>>._, A<CancellationToken>._)).Throws<Exception>();

        // Act
        var act = async () => await _service.FilterByActiveAsync(true, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ApiException>()
            .Where(e => e.StatusCode == HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task AddAsync_ShouldAddUser_WhenEmailIsUnique()
    {
        // Arrange
        var newUser = new User { Id = 0, Email = "unique@example.com" };
        A.CallTo(() => _userRepository.GetAllAsync(A<Expression<Func<User, bool>>>._, A<CancellationToken>._))
            .Returns(new List<User>());

        // Act
        await _service.Invoking(s => s.AddAsync(newUser))
            .Should().NotThrowAsync();

        // Assert
        A.CallTo(() => _userRepository.CreateAsync(newUser))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task AddAsync_ShouldThrowConflict_WhenEmailAlreadyExists()
    {
        // Arrange
        var newUser = new User { Id = 0, Email = "duplicate@example.com" };
        var existingUser = new User { Id = 1, Email = "duplicate@example.com" };

        A.CallTo(() => _userRepository.GetAllAsync(A<Expression<Func<User, bool>>>._, A<CancellationToken>._))
            .Returns(new List<User> { existingUser });

        // Act
        var act = () => _service.AddAsync(newUser);

        // Assert
        await act.Should().ThrowAsync<ApiException>()
            .Where(e => e.StatusCode == HttpStatusCode.Conflict);

        A.CallTo(() => _userRepository.CreateAsync(A<User>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task AddAsync_WhenExceptionThrown_ShouldThrowApiException()
    {
        // Arrange
        var user = new User { Id = 1 };
        A.CallTo(() => _userRepository.CreateAsync(user)).Throws<Exception>();

        // Act
        var act = async () => await _service.AddAsync(user);

        // Assert
        await act.Should().ThrowAsync<ApiException>()
            .Where(e => e.StatusCode == HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateUser_WhenEmailIsUnique()
    {
        // Arrange
        var user = new User { Id = 2, Email = "unique@example.com" };
        A.CallTo(() => _userRepository.GetAllAsync(A<Expression<Func<User, bool>>>._, A<CancellationToken>._))
            .Returns(new List<User>());

        // Act
        await _service.Invoking(s => s.UpdateAsync(user))
            .Should().NotThrowAsync();

        // Assert
        A.CallTo(() => _userRepository.UpdateAsync(user))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrowConflict_WhenEmailBelongsToAnotherUser()
    {
        // Arrange
        var user = new User { Id = 2, Email = "duplicate@example.com" };
        var otherUser = new User { Id = 1, Email = "duplicate@example.com" };

        A.CallTo(() => _userRepository.GetAllAsync(A<Expression<Func<User, bool>>>._, A<CancellationToken>._))
            .Returns(new List<User> { otherUser });

        // Act
        var act = () => _service.UpdateAsync(user);

        // Assert
        await act.Should().ThrowAsync<ApiException>()
            .Where(e => e.StatusCode == HttpStatusCode.Conflict);

        A.CallTo(() => _userRepository.UpdateAsync(A<User>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task UpdateAsync_WhenExceptionThrown_ShouldThrowApiException()
    {
        // Arrange
        var user = new User { Id = 1 };
        A.CallTo(() => _userRepository.UpdateAsync(user)).Throws<Exception>();

        // Act
        var act = async () => await _service.UpdateAsync(user);

        // Assert
        await act.Should().ThrowAsync<ApiException>()
            .Where(e => e.StatusCode == HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnDeleteCount()
    {
        // Arrange
        A.CallTo(() => _userRepository.DeleteAsync(1L)).Returns(1);

        // Act
        var result = await _service.DeleteAsync(1L);

        // Assert
        result.Should().Be(1);
    }

    [Fact]
    public async Task DeleteAsync_WhenExceptionThrown_ShouldThrowApiException()
    {
        // Arrange
        A.CallTo(() => _userRepository.DeleteAsync(1L)).Throws<Exception>();

        // Act
        var act = async () => await _service.DeleteAsync(1L);

        // Assert
        await act.Should().ThrowAsync<ApiException>()
            .Where(e => e.StatusCode == HttpStatusCode.InternalServerError);
    }
}
