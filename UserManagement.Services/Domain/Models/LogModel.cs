using System;
using System.ComponentModel;
using UserManagement.Data.Entities.Enums;

namespace UserManagement.Services.Domain.Models;

public record LogModel
{
    public long Id { get; init; }

    [DisplayName("User Id")]
    public long UserId { get; init; }
    public DateTime Timestamp { get; init; }

    [DisplayName("Action")]
    public ActionType ActionType { get; init; }
    public string? Details { get; init; }
}
