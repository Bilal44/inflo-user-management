using System;
using System.Collections.Generic;
using UserManagement.Data.Entities;
using UserManagement.Data.Entities.Enums;
using UserManagement.Services.Domain.Models;
using UserManagement.Services.Mapping;

namespace UserManagement.Services.Tests.Mapping
{
    public class LogMapperTests
    {
        [Fact]
        public void MapToLogModel_ValidLogEntity_ReturnsCorrectLogModel()
        {
            // Arrange
            var log = new Log
            {
                Id = 1,
                UserId = 123,
                Timestamp = DateTime.Now,
                ActionType = ActionType.AddUser,
                Details = "User logged in successfully"
            };

            // Act
            var result = LogMapper.MapToLogModel(log);

            // Assert
            result.Id.Should().Be(log.Id);
            result.UserId.Should().Be(log.UserId);
            result.Timestamp.Should().Be(log.Timestamp);
            result.ActionType.Should().Be(log.ActionType);
            result.Details.Should().Be(log.Details);
        }

        [Fact]
        public void MapToPaginatedLogModelList_ValidLogsAndFilter_ReturnsPaginatedList()
        {
            // Arrange
            var filter = new PaginationFilter { CurrentPage = 1, PageSize = 10 };
            var totalPages = 5;
            var logs = new List<Log>
            {
                new Log { Id = 1, UserId = 123, Timestamp = DateTime.Now, ActionType = ActionType.UpdateUser, Details = "Login action" },
                new Log { Id = 2, UserId = 456, Timestamp = DateTime.Now, ActionType = ActionType.DeleteUser, Details = "Logout action" }
            };

            // Act
            var result = LogMapper.MapToPaginatedLogModelList(filter, totalPages, logs);

            // Assert
            result.PaginationFilter.Should().Be(filter);
            result.TotalPages.Should().Be(totalPages);
            result.Data.Should().HaveCount(2);
            result.Data[0].Id.Should().Be(logs[0].Id);
            result.Data[1].Id.Should().Be(logs[1].Id);
        }

        [Fact]
        public void MapToLogEntity_ValidLogModel_ReturnsCorrectLogEntity()
        {
            // Arrange
            var logModel = new LogModel
            {
                Id = 1,
                UserId = 123,
                Timestamp = DateTime.Now,
                ActionType = ActionType.ViewUser,
                Details = "User logged in successfully"
            };

            // Act
            var result = LogMapper.MapToLogEntity(logModel);

            // Assert
            result.Id.Should().Be(logModel.Id);
            result.UserId.Should().Be(logModel.UserId);
            result.Timestamp.Should().Be(logModel.Timestamp);
            result.ActionType.Should().Be(logModel.ActionType);
            result.Details.Should().Be(logModel.Details);
        }

        [Fact]
        public void MapToPaginatedLogModelList_EmptyLogsList_ReturnsEmptyPaginatedList()
        {
            // Arrange
            var filter = new PaginationFilter { CurrentPage = 1, PageSize = 10 };
            var totalPages = 0;
            var logs = new List<Log>();

            // Act
            var result = LogMapper.MapToPaginatedLogModelList(filter, totalPages, logs);

            // Assert
            result.PaginationFilter.Should().Be(filter);
            result.TotalPages.Should().Be(totalPages);
            result.Data.Should().BeEmpty();
        }
    }
}
