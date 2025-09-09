using UserManagement.Client.Components;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .Services.AddScoped(s =>
    {
        var apiUrl = builder.Configuration["Api:BaseUrl"];
        var apiKey = builder.Configuration["Api:ApiKey"];

        var client = new HttpClient
        {
            BaseAddress = new Uri(apiUrl!)
        };

        client.DefaultRequestHeaders.Add("x-api-key", apiKey!);
        return client;
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler("/Error", createScopeForErrors: true);
app.UseHsts();

app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
