using System.Threading;
using System.Threading.Tasks;
using UserManagement.Data.Entities;
using UserManagement.Data.Repositories;
using UserManagement.Data.Tests.Helpers;

namespace UserManagement.Data.Tests.Repositories;

public class RepositoryTests
{
    [Fact]
    public async Task GetAllAsync_WhenNewEntityAdded_MustIncludeNewEntity()
    {
        // Arrange
        var repository = new Repository<User>(Persistence.GetInMemoryContext());
        var user = new User
        {
            Forename = "Brand New",
            Surname = "User",
            Email = "brandnewuser@example.com"
        };

        // Act
        await repository.CreateAsync(user);

        // Assert
        var result = await repository.GetAllAsync(null, CancellationToken.None);
        result.Should().HaveCount(4);
        result.Should().ContainSingle(u => u.Email == "brandnewuser@example.com");
    }

    [Fact]
    public async Task GetByIdAsync_WhenEntityExists_ReturnsEntity()
    {
        // Arrange
        var context = Persistence.GetInMemoryContext();
        var repository = new Repository<User>(context);

        // Act
        var result = await repository.GetByIdAsync(1L);

        // Assert
        result.Should().NotBeNull();
        result.Email.Should().Be("a@b.com");
    }

    [Fact]
    public async Task UpdateAsync_WhenEntityUpdated_ChangesArePersisted()
    {
        // Arrange
        var context = Persistence.GetInMemoryContext();
        var repository = new Repository<User>(context);

        var user = new User { Id = 5, Forename = "Alice", Surname = "Smith", Email = "alice@example.com" };
        await repository.CreateAsync(user);

        // Act
        user.Email = "updated@example.com";
        await repository.UpdateAsync(user);

        var result = await repository.GetByIdAsync(5L);

        // Assert
        result!.Email.Should().Be("updated@example.com");
    }

    [Fact]
    public async Task GetPaginatedResultsAsync_WhenCalled_ReturnsCorrectPage()
    {
        // Arrange
        var context = Persistence.GetInMemoryContext();
        var repository = new Repository<User>(context);

        for (var i = 4; i <= 30; i++)
        {
            await repository.CreateAsync(new User
            {
                Id = i,
                Forename = $"User{i}",
                Surname = "Test",
                Email = $"user{i}@example.com"
            });
        }

        // Act
        var (results, totalPages) = await repository.GetPaginatedResultsAsync(
            predicate: null,
            orderBy: u => u.Id,
            descending: false,
            currentPage: 2,
            pageSize: 10
        );

        // Assert
        results.Should().HaveCount(10);
        results[0].Id.Should().Be(11);
        totalPages.Should().Be(3);
    }

    [Fact]
    public async Task GetAllAsync_WithPredicate_FiltersResults()
    {
        // Arrange
        var context = Persistence.GetInMemoryContext();
        var repository = new Repository<User>(context);

        // Act
        var result = await repository.GetAllAsync(u => u.IsActive, CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
    }

    // [Fact]
    // public async Task GetAllAsync_WhenEntityDeleted_MustNotIncludeDeletedEntity()
    // {
    //     // Arrange
    //     var repository = new Repository<User>(Persistence.GetInMemoryContext());
    //
    //     // Act
    //     var deleteCount = await repository.DeleteAsync(1L);
    //
    //     // Assert
    //     var result = await repository.GetAllAsync(null, CancellationToken.None);
    //     result.Should().HaveCount(2);
    //     deleteCount.Should().Be(1);
    // }
}
