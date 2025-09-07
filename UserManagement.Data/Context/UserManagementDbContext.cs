using System;
using Microsoft.EntityFrameworkCore;
using UserManagement.Data.Entities;
using UserManagement.Data.Entities.Enums;

namespace UserManagement.Data.Context;

public class UserManagementDbContext(DbContextOptions options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder model)
    {
        model.Entity<User>().HasData(
            new User
            {
                Id = 1,
                Forename = "Peter",
                Surname = "Loew",
                Email = "ploew@example.com",
                IsActive = true
            },
            new User
            {
                Id = 2,
                Forename = "Benjamin Franklin",
                Surname = "Gates",
                Email = "bfgates@example.com",
                IsActive = true
            },
            new User
            {
                Id = 3,
                Forename = "Castor",
                Surname = "Troy",
                Email = "ctroy@example.com",
                IsActive = false
            },
            new User
            {
                Id = 4,
                Forename = "Memphis",
                Surname = "Raines",
                Email = "mraines@example.com",
                IsActive = true
            },
            new User
            {
                Id = 5,
                Forename = "Stanley",
                Surname = "Goodspeed",
                Email = "sgodspeed@example.com",
                IsActive = true
            },
            new User
            {
                Id = 6,
                Forename = "H.I.",
                Surname = "McDunnough",
                Email = "himcdunnough@example.com",
                IsActive = true
            },
            new User
            {
                Id = 7,
                Forename = "Cameron",
                Surname = "Poe",
                Email = "cpoe@example.com",
                IsActive = false
            },
            new User
            {
                Id = 8,
                Forename = "Edward",
                Surname = "Malus",
                Email = "emalus@example.com",
                IsActive = false
            },
            new User
            {
                Id = 9,
                Forename = "Damon",
                Surname = "Macready",
                Email = "dmacready@example.com",
                IsActive = false
            },
            new User
            {
                Id = 10,
                Forename = "Johnny",
                Surname = "Blaze",
                Email = "jblaze@example.com",
                IsActive = true
            },
            new User
            {
                Id = 11,
                Forename = "Robin",
                Surname = "Feld",
                Email = "rfeld@example.com",
                IsActive = true
            }
        );

        model.Entity<Log>().HasData(
            new Log { Id = 1, UserId = 1, Timestamp = new DateTime(2025, 7, 18, 14, 23, 45, 123), ActionType = ActionType.AddUser, Details = "Test text #12" },
            new Log { Id = 2, UserId = 2, Timestamp = new DateTime(2025, 7, 18, 9, 15, 30, 456), ActionType = ActionType.DeleteUser, Details = "Test text #34" },
            new Log { Id = 3, UserId = 3, Timestamp = new DateTime(2025, 7, 18, 22, 5, 12, 789), ActionType = ActionType.UpdateUser, Details = "Test text #56" },
            new Log { Id = 4, UserId = 4, Timestamp = new DateTime(2025, 7, 18, 6, 47, 59, 321), ActionType = ActionType.ViewUser, Details = "Test text #78" },
            new Log { Id = 5, UserId = 5, Timestamp = new DateTime(2025, 7, 18, 17, 33, 10, 654), ActionType = ActionType.DeleteUser, Details = "Test text #90" },
            new Log { Id = 6, UserId = 6, Timestamp = new DateTime(2025, 7, 18, 11, 12, 25, 987), ActionType = ActionType.ViewUser, Details = "Test text #21" },
            new Log { Id = 7, UserId = 7, Timestamp = new DateTime(2025, 7, 18, 19, 44, 38, 210), ActionType = ActionType.UpdateUser, Details = "Test text #43" },
            new Log { Id = 8, UserId = 8, Timestamp = new DateTime(2025, 7, 18, 3, 29, 50, 333), ActionType = ActionType.DeleteUser, Details = "Test text #65" },
            new Log { Id = 9, UserId = 9, Timestamp = new DateTime(2025, 7, 18, 15, 55, 5, 876), ActionType = ActionType.AddUser, Details = "Test text #87" },
            new Log { Id = 10, UserId = 10, Timestamp = new DateTime(2025, 8, 17, 8, 8, 8, 888), ActionType = ActionType.UpdateUser, Details = "Test text #11" },
            new Log { Id = 11, UserId = 1, Timestamp = new DateTime(2025, 8, 17, 10, 22, 33, 111), ActionType = ActionType.ViewUser, Details = "Test text #22" },
            new Log { Id = 12, UserId = 2, Timestamp = new DateTime(2025, 8, 17, 12, 34, 56, 222), ActionType = ActionType.UpdateUser, Details = "Test text #33" },
            new Log { Id = 13, UserId = 3, Timestamp = new DateTime(2025, 8, 17, 14, 46, 19, 333), ActionType = ActionType.AddUser, Details = "Test text #44" },
            new Log { Id = 14, UserId = 4, Timestamp = new DateTime(2025, 8, 17, 16, 58, 42, 444), ActionType = ActionType.AddUser, Details = "Test text #55" },
            new Log { Id = 15, UserId = 5, Timestamp = new DateTime(2025, 8, 17, 18, 10, 5, 555), ActionType = ActionType.AddUser, Details = "Test text #66" },
            new Log { Id = 16, UserId = 6, Timestamp = new DateTime(2025, 8, 17, 20, 21, 28, 666), ActionType = ActionType.UpdateUser, Details = "Test text #77" },
            new Log { Id = 17, UserId = 7, Timestamp = new DateTime(2025, 8, 17, 22, 33, 51, 777), ActionType = ActionType.ViewUser, Details = "Test text #88" },
            new Log { Id = 18, UserId = 8, Timestamp = new DateTime(2025, 8, 17, 0, 45, 14, 888), ActionType = ActionType.AddUser, Details = "Test text #99" },
            new Log { Id = 19, UserId = 9, Timestamp = new DateTime(2025, 8, 17, 2, 56, 37, 999), ActionType = ActionType.DeleteUser, Details = "Test text #10" },
            new Log { Id = 20, UserId = 10, Timestamp = new DateTime(2025, 8, 17, 4, 8, 0, 101), ActionType = ActionType.DeleteUser, Details = "Test text #20" },
            new Log { Id = 21, UserId = 1, Timestamp = new DateTime(2025, 8, 27, 6, 19, 23, 202), ActionType = ActionType.ViewUser, Details = "Test text #30" },
            new Log { Id = 22, UserId = 2, Timestamp = new DateTime(2025, 8, 27, 8, 30, 46, 303), ActionType = ActionType.ViewUser, Details = "Test text #40" },
            new Log { Id = 23, UserId = 3, Timestamp = new DateTime(2025, 8, 27, 10, 42, 9, 404), ActionType = ActionType.UpdateUser, Details = "Test text #50" },
            new Log { Id = 24, UserId = 4, Timestamp = new DateTime(2025, 8, 27, 12, 53, 32, 505), ActionType = ActionType.AddUser, Details = "Test text #60" },
            new Log { Id = 25, UserId = 5, Timestamp = new DateTime(2025, 8, 27, 14, 4, 55, 606), ActionType = ActionType.AddUser, Details = "Test text #70" },
            new Log { Id = 26, UserId = 6, Timestamp = new DateTime(2025, 8, 27, 16, 16, 18, 707), ActionType = ActionType.AddUser, Details = "Test text #80" },
            new Log { Id = 27, UserId = 7, Timestamp = new DateTime(2025, 8, 27, 18, 27, 41, 808), ActionType = ActionType.DeleteUser, Details = "Test text #90" },
            new Log { Id = 28, UserId = 8, Timestamp = new DateTime(2025, 8, 27, 20, 39, 4, 909), ActionType = ActionType.ViewUser, Details = "Test text #15" },
            new Log { Id = 29, UserId = 9, Timestamp = new DateTime(2025, 8, 27, 22, 50, 27, 111), ActionType = ActionType.ViewUser, Details = "Test text #25" },
            new Log { Id = 30, UserId = 10, Timestamp = new DateTime(2025, 8, 27, 1, 1, 50, 222), ActionType = ActionType.ViewUser, Details = "Test text #35" },
            new Log { Id = 31, UserId = 1, Timestamp = new DateTime(2025, 9, 1, 3, 13, 13, 333), ActionType = ActionType.AddUser, Details = "Test text #45" },
            new Log { Id = 32, UserId = 2, Timestamp = new DateTime(2025, 9, 1, 5, 24, 36, 444), ActionType = ActionType.DeleteUser, Details = "Test text #55" },
            new Log { Id = 33, UserId = 3, Timestamp = new DateTime(2025, 9, 1, 7, 35, 59, 555), ActionType = ActionType.UpdateUser, Details = "Test text #65" },
            new Log { Id = 34, UserId = 4, Timestamp = new DateTime(2025, 9, 1, 4, 33, 22, 346), ActionType = ActionType.UpdateUser, Details = "Test text #75" },
            new Log { Id = 35, UserId = 5, Timestamp = new DateTime(2025, 9, 1, 1, 22, 20, 653), ActionType = ActionType.DeleteUser, Details = "Test text #85" }
            );

        model.ApplyConfigurationsFromAssembly(typeof(UserManagementDbContext).Assembly);
    }

    public DbSet<User>? Users { get; set; }
    public DbSet<Log>? Logs { get; set; }
}
