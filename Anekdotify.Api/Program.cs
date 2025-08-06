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
using Serilog;

public class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.

        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day)
            .CreateLogger();

        builder.Host.UseSerilog();

        try
        {
            Log.Information("Starting up Anekdotify API...");


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
                .PersistKeysToFileSystem(new DirectoryInfo("/app/keys")).SetApplicationName("AnekdotifyAPI");

            builder.Services.AddControllers().
                AddNewtonsoftJson(options =>
                {
                    options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                });

            builder.Services.AddDbContext<ApplicationDBContext>(options =>
                {
                    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
                }
                );

            builder.Services.AddScoped<IJokeService, JokeService>();
            builder.Services.AddScoped<ICommentService, CommentService>();
            builder.Services.AddScoped<IClassifficationService, ClassifficationService>();
            builder.Services.AddScoped<IJokeRatingsService, JokeRatingsService>();
            builder.Services.AddScoped<ICommentRatingService, CommentRatingService>();
            builder.Services.AddScoped<IUserSavedJokeService, UserSavedJokeService>();
            builder.Services.AddScoped<IUserViewedJokesService, UserViewedJokesService>();
            builder.Services.AddScoped<IAccountService, AccountService>();
            builder.Services.AddScoped<ISourceFetchedJokesService, SourceFetchedJokesService>();
            builder.Services.AddScoped<IJokeCacheService, JokeCacheService>();


            builder.Services.AddScoped<IJokeRepository, JokeRepository>();
            builder.Services.AddScoped<ICommentRepository, CommentRepository>();
            builder.Services.AddScoped<IUserSavedJokeRepository, UserSavedJokeRepository>();
            builder.Services.AddScoped<IJokeRatingsRepository, JokeRatingsRepository>();
            builder.Services.AddScoped<ICommentRatingRepository, CommentRatingRepository>();
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
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowFrontend", policy =>
                {
                    policy.WithOrigins(
                            builder.Configuration["Frontend:BaseUrl"] ?? "https://anekdotify.at",
                            "http://localhost:5173", // Vite dev server
                            "https://localhost:5173"
                        )
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                });
            });
            builder.Services.AddAuthentication(options =>
            {

                // Default schemes for API authentication
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

                // Sign-in scheme for OAuth flows
                options.DefaultSignInScheme = IdentityConstants.ExternalScheme;

            }).AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
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
                            builder.Configuration["JWT:SigningKey"] ??
                            throw new InvalidOperationException("JWT:SigningKey is not configured.")
                        )
                    ),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };
            })
            .AddGitHub("GitHub", options =>
            {
                options.ClientId = builder.Configuration["OAuth:GitHub:ClientId"] ??
                    throw new InvalidOperationException("GitHub ClientId not configured");
                options.ClientSecret = builder.Configuration["OAuth:GitHub:ClientSecret"] ??
                    throw new InvalidOperationException("GitHub ClientSecret not configured");

                options.CallbackPath = "/api/auth/github/callback";
                options.Scope.Add("user:email");
                options.SignInScheme = IdentityConstants.ExternalScheme;
                options.SaveTokens = false;

                // Remove or simplify the event handler
                options.Events.OnCreatingTicket = async context =>
                {
                    var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                    logger.LogInformation("GitHub OnCreatingTicket event fired");
                    await Task.CompletedTask;
                };
            })
            .AddGoogle("Google", options =>
            {
                options.ClientId = builder.Configuration["OAuth:Google:ClientId"] ??
                    throw new InvalidOperationException("Google ClientId not configured");
                options.ClientSecret = builder.Configuration["OAuth:Google:ClientSecret"] ??
                    throw new InvalidOperationException("Google ClientSecret not configured");

                options.CallbackPath = "/api/auth/google/callback";
                options.SignInScheme = IdentityConstants.ExternalScheme;
                options.SaveTokens = false;

                // Remove or simplify the event handler
                options.Events.OnCreatingTicket = async context =>
                {
                    var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                    logger.LogInformation("Google OnCreatingTicket event fired");
                    await Task.CompletedTask;
                };
            });

            builder.AddRedisDistributedCache("redis");

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Anekdotify API v1");
                });
            }

            app.UseCors("AllowFrontend");
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            Log.Information("Anekdotify API started successfully.");
            app.Run();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Anekdotify API terminated unexpectedly!");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }
}