using System.Net;
using System.Threading;
using UserManagement.Services.Exceptions;

namespace UserManagement.Web.Tests.Controllers;

public class UserControllerTests
{
    private readonly IUserService _userService = A.Fake<IUserService>();
    private readonly UserController _controller;
    public UserControllerTests()
    {
        _controller = new UserController(_userService);
        _controller.TempData = new TempDataDictionary(
            new DefaultHttpContext(),
            A.Fake<ITempDataProvider>());
    }


    [Fact]
    public async Task List_WhenServiceReturnsUsers_ModelShouldContainUsers()
    {
        // Arrange
        var users = GetUsers();

        // Act
        var result = await _controller.List(null);

        // Assert
        result.Should().BeOfType<ViewResult>()
            .Which.Model.Should().BeOfType<List<UserModel>>()
            .Which.Count.Should().Be(users.Count);
    }

    [Fact]
    public async Task Edit_WithUpdateServiceException_ReturnsViewWithError()
    {
        // Arrange
        var userId = 1L;
        var userModel = new UserModel
        {
            Id = userId,
            Forename = "John",
            Surname = "Doe",
            Email = "john.doe@example.com",
            IsActive = true
        };

        A.CallTo(() => _userService.UpdateAsync(A<User>._))
            .ThrowsAsync(new Exception("Database connection failed"));

        // Act
        var result = await _controller.Edit(userId, userModel);

        // Assert
        result.Should().BeOfType<ViewResult>()
            .Which.Model.Should().Be(userModel);

        _controller.TempData["error"].As<string>()
            .Should().Contain("unexpected error");
    }

