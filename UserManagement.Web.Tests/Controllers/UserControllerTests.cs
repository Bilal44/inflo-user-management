using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using UserManagement.Data.Entities;
using UserManagement.Services.Domain.Models;
using UserManagement.Services.Interfaces;
using UserManagement.Web.Controllers;

namespace UserManagement.Web.Tests.Controllers;

public class UserControllerTests
{
    private readonly IUserService _userService = A.Fake<IUserService>();
    private readonly ILogService _logService = A.Fake<ILogService>();

    [Fact]
    public async Task List_WhenServiceReturnsUsers_ModelMustContainUsers()
    {
        // Arrange
        var users = GetUsers();
        var controller = new UserController(_userService, _logService);

        // Act
        var result = await controller.List(null);

        // Assert
        result.Should().BeOfType<ViewResult>()
            .Which.Model.Should().BeOfType<List<UserModel>>();
    }

    private List<User> GetUsers()
    {
        var users = new List<User>
        {
            new User
            {
                Forename = "Johnny",
                Surname = "User",
                Email = "juser@example.com",
                IsActive = true
            }
        };

        A.CallTo(() => _userService.GetAllAsync(CancellationToken.None)).Returns(users);
        return users;
    }
}
