using System.Collections.Generic;
using System.Linq;
using UserManagement.Data.Entities;
using UserManagement.Services.Domain.Models;

namespace UserManagement.Services.Mapping;

public static class UserMapper
{
    public static UserModel MapToUserModel(User user)
        => new UserModel
        {
            Id = user.Id,
            Forename = user.Forename,
            Surname = user.Surname,
            Email = user.Email,
            IsActive = user.IsActive,
            DateOfBirth = user.DateOfBirth
        };

    public static List<UserModel> MapToUserModelList(List<User> users)
        => users.Select(u =>
                new UserModel
                {
                    Id = u.Id,
                    Forename = u.Forename,
                    Surname = u.Surname,
                    Email = u.Email,
                    IsActive = u.IsActive,
                    DateOfBirth = u.DateOfBirth
                })
            .ToList();

    public static User MapToUserEntity(UserModel model)
        => new User
        {
            Id = model.Id,
            Forename = model.Forename,
            Surname = model.Surname,
            Email = model.Email,
            IsActive = model.IsActive,
            DateOfBirth = model.DateOfBirth
        };
}
