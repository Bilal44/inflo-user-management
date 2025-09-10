using System.Globalization;
using Blazored.Toast;
using UserManagement.Client.Components;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services
    .AddBlazoredToast()
    .AddScoped(_ =>
    {
        var apiUrl = builder.Configuration["Api:BaseUrl"];
        var apiKey = builder.Configuration["Api:ApiKey"];

        var client = new HttpClient { BaseAddress = new Uri(apiUrl!) };

        client.DefaultRequestHeaders.Add("x-api-key", apiKey!);
        return client;
    });

var app = builder.Build();

// Add culture info for machine-agnostic deployments
var cultureInfo = new CultureInfo("en-GB");
CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

// Configure the HTTP request pipeline.
app.UseExceptionHandler("/Error", createScopeForErrors: true);
app.UseHsts();
app.UseHttpsRedirection();
app.UseAntiforgery();
app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
