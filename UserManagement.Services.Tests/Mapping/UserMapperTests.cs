using System;
using System.Collections.Generic;
using UserManagement.Data.Entities;
using UserManagement.Services.Domain.Models;
using UserManagement.Services.Mapping;

namespace UserManagement.Services.Tests.Mapping
{
    public class UserMapperTests
    {
        [Fact]
        public void MapToUserModel_ValidUser_ReturnsCorrectUserModel()
        {
            // Arrange
            var user = new User
            {
                Id = 1,
                Forename = "John",
                Surname = "Doe",
                Email = "john.doe@example.com",
                IsActive = true,
                DateOfBirth = DateOnly.FromDateTime(new DateTime(1990, 1, 1))
            };

            // Act
            var result = UserMapper.MapToUserModel(user);

            // Assert
            result.Id.Should().Be(user.Id);
            result.Forename.Should().Be(user.Forename);
            result.Surname.Should().Be(user.Surname);
            result.Email.Should().Be(user.Email);
            result.IsActive.Should().Be(user.IsActive);
            result.DateOfBirth.Should().Be(user.DateOfBirth);
        }

        [Fact]
        public void MapToUserModelList_ValidUserList_ReturnsCorrectUserModelList()
        {
            // Arrange
            var users = new List<User>
            {
                new User
                {
                    Id = 1,
                    Forename = "a",
                    Surname = "b",
                    Email = "a@b.com",
                    IsActive = true,
                    DateOfBirth = DateOnly.FromDateTime(new DateTime(1990, 1, 1))
                },
                new User
                {
                    Id = 2,
                    Forename = "c",
                    Surname = "d",
                    Email = "c@d.com",
                    IsActive = false,
                    DateOfBirth = DateOnly.FromDateTime(new DateTime(1985, 5, 15))
                }
            };

            // Act
            var result = UserMapper.MapToUserModelList(users);

            // Assert
            result.Should().HaveCount(users.Count);
            for (var i = 0; i < users.Count; i++)
            {
                result[i].Id.Should().Be(users[i].Id);
                result[i].Forename.Should().Be(users[i].Forename);
                result[i].Surname.Should().Be(users[i].Surname);
                result[i].Email.Should().Be(users[i].Email);
                result[i].IsActive.Should().Be(users[i].IsActive);
                result[i].DateOfBirth.Should().Be(users[i].DateOfBirth);
            }
        }

        [Fact]
        public void MapToUserEntity_ValidUserModel_ReturnsCorrectUser()
        {
            // Arrange
            var userModel = new UserModel
            {
                Id = 1,
                Forename = "John",
                Surname = "Doe",
                Email = "john.doe@example.com",
                IsActive = true,
                DateOfBirth = DateOnly.FromDateTime(new DateTime(1990, 1, 1))
            };

            // Act
            var result = UserMapper.MapToUserEntity(userModel);

            // Assert
            result.Id.Should().Be(userModel.Id);
            result.Forename.Should().Be(userModel.Forename);
            result.Surname.Should().Be(userModel.Surname);
            result.Email.Should().Be(userModel.Email);
            result.IsActive.Should().Be(userModel.IsActive);
            result.DateOfBirth.Should().Be(userModel.DateOfBirth);
        }

        [Fact]
        public void MapToUserModelList_EmptyList_ReturnsEmptyList()
        {
            // Arrange
            var users = new List<User>();

            // Act
            var result = UserMapper.MapToUserModelList(users);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }
    }
}
