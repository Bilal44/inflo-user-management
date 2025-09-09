using System.Reflection;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using Hangfire;
using Hangfire.Dashboard.BasicAuthorization;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
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
            .AddDomainServices(builder.Configuration)
            .AddApiServices()
            .Configure<RouteOptions>(options =>
            {
                options.LowercaseUrls = true;
                options.LowercaseQueryStrings = true;
            })
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

        // Basic authorisation for Hangfire dashboard
        var options = new DashboardOptions
        {
            Authorization = new[]
            {
                new BasicAuthAuthorizationFilter(new BasicAuthAuthorizationFilterOptions
                {
                    SslRedirect = false,
                    RequireSsl = false,
                    LoginCaseSensitive = true,
                    Users = new []
                    {
                        new BasicAuthAuthorizationUser
                        {
                            Login = builder.Configuration["Hangfire:Credentials:Username"],
                            PasswordClear = builder.Configuration["Hangfire:Credentials:Password"]
                        }
                    }
                })
            }
        };

        builder.Services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Version = "v1",
                Title = "Info User Management API",
                Description = "A RESTful .NET 9 API facilitating user management and log querying for Inflo tech task.",
            });

            // Configure API key authentication on Swagger documentation page
            options.AddSecurityDefinition("API Key", new OpenApiSecurityScheme
            {
                Name = "x-api-key",
                Type = SecuritySchemeType.ApiKey,
                In = ParameterLocation.Header,
                Description = "Provide your API key in the request header."
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "API Key"
                        }
                    }, []
                }
            });

            var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
        });

        builder.Services.AddHealthChecks();

        var app = builder.Build();

        using (var scope = app.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<UserManagementDbContext>();
            if (dbContext.Database.IsSqlServer())
                dbContext.Database.Migrate();
        }

        app.UseHsts();
        app.MapOpenApi();
        app.MapScalarApiReference();
        app.UseSwagger();
        app.UseSwaggerUI();
        app.UseRateLimiter();
        app.UseHealthChecks("/");
        app.UseHttpsRedirection();
        app.UseMiddleware<AuthMiddleware>(); // API key authentication middleware
        app.UseHangfireDashboard(options: options);
        app.MapHangfireDashboard();
        app.MapControllers();
        app.Run();
    }
}
