using Anekdotify.Frontend.Authentication;
using Anekdotify.Frontend.Clients;
using Anekdotify.Frontend.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Blazored.Toast;
using Blazored.LocalStorage;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddBlazoredToast();
builder.Services.AddBlazorBootstrap();
builder.Services.AddBlazoredLocalStorage();

var jokeStoreApiUrl = builder.Configuration["JokeStoreApiUrl"] ?? throw new Exception("Joke Store Api Url is not set");

builder.Services.AddAuthentication();
builder.Services.AddCascadingAuthenticationState();

// Fix HttpClient configuration
builder.Services.AddHttpClient<ApiClient>(client =>
{
    client.BaseAddress = new Uri(jokeStoreApiUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
});

// Remove the problematic line and replace with proper HttpClient
builder.Services.AddScoped<HttpClient>(sp =>
{
    var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
    return httpClientFactory.CreateClient(nameof(ApiClient));
});

builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();
builder.Services.AddScoped<INavigationHistoryService, NavigationHistoryService>();
builder.Services.AddScoped<IOAuthService, OAuthService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStaticFiles();
app.UseAntiforgery();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorComponents<Anekdotify.Frontend.Components.App>()
    .AddInteractiveServerRenderMode();

app.Run();
