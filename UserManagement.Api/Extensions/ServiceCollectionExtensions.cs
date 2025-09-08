using UserManagement.Services.Filters;

namespace UserManagement.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApiServices(this IServiceCollection services)
        => services.AddScoped<ApiExceptionFilter>()
            .AddOpenApi();
}
