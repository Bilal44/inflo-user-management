using System;
using UserManagement.Data.Entities.Enums;

namespace UserManagement.Data.Entities;

public class Log
{
    public long Id { get; set; }
    public DateTime Timestamp { get; set; }
    public ActionType ActionType { get; set; }
    public string? Details { get; set; }
    public long UserId { get; set; }
    public User? User { get; set; }
}
