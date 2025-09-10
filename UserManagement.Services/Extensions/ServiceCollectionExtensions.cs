using Hangfire;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using UserManagement.Services.Filters;
using UserManagement.Services.Interfaces;

namespace UserManagement.Services.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDomainServices(
        this IServiceCollection services,
        IConfiguration configuration)
        => services
            .AddScoped<IUserService, UserService>()
            .AddScoped<ILogService, LogService>()
            .AddScoped<ApiExceptionFilter>()
            .AddSerilog()
            .AddHangfire(config => config
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                .UseColouredConsoleLogProvider()
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseSqlServerStorage(configuration.GetConnectionString("UserManagementDb")))
            .AddHangfireServer();
}
