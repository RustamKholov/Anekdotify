using Anekdotify.BL.Repositories;
using Anekdotify.Database.Data;
using Anekdotify.Models.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Microsoft.AspNetCore.DataProtection;
using Anekdotify.BL.Services;
using Anekdotify.BL.Interfaces.Repositories;
using Anekdotify.BL.Interfaces.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.


builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Anekdotify API",
        Version = "v1",
        Description = "An ASP.NET Core Web API for sharing anekdots."
    });
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter a valid token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type=ReferenceType.SecurityScheme,
                    Id="Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(@"C:\Users\roshf\Anekdotify\Anekdotify.Database\Data\"));

builder.Services.AddControllers().
    AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
    });

builder.Services.AddDbContext<ApplicationDBContext>(options =>
    {
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
    }
    );

builder.Services.AddScoped<IJokeService, JokeService>();
builder.Services.AddScoped<ICommentService, CommentService>();
builder.Services.AddScoped<IClassifficationService, ClassifficationService>();
builder.Services.AddScoped<IJokeRatingsService, JokeRatingsService>();
builder.Services.AddScoped<IUserSavedJokeService, UserSavedJokeService>();
builder.Services.AddScoped<IUserViewedJokesService, UserViewedJokesService>();
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<ISourceFetchedJokesService, SourceFetchedJokesService>();
builder.Services.AddScoped<IJokeCacheService, JokeCacheService>();


builder.Services.AddScoped<IJokeRepository, JokeRepository>();
builder.Services.AddScoped<ICommentRepository, CommentRepository>();
builder.Services.AddScoped<IUserSavedJokeRepository, UserSavedJokeRepository>();
builder.Services.AddScoped<IJokeRatingsRepository, JokeRatingsRepository>();
builder.Services.AddScoped<IClassificationRepository, ClassificationRepository>();
builder.Services.AddScoped<IUserViewedJokesRepository, UserViewedJokesRepository>();
builder.Services.AddScoped<IAccountRepository, AccountRepository>();
builder.Services.AddScoped<ISourceFetchedJokesRepository, SourceFetchedJokesRepository>();

builder.Services.AddScoped<ITokenService, TokenService>();

builder.Services.AddScoped<IJokeSeederService, JokeSeederService>();

builder.Services.AddHttpClient<IJokeSource, JokeSourceJokeApi>(client =>
{
    client.BaseAddress = new Uri("https://v2.jokeapi.dev/");
});

builder.Services.AddIdentity<User, IdentityRole>(options =>
    {
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = true;
        options.Password.RequiredLength = 12;
    }
).AddEntityFrameworkStores<ApplicationDBContext>();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme =
    options.DefaultChallengeScheme =
    options.DefaultForbidScheme =
    options.DefaultScheme =
    options.DefaultSignInScheme =
    options.DefaultSignOutScheme = JwtBearerDefaults.AuthenticationScheme;

}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["JWT:Issuer"],
        ValidateAudience = true,
        ValidAudience = builder.Configuration["JWT:Audience"],
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(
            System.Text.Encoding.UTF8.GetBytes(
                builder.Configuration["JWT:SigningKey"] ?? throw new InvalidOperationException("JWT:SigningKey is not configured.")
            )
        )
    };
});

builder.AddRedisDistributedCache("cache");

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Anekdotify API v1");
    });
}
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();