    [Fact]
    public async Task List_WhenNoActiveFilterProvided_ReturnsAllUsers()
    {
        // Arrange
        var users = new List<User>
        {
            new User
            {
                Id = 1,
                Forename = "John",
                Surname = "Doe",
                Email = "john.doe@example.com",
                IsActive = true
            },
            new User
            {
                Id = 2,
                Forename = "Jane",
                Surname = "Smith",
                Email = "jane.smith@example.com",
                IsActive = false
            }
        };

        A.CallTo(() => _userService.GetAllAsync(A<CancellationToken>._))
            .Returns(users);

        // Act
        var result = await _controller.List(null);

        // Assert
        result.Should().BeOfType<ViewResult>()
            .Which.Model.Should().BeOfType<List<UserModel>>()
            .Which.Count.Should().Be(2);

        A.CallTo(() => _userService.GetAllAsync(A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _userService.FilterByActiveAsync(A<bool>._, A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task View_WhenValidUserIdProvided_ReturnsUserDetails()
    {
        // Arrange
        var userId = 1L;
        var user = new User
        {
            Id = userId,
            Forename = "John",
            Surname = "Doe",
            Email = "john.doe@example.com",
            IsActive = true
        };

        A.CallTo(() => _userService.GetByIdAsync(userId)).Returns(user);

        // Act
        var result = await _controller.View(userId);

        // Assert
        result.Should().BeOfType<ViewResult>()
            .Which.Model.Should().BeOfType<UserModel>()
            .Which.Id.Should().Be(userId);

        A.CallTo(() => _userService.GetByIdAsync(userId)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Add_WhenValidModelProvided_CreatesUserSuccessfully()
    {
        // Arrange
        var userModel = new UserModel
        {
            Forename = "Jane",
            Surname = "Smith",
            Email = "jane.smith@example.com",
            IsActive = true
        };

        _controller.ModelState.Clear();

        // Act
        var result = await _controller.Add(userModel);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>()
            .Which.ActionName.Should().Be("List");

        A.CallTo(() => _userService.AddAsync(A<User>._)).MustHaveHappenedOnceExactly();
        _controller.TempData["success"].As<string>()
            .Should().StartWith("Successfully added");
    }

    [Fact]
    public async Task List_WhenActiveFilterProvided_ReturnsFilteredUsers()
    {
        // Arrange
        var activeUsers = new List<User>
        {
            new User
            {
                Id = 1,
                Forename = "John",
                Surname = "Doe",
                Email = "john.doe@example.com",
                IsActive = true
            }
        };

        A.CallTo(() => _userService.FilterByActiveAsync(true, A<CancellationToken>._))
            .Returns(activeUsers);

        // Act
        var result = await _controller.List(true);

        // Assert
        result.Should().BeOfType<ViewResult>()
            .Which.Model.Should().BeOfType<List<UserModel>>()
            .Which.Count.Should().Be(1);

        A.CallTo(() => _userService.FilterByActiveAsync(true, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _userService.GetAllAsync(A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Edit_WhenValidModelAndMatchingId_UpdatesUserSuccessfully()
    {
        // Arrange
        var userId = 1L;
        var userModel = new UserModel
        {
            Id = userId,
            Forename = "John",
            Surname = "Doe",
            Email = "john.doe@example.com",
            IsActive = true
        };

        _controller.ModelState.Clear();

        // Act
        var result = await _controller.Edit(userId, userModel);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>()
            .Which.ActionName.Should().Be("List");

        A.CallTo(() => _userService.UpdateAsync(A<User>._))
            .MustHaveHappenedOnceExactly();
        _controller.TempData["success"].As<string>()
            .Should().StartWith("Successfully updated");
    }

    [Fact]
    public async Task DeleteConfirmed_WhenValidIdProvided_DeletesUserSuccessfully()
    {
        // Arrange
        var userId = 1L;
        A.CallTo(() => _userService.DeleteAsync(userId))
            .Returns(1);

        // Act
        var result = await _controller.DeleteConfirmed(userId);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>()
            .Which.ActionName.Should().Be("List");

        A.CallTo(() => _userService.DeleteAsync(userId))
            .MustHaveHappenedOnceExactly();
        _controller.TempData["success"].Should().Be($"Successfully deleted user with id [{userId}].");
    }

    [Fact]
    public async Task Edit_WhenValidUserIdProvided_ReturnsPreFilledForm()
    {
        // Arrange
        var userId = 1L;
        var user = new User
        {
            Id = userId,
            Forename = "John",
            Surname = "Doe",
            Email = "john.doe@example.com",
            IsActive = true
        };

        A.CallTo(() => _userService.GetByIdAsync(userId)).Returns(user);

        // Act
        var result = await _controller.Edit(userId);

        // Assert
        result.Should().BeOfType<ViewResult>()
            .Which.Model.Should().BeOfType<UserModel>()
            .Which.Id.Should().Be(userId);

        A.CallTo(() => _userService.GetByIdAsync(userId)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Delete_WhenValidUserIdProvided_ReturnsUserDetailsView()
    {
        // Arrange
        var userId = 1L;
        var user = new User
        {
            Id = userId,
            Forename = "John",
            Surname = "Doe",
            Email = "john.doe@example.com",
            IsActive = true
        };

        A.CallTo(() => _userService.GetByIdAsync(userId)).Returns(user);

        // Act
        var result = await _controller.Delete(userId);

        // Assert
        result.Should().BeOfType<ViewResult>()
            .Which.Model.Should().BeOfType<UserModel>()
            .Which.Id.Should().Be(userId);

        A.CallTo(() => _userService.GetByIdAsync(userId))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Delete_WithNullId_RedirectsToListWithError()
    {
        // Act
        var result = await _controller.Delete(null);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>()
            .Which.ActionName.Should().Be("List");

        _controller.TempData["error"].Should().Be("Failed to find a user with id [].");
    }

    [Fact]
    public async Task Delete_WhenUserNotFound_RedirectsToList()
    {
        // Arrange
        var userId = 1L;
        A.CallTo(() => _userService.GetByIdAsync(userId))
            .Returns<User?>(null);

        // Act
        var result = await _controller.Delete(userId);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>()
            .Which.ActionName.Should().Be("List");

        _controller.TempData["error"].Should().Be($"Failed to find a user with id [{userId}].");
    }

    [Fact]
    public async Task Delete_WhenUnexpectedExceptionOccurs_RedirectsToList()
    {
        // Arrange
        var userId = 1L;
        A.CallTo(() => _userService.GetByIdAsync(userId))
            .ThrowsAsync(new Exception("Service unavailable"));

        // Act
        var result = await _controller.Delete(userId);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>()
            .Which.ActionName.Should().Be("List");

        _controller.TempData["error"].As<string>()
            .Should().Contain("unexpected error");
        A.CallTo(() => _userService.GetByIdAsync(userId))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void Add_WhenGetRequestMade_ReturnsEmptyFormView()
    {
        // Act
        var result = _controller.Add();

        // Assert
        result.Should().BeOfType<ViewResult>()
            .Which.Model.Should().BeNull();
    }

    [Fact]
    public async Task Edit_WhenDuplicateEmailExists_HandlesConflictException()
    {
        // Arrange
        var userId = 1L;
        var userModel = new UserModel
        {
            Id = userId,
            Forename = "John",
            Surname = "Doe",
            Email = "john.doe@example.com",
            IsActive = true
        };

        A.CallTo(() => _userService.UpdateAsync(A<User>._))
            .ThrowsAsync(new ApiException(HttpStatusCode.Conflict, "Conflict"));

        _controller.ModelState.Clear();

        // Act
        var result = await _controller.Edit(userId, userModel);

        // Assert
        result.Should().BeOfType<ViewResult>()
            .Which.Model.Should().Be(userModel);

        _controller.TempData["error"].Should().Be("A user with this email address already exists.");
    }

    [Fact]
    public async Task DeleteConfirmed_WhenNullIdProvided_RedirectsToList()
    {
        // Act
        var result = await _controller.DeleteConfirmed(null);

        // Assert
        result.Should().BeOfType<ViewResult>();
        _controller.TempData["error"].Should().Be("Failed to delete user.");
    }

    [Fact]
    public async Task Edit_WhenNullIdProvided_RedirectsToList()
    {
        // Act
        var result = await _controller.Edit(null);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>()
            .Which.ActionName.Should().Be("List");
        _controller.TempData["error"].Should().Be("Failed to find a user with id [].");
    }

    [Fact]
    public async Task Edit_WhenUserNotFound_RedirectsToList()
    {
        // Arrange
        var userId = 1L;
        A.CallTo(() => _userService.GetByIdAsync(userId)).Returns<User?>(null);

        // Act
        var result = await _controller.Edit(userId);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>()
            .Which.ActionName.Should().Be("List");
        _controller.TempData["error"].Should().Be($"Failed to find a user with id [{userId}].");
    }

    [Fact]
    public async Task Edit_WhenUnexpectedExceptionOccurs_ReturnsViewWithModel()
    {
        // Arrange
        var userId = 1L;
        A.CallTo(() => _userService.GetByIdAsync(userId)).ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.Edit(userId);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>()
            .Which.ActionName.Should().Be("List");
        _controller.TempData["error"].As<string>()
            .Should().Contain("unexpected error");
    }

    [Fact]
    public async Task List_WhenActiveStatusIsFalse_ReturnsInactiveUsers()
    {
        // Arrange
        var inactiveUsers = new List<User>
        {
            new User
            {
                Id = 1,
                Forename = "John",
                Surname = "Doe",
                Email = "john.doe@example.com",
                IsActive = false
            },
            new User
            {
                Id = 2,
                Forename = "Jane",
                Surname = "Smith",
                Email = "jane.smith@example.com",
                IsActive = false
            }
        };

        A.CallTo(() => _userService.FilterByActiveAsync(false, A<CancellationToken>._))
            .Returns(inactiveUsers);

        // Act
        var result = await _controller.List(false);

        // Assert
        result.Should().BeOfType<ViewResult>()
            .Which.Model.Should().BeOfType<List<UserModel>>()
            .Which.Count.Should().Be(2);

        A.CallTo(() => _userService.FilterByActiveAsync(false, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _userService.GetAllAsync(A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task View_WhenNullIdProvided_RedirectsToList()
    {
        // Act
        var result = await _controller.View(null);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>()
            .Which.ActionName.Should().Be("List");

        _controller.TempData["error"].Should().Be("Failed to find a user with id [].");
    }

    [Fact]
    public async Task Add_WhenDuplicateEmailExists_HandlesConflictException()
    {
        // Arrange
        var userModel = new UserModel
        {
            Forename = "John",
            Surname = "Doe",
            Email = "john.doe@example.com",
            IsActive = true
        };

        A.CallTo(() => _userService.AddAsync(A<User>._))
            .ThrowsAsync(new ApiException(HttpStatusCode.Conflict, "Conflict"));

        _controller.ModelState.Clear();

        // Act
        var result = await _controller.Add(userModel);

        // Assert
        result.Should().BeOfType<ViewResult>()
            .Which.Model.Should().Be(userModel);

        _controller.TempData["error"].Should().Be("A user with this email address already exists.");
    }

    [Fact]
    public async Task Edit_WhenIdMismatchOccurs_ReturnsViewWithError()
    {
        // Arrange
        var routeId = 1L;
        var modelId = 2L;
        var userModel = new UserModel
        {
            Id = modelId,
            Forename = "John",
            Surname = "Doe",
            Email = "john.doe@example.com",
            IsActive = true
        };

        _controller.ModelState.Clear();

        // Act
        var result = await _controller.Edit(routeId, userModel);

        // Assert
        result.Should().BeOfType<ViewResult>()
            .Which.Model.Should().Be(userModel);

        _controller.TempData["error"].Should().Be("Failed to update user.");
        A.CallTo(() => _userService.UpdateAsync(A<User>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task DeleteConfirmed_WhenServiceReturnsUnexpectedCount_HandlesFailure()
    {
        // Arrange
        var userId = 1L;
        A.CallTo(() => _userService.DeleteAsync(userId)).Returns(0);

        // Act
        var result = await _controller.DeleteConfirmed(userId);

        // Assert
        result.Should().BeOfType<ViewResult>();
        _controller.TempData["error"].Should().Be("Failed to delete user.");
        A.CallTo(() => _userService.DeleteAsync(userId))
            .MustHaveHappenedOnceExactly();
    }


    [Fact]
    public async Task Edit_WhenModelStateInvalid_ReturnsViewWithModel()
    {
        // Arrange
        var userId = 1L;
        var userModel = new UserModel
        {
            Id = userId,
            Forename = "John",
            Surname = "Doe",
            Email = "invalid-email",
            IsActive = true
        };

        _controller.ModelState.AddModelError("Email", "Invalid email format");

        // Act
        var result = await _controller.Edit(userId, userModel);

        // Assert
        result.Should().BeOfType<ViewResult>()
            .Which.Model.Should().Be(userModel);

        A.CallTo(() => _userService.UpdateAsync(A<User>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task View_WhenUnexpectedExceptionOccurs_RedirectsToList()
    {
        // Arrange
        var userId = 1L;
        A.CallTo(() => _userService.GetByIdAsync(userId)).ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.View(userId);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>()
            .Which.ActionName.Should().Be("List");
        _controller.TempData["error"].As<string>()
            .Should().Contain("unexpected error");
    }

    [Fact]
    public async Task Edit_WhenUnexpectedExceptionOccurs_RedirectsToList()
    {
        // Arrange
        var userId = 1L;
        A.CallTo(() => _userService.GetByIdAsync(userId))
            .ThrowsAsync(new Exception("Service unavailable"));

        // Act
        var result = await _controller.Edit(userId);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>()
            .Which.ActionName.Should().Be("List");
        _controller.TempData["error"].As<string>()
            .Should().Contain("unexpected error");
    }

    [Fact]
    public async Task DeleteConfirmed_WhenUnexpectedExceptionOccurs_RedirectsToEdit()
    {
        // Arrange
        var userId = 1L;
        A.CallTo(() => _userService.DeleteAsync(userId))
            .ThrowsAsync(new Exception("Database connection failed"));

        // Act
        var result = await _controller.DeleteConfirmed(userId);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>()
            .Which.ActionName.Should().Be("Edit");

        _controller.TempData["error"].As<string>()
            .Should().Contain("unexpected error");
        A.CallTo(() => _userService.DeleteAsync(userId))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Add_WhenUnexpectedExceptionOccurs_ReturnsViewWithModel()
    {
        // Arrange
        var userModel = new UserModel
        {
            Forename = "John",
            Surname = "Doe",
            Email = "john.doe@example.com",
            IsActive = true
        };

        A.CallTo(() => _userService.AddAsync(A<User>._))
            .ThrowsAsync(new Exception("Database connection failed"));

        _controller.ModelState.Clear();

        // Act
        var result = await _controller.Add(userModel);

        // Assert
        result.Should().BeOfType<ViewResult>()
            .Which.Model.Should().Be(userModel);

        _controller.TempData["error"].As<string>()
            .Should().Contain("unexpected error");
    }

    [Fact]
    public async Task View_WhenUserNotFound_RedirectsToList()
    {
        // Arrange
        var userId = 1L;
        A.CallTo(() => _userService.GetByIdAsync(userId)).Returns<User?>(null);

        // Act
        var result = await _controller.View(userId);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>()
            .Which.ActionName.Should().Be("List");

        _controller.TempData["error"].Should().Be($"Failed to find a user with id [{userId}].");
    }

    [Fact]
    public async Task List_WhenUnexpectedExceptionOccurs_HandlesGracefully()
    {
        // Arrange
        A.CallTo(() => _userService.GetAllAsync(A<CancellationToken>._))
            .ThrowsAsync(new Exception("Database connection failed"));

        // Act
        var result = await _controller.List(null);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>()
            .Which.ActionName.Should().Be(nameof(_controller.List));
        _controller.TempData["error"].As<string>()
            .Should().Contain("unexpected error");
        A.CallTo(() => _userService.GetAllAsync(A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
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
