using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UserManagement.Data.Context;
using UserManagement.Data.Repositories;
using UserManagement.Data.Repositories.Interfaces;

namespace UserManagement.Data.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDataAccess(this IServiceCollection services, IConfiguration configuration)
        => services.AddDbContext<UserManagementDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("UserManagementDb")))
                .AddScoped(typeof(IRepository<>), typeof(Repository<>));
}
