using Hangfire;
using Hangfire.Dashboard.BasicAuthorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using UserManagement.Data.Context;
using UserManagement.Data.Extensions;
using UserManagement.Services.Extensions;
using Westwind.AspNetCore.Markdown;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services
    .AddDataAccess(builder.Configuration)
    .AddDomainServices(builder.Configuration)
    .AddMarkdown()
    .Configure<RouteOptions>(options =>
    {
        options.LowercaseUrls = true;
        options.LowercaseQueryStrings = true;
    })
    .AddControllersWithViews();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<UserManagementDbContext>();
    if (dbContext.Database.IsSqlServer())
        dbContext.Database.Migrate();
}


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

app.UseMarkdown();
app.UseHangfireDashboard(options: options);
app.UseHsts();
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.MapHangfireDashboard();
app.MapDefaultControllerRoute();
app.Run();
