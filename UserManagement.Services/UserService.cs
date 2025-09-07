using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using UserManagement.Data.Entities;
using UserManagement.Data.Repositories.Interfaces;
using UserManagement.Services.Exceptions;
using UserManagement.Services.Interfaces;

namespace UserManagement.Services;

public class UserService(
    IRepository<User> repository,
    ILogger<UserService> logger) : IUserService
{

    private const string GenericErrorMessage = "An unexpected error occurred. Please try again. If the problem persists, please contact our support team.";
    public async Task<List<User>> GetAllAsync(CancellationToken cancellationToken)
    {
        try
        {
            return await repository.GetAllAsync(cancellationToken: cancellationToken);
        }
        catch (Exception e)
        {
            logger.LogError(e,
                "Encountered an error while retrieving users");

            throw new ApiException(HttpStatusCode.InternalServerError, GenericErrorMessage);
        }
    }

    public async Task<User?> GetByIdAsync(long id)
    {
        try
        {
            return await repository.GetByIdAsync(id);
        }
        catch (Exception e)
        {
            logger.LogError(e,
                "Encountered an error while retrieving user for id [{UserId}]", id);

            throw new ApiException(HttpStatusCode.InternalServerError, GenericErrorMessage);
        }
    }

    public async Task<List<User>> FilterByActiveAsync(bool isActive, CancellationToken cancellationToken)
    {
        try
        {
            return await repository.GetAllAsync(u => u.IsActive == isActive, cancellationToken);
        }
        catch (Exception e)
        {
            logger.LogError(e,
                "Encountered an error while retrieving users with status [{UserStatus}]", isActive);

            throw new ApiException(HttpStatusCode.InternalServerError, GenericErrorMessage);
        }
    }

    public async Task AddAsync(User user)
    {
        try
        {
            await repository.CreateAsync(user);
        }
        catch (Exception e)
        {
            logger.LogError(e,
                "Encountered an error while adding a new user with id [{UserId}]", user.Id);

            throw new ApiException(HttpStatusCode.InternalServerError, GenericErrorMessage);
        }
    }

    public async Task UpdateAsync(User user)
    {
        try
        {
            await repository.UpdateAsync(user);
        }
        catch (Exception e)
        {
            logger.LogError(e,
                "Encountered an error while updating a user with id [{UserId}]", user.Id);

            throw new ApiException(HttpStatusCode.InternalServerError, GenericErrorMessage);
        }
    }

    public async Task<int> DeleteAsync(long id)
    {
        try
        {
            return await repository.DeleteAsync(id);
        }
        catch (Exception e)
        {
            logger.LogError(e,
                "Encountered an error while deleting a user with id [{UserId}]", id);

            throw new ApiException(HttpStatusCode.InternalServerError, GenericErrorMessage);
        }
    }
}
