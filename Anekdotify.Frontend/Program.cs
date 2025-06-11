using Anekdotify.Frontend.Authentication;
using Anekdotify.Frontend.Clients;
using Anekdotify.Frontend.Heplers;
using Anekdotify.Frontend.Components;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Components.Authorization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var jokeStoreApiUrl = builder.Configuration["JokeStoreApiUrl"] ?? throw new Exception("Joke Store Api Url is not set");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme =
    options.DefaultChallengeScheme =
    options.DefaultForbidScheme =
    options.DefaultScheme =
    options.DefaultSignInScheme =
    options.DefaultSignOutScheme = JwtBearerDefaults.AuthenticationScheme;

}).AddJwtBearer();


builder.Services.AddCascadingAuthenticationState();

builder.Services.AddHttpClient<JokesClient>(client => client.BaseAddress = new Uri(jokeStoreApiUrl));
builder.Services.AddHttpClient<CommentClient>(client => client.BaseAddress = new Uri(jokeStoreApiUrl));
builder.Services.AddHttpClient<ApiClient>(client => client.BaseAddress = new Uri(jokeStoreApiUrl));
builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient(builder.Configuration["JokeStoreApiUrl"]!));
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();

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

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
