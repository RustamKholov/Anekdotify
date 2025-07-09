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

builder.Services.AddHttpClient<ApiClient>(client => client.BaseAddress = new Uri(jokeStoreApiUrl));
builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient(builder.Configuration["JokeStoreApiUrl"]!));
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();
builder.Services.AddScoped<INavigationHistoryService, NavigationHistoryService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseStaticFiles();
app.UseAntiforgery();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorComponents<Anekdotify.Frontend.Components.App>()
    .AddInteractiveServerRenderMode();

app.Run();
