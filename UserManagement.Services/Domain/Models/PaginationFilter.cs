using System;
using System.ComponentModel.DataAnnotations;

namespace UserManagement.Services.Domain.Models;

public class PaginationFilter
{
    public long? UserId { get; set; }
    public string? Search { get; set; }
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
    public string? SortBy { get; set; }
}
