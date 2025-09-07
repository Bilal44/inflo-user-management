using System;
using Microsoft.EntityFrameworkCore;
using UserManagement.Data.Context;
using UserManagement.Data.Entities;

namespace UserManagement.Data.Tests.Helpers;

public static class Persistence
{
    public static UserManagementDbContext GetInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<UserManagementDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var context = new UserManagementDbContext(options);

        context.Users?.AddRange(new User
        {
            Id = 1,
            Forename = "not",
            Surname = "active",
            Email = "a@b.com",
            IsActive = false
        }, new User
        {
            Id = 2,
            Forename = "is",
            Surname = "active",
            Email = "aaa@b.com",
            IsActive = true
        }, new User
        {
            Id = 3,
            Forename = "active",
            Surname = "too",
            Email = "accc@b.com",
            IsActive = true
        });

        context.SaveChanges();
        return context;
    }
}
