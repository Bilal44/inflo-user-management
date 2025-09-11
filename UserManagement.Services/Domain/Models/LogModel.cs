using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using UserManagement.Data.Entities.Enums;

namespace UserManagement.Services.Domain.Models;

public record LogModel
{
    public long Id { get; init; }

    [DisplayName("User Id")]
    public long UserId { get; init; }

    [DisplayName("Timestamp (UTC)")]
    [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm:ss}")]
    public DateTime Timestamp { get; init; }

    [DisplayName("Action")]
    public ActionType ActionType { get; init; }
    public string? Details { get; init; }
}
