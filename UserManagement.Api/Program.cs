using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using UserManagement.Api.Authentication;
using UserManagement.Api.Extensions;
using UserManagement.Data.Context;
using UserManagement.Data.Extensions;
using UserManagement.Services.Extensions;
using UserManagement.Services.Filters;

namespace UserManagement.Api;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services
            .AddDataAccess(builder.Configuration)
            .AddDomainServices()
            .AddApiServices()
            .AddControllers(options => { options.Filters.Add<ApiExceptionFilter>(); })
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
            });

        // Add rate limiter
        builder.Services.AddRateLimiter(rateLimiterOptions =>
        {
            rateLimiterOptions.AddSlidingWindowLimiter("sliding", options =>
            {
                options.PermitLimit = 10;
                options.Window = TimeSpan.FromSeconds(10);
                options.SegmentsPerWindow = 2;
                options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                options.QueueLimit = 5;
            });

            rateLimiterOptions.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
        });

        builder.Services.AddHealthChecks();

        var app = builder.Build();

        using (var scope = app.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<UserManagementDbContext>();
            if (dbContext.Database.IsSqlServer())
                dbContext.Database.Migrate();
        }

        app.MapOpenApi();
        app.MapScalarApiReference();
        app.UseHsts();
        app.UseRateLimiter();
        app.UseHealthChecks("/");
        app.UseHttpsRedirection();
        app.UseMiddleware<AuthMiddleware>(); // API key authentication middleware
        app.MapControllers();
        app.Run();
    }
}
