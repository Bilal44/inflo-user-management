using Microsoft.AspNetCore.Mvc;
using UserManagement.Api.Controllers;
using UserManagement.Data.Entities;
using UserManagement.Services.Domain.Models;
using UserManagement.Services.Interfaces;

namespace UserManagement.Api.Tests.Controllers;

public class UserControllerTests
{
    private readonly IUserService _userService = A.Fake<IUserService>();
    private readonly UserController _controller;

    public UserControllerTests()
    {
        _controller = new UserController(_userService);
    }

    [Fact]
    public async Task GetUsers_ShouldReturnMappedUsers()
    {
        // Arrange
        var users = new List<User> { new() { Id = 1, Forename = "Alice", Email = "alice@example.com" } };
        A.CallTo(() => _userService.GetAllAsync(A<CancellationToken>._)).Returns(users);

        // Act
        var result = await _controller.GetUsers(null, CancellationToken.None);

        // Assert
        var okResult = result.Result as OkObjectResult;
        okResult.Should().NotBeNull();
        var model = okResult!.Value as List<UserModel>;
        model.Should().NotBeNull();
        model.Should().ContainSingle(u => u.Id == 1 && u.Email == "alice@example.com");
    }

    [Fact]
    public async Task GetUser_WhenUserExists_ShouldReturnMappedUser()
    {
        var user = new User { Id = 1, Forename = "Bob", Email = "bob@example.com" };
        A.CallTo(() => _userService.GetByIdAsync(1)).Returns(user);

        var result = await _controller.GetUser(1);

        var okResult = result as OkObjectResult;
        okResult.Should().NotBeNull();
        var model = okResult!.Value as UserModel;
        model.Should().NotBeNull();
        model!.Id.Should().Be(1);
        model.Email.Should().Be("bob@example.com");
    }

    [Fact]
    public async Task GetUser_WhenUserDoesNotExist_ShouldReturnNotFound()
    {
        A.CallTo(() => _userService.GetByIdAsync(99)).Returns((User?)null);

        var result = await _controller.GetUser(99);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task UpdateUser_WhenIdMismatch_ShouldReturnBadRequest()
    {
        var model = new UserModel { Id = 2 };

        var result = await _controller.UpdateUser(1, model);

        result.Should().BeOfType<BadRequestResult>();
    }

    [Fact]
    public async Task UpdateUser_WhenValid_ShouldCallServiceAndReturnNoContent()
    {
        var model = new UserModel { Id = 1, Forename = "Charlie" };

        var result = await _controller.UpdateUser(1, model);

        A.CallTo(() => _userService.UpdateAsync(A<User>._)).MustHaveHappenedOnceExactly();
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task AddUser_ShouldCallServiceAndReturnCreated()
    {
        var model = new UserModel { Id = 1, Forename = "Dana" };

        var result = await _controller.AddUser(model);

        A.CallTo(() => _userService.AddAsync(A<User>._)).MustHaveHappenedOnceExactly();

        var createdResult = result.Result as CreatedAtActionResult;
        createdResult.Should().NotBeNull();
        createdResult!.Value.Should().Be(model);
        createdResult.RouteValues!["id"].Should().Be(1);
    }

    [Fact]
    public async Task DeleteUser_WhenUserNotFound_ShouldReturnNotFound()
    {
        A.CallTo(() => _userService.GetByIdAsync(99)).Returns<User?>(null);

        var result = await _controller.DeleteUser(99);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task DeleteUser_WhenDeleted_ShouldReturnNoContent()
    {
        A.CallTo(() => _userService.GetByIdAsync(1)).Returns(new User { Id = 1 });
        A.CallTo(() => _userService.DeleteAsync(1)).Returns(1);

        var result = await _controller.DeleteUser(1);

        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task DeleteUser_WhenDeleteFails_ShouldReturnServerError()
    {
        A.CallTo(() => _userService.GetByIdAsync(1L)).Returns(new User { Id = 1 });
        A.CallTo(() => _userService.DeleteAsync(1L)).Returns(0);

        var result = await _controller.DeleteUser(1L);

        var statusResult = result as ObjectResult;
        statusResult.Should().NotBeNull();
        statusResult.StatusCode.Should().Be(500);
    }
}
