using System.Collections.Generic;

namespace UserManagement.Services.Domain.Models;

public class PaginatedList<T>
{
    public PaginationFilter PaginationFilter { get; set; } = new PaginationFilter();
    public int TotalPages { get; set; }
    public List<T> Data { get; set; } = new List<T>();
}
