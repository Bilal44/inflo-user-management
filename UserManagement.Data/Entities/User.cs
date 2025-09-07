using System;
using System.Collections.Generic;

namespace UserManagement.Data.Entities;

public class User
{
    public long Id { get; set; }
    public string Forename { get; set; } = default!;
    public string Surname { get; set; } = default!;
    public string Email { get; set; } = default!;
    public bool IsActive { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public ICollection<Log> Logs { get; set; } = new List<Log>();
}
