using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.Extensions.Logging;
using UserManagement.Data.Entities;
using UserManagement.Data.Entities.Enums;
using UserManagement.Data.Repositories.Interfaces;
using UserManagement.Services.Exceptions;
using UserManagement.Services.Interfaces;

namespace UserManagement.Services;

public class UserService(
    IRepository<User> userRepository,
    IRepository<Log> logRepository,
    IBackgroundJobClient backgroundJob,
    ILogger<UserService> logger) : IUserService
{

    private const string GenericErrorMessage = "An unexpected error occurred, please try again. If the problem persists, please contact our support team.";
    public async Task<List<User>> GetAllAsync(CancellationToken cancellationToken)
    {
        try
        {
            return await userRepository.GetAllAsync(cancellationToken: cancellationToken);
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
            var user = await userRepository.GetByIdAsync(id);
            if (user is not null)
                await AddLogEntry(ActionType.ViewUser, user.Id);
            return user;
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
            return await userRepository.GetAllAsync(u => u.IsActive == isActive, cancellationToken);
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
        var existingUser = await userRepository.GetAllAsync(u => u.Email == user.Email);
        if (existingUser.Count > 0)
        {
            logger.LogWarning("User with id [{UserId}] already exists with this email.", existingUser[0].Id);
            throw new ApiException(HttpStatusCode.Conflict, "A user already exists with this email.");
        }

        try
        {
            await userRepository.CreateAsync(user);
            await AddLogEntry(ActionType.AddUser, user.Id);
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
        var existingUser = await userRepository.GetAllAsync(u => u.Id != user.Id &&
                                                             u.Email == user.Email);
        if (existingUser.Count > 0)
        {
            logger.LogWarning("User with id [{UserId}] already exists with this email.", existingUser[0].Id);
            throw new ApiException(HttpStatusCode.Conflict, "A user already exists with this email.");
        }

        try
        {
            await userRepository.UpdateAsync(user);
            await AddLogEntry(ActionType.UpdateUser, user.Id);
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
            var deletedCount =  await userRepository.DeleteAsync(id);
            if (deletedCount == 1)
                await AddLogEntry(ActionType.DeleteUser, id);
            return deletedCount;
        }
        catch (Exception e)
        {
            logger.LogError(e,
                "Encountered an error while deleting a user with id [{UserId}]", id);
            throw new ApiException(HttpStatusCode.InternalServerError, GenericErrorMessage);
        }
    }

    // Queue the log entry after every successful user action for asynchronous update.
    private Task AddLogEntry(
        ActionType actionType,
        long userId)
    {
        var actions = new ReadOnlyDictionary<ActionType, string>(
            new Dictionary<ActionType, string>
            {
                { ActionType.ViewUser, "Viewed" },
                { ActionType.AddUser, "Added" },
                { ActionType.UpdateUser, "Updated" },
                { ActionType.DeleteUser, "Deleted" }
            }
        );

        var log = new Log
        {
            UserId = userId,
            ActionType = actionType,
            Details = $"{actions.GetValueOrDefault(actionType)} user with id [{userId}]",
        };

        backgroundJob.Enqueue(() => logRepository.CreateAsync(log));
        return Task.CompletedTask;
    }
}
