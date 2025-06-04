using frontend.Clients;
using frontend.Components;
using frontend.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var jokeStoreApiUrl = builder.Configuration["JokeStoreApiUrl"] ?? throw new Exception("Joke Store Api Url is not set");

builder.Services.AddHttpClient<JokesClient>(client => client.BaseAddress = new Uri(jokeStoreApiUrl));
builder.Services.AddHttpClient<CommentClient>(client => client.BaseAddress = new Uri(jokeStoreApiUrl));

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

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
