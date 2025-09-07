using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UserManagement.Data.Entities;

namespace UserManagement.Services.Interfaces;

public interface IUserService
{
    Task<List<User>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<User?> GetByIdAsync(long id);

    /// <summary>
    /// Return users by active state
    /// </summary>
    /// <param name="isActive"></param>
    /// <returns></returns>
    Task<List<User>> FilterByActiveAsync(bool isActive = default, CancellationToken cancellationToken = default);
    Task AddAsync(User user);
    Task UpdateAsync(User user);
    Task<int> DeleteAsync(long id);
}
