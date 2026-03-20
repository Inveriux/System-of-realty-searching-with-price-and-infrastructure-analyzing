using UserService.Services.JwtProviders.Abstractions;
using UserService.Services.AdminSeeder.Abstractions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.Google;
using UserService.Data.Repositiroes.Abstractions;
using Microsoft.AspNetCore.Authentication.OAuth;
using BasicBuildingBlocks.Services.Abstractions;
using BasicBuildingBlocks.Endpoints.Extensions;
using Microsoft.AspNetCore.Authorization;
using BasicBuildingBlocks.Authorization;
using UserService.Services.JwtProviders;
using UserService.Services.AdminSeeder;
using UserService.Services.Images.Abstractions;
using UserService.Services.Images;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Identity;
using UserService.Data.Repositiroes;
using Microsoft.EntityFrameworkCore;
using BasicBuildingBlocks.Behaviors;
using Microsoft.Extensions.Options;
using BasicBuildingBlocks.Handlers;
using BasicBuildingBlocks.Services;
using UserService.Authorization;
using UserService.Models.Enums;
using System.Security.Claims;
using UserService.Models;
using System.Reflection;
using FluentValidation;
using UserService.Data;
using System.Text;
using MediatR;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAutoMapper(cfg => cfg.AddMaps(Assembly.GetExecutingAssembly()));
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
    cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>)); 
});
builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

builder.Services.AddDbContext<ApplicationDbContext>((sp, options) =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("PostgreSqlConnection"))
           .UseSeedingAsync(sp)
           .UseSnakeCaseNamingConvention());

builder.Services.AddIdentityCore<User>()
    .AddRoles<IdentityRole<Guid>>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.Configure<IdentityOptions>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;

    options.User.RequireUniqueEmail = true;
});

#region Authentication

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer()
    .AddCookie(IdentityConstants.ExternalScheme)
    .AddGoogle();

builder.Services.AddHttpContextAccessor();

var jwtSettings = builder.Services
    .AddOptions<JwtSettingsOptions>()
    .BindConfiguration(JwtSettingsOptions.SectionName)
    // .Validate(o => o.Authority is not null)
    // .Validate(o => o.Audience is not null)
    // .Validate(o => o.Key is not null)
    .ValidateOnStart();

var googleAuthSettings = builder.Services
    .AddOptions<GoogleAuthOptions>()
    .BindConfiguration(GoogleAuthOptions.SectionName)
    // .Validate(o => o.ClientId is not null)
    // .Validate(o => o.ClientSecret is not null)
    .ValidateOnStart();

builder.Services
    .AddOptions<ImageServiceOptions>()
    .BindConfiguration(ImageServiceOptions.SectionName)
    .Validate(o => !string.IsNullOrWhiteSpace(o.BaseUrl), "Image service base url is required.")
    .ValidateOnStart();

var service = builder.Services
    .AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
    .Configure<IOptions<JwtSettingsOptions>>((options, settings) => {

        options.IncludeErrorDetails = true;
        options.Audience = settings.Value.Audience;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = settings.Value.Authority,
            ValidateAudience = true,
            ValidAudience = settings.Value.Audience,
            ValidateLifetime = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.Value.Key)),
            ValidateIssuerSigningKey = true,
            ClockSkew = TimeSpan.Zero
        };

        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = context =>
            {
                var user = context.Principal;

                if (user?.Identity?.IsAuthenticated != true)
                {
                    context.Fail("User is not authenticated.");
                    return Task.CompletedTask;
                }

                var userId = user.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                    ?? user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var email = user.FindFirst(ClaimTypes.Email)?.Value;
                var name = user.FindFirst(ClaimTypes.Name)?.Value;
                var role = user.FindFirst(ClaimTypes.Role)?.Value;

                if (string.IsNullOrWhiteSpace(userId) ||
                    string.IsNullOrWhiteSpace(email) ||
                    string.IsNullOrWhiteSpace(name) ||
                    string.IsNullOrWhiteSpace(role))
                {
                    context.Fail("Required claims are missing.");
                    return Task.CompletedTask;
                }
                if (!Guid.TryParse(userId, out _))
                {
                    context.Fail("User id claim is invalid.");
                }

                return Task.CompletedTask;
            },

            OnAuthenticationFailed = context =>
            {
                context.NoResult();
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                context.Response.Headers.WWWAuthenticate =
                    $"Bearer error=\"invalid_token\", error_description=\"{context.Exception.Message}\"";
                return Task.CompletedTask;
            },
        };
    });

builder.Services.AddOptions<GoogleOptions>(GoogleDefaults.AuthenticationScheme)
    .Configure<IOptions<GoogleAuthOptions>>((options, settings) => {
        options.SignInScheme = IdentityConstants.ExternalScheme;
        options.ClientId = settings.Value.ClientId;
        options.ClientSecret = settings.Value.ClientSecret;
        options.CallbackPath = settings.Value.CallbackPath;

        options.Events = new OAuthEvents
        {
            OnRemoteFailure = context =>
            {
                context.HandleResponse();
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                context.Response.Headers.WWWAuthenticate =
                    $"Google error=\"{context?.Failure?.Message}\"";
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddScoped<IRefreshTokenProvider, RefreshTokenProvider>();
builder.Services.AddHttpClient<IImageServiceClient, ImageServiceClient>();
builder.Services.AddSingleton<IDomainJwtProvider, DomainJwtProvider>((services) =>
{
    var settings = services.GetRequiredService<IOptions<JwtSettingsOptions>>().Value;;
    return new DomainJwtProvider(settings.Authority, settings.Audience, settings.Key);
});

builder.Services.AddSingleton<ICurrentUserProvider, CurrentUserProvider>();

#endregion 

#region Authorization

builder.Services.AddScoped<IOwnerResourceAccessor<string>, UserEmailOwnerResourceAccessor>();
builder.Services.AddScoped<IAuthorizationHandler, OwnerOrAdminAccessAuthorizationHandler<string>>();

builder.Services.AddAuthorization(cfg =>
{
    cfg.AddPolicy(PolicyNames.User, policy => 
        { 
            policy.RequireClaim(ClaimTypes.Email);
            policy.RequireClaim(ClaimTypes.Name);
            policy.RequireRole(PolicyNames.User);
        });
    cfg.AddPolicy(PolicyNames.Admin, policy => 
        { 
            policy.RequireClaim(ClaimTypes.Email);
            policy.RequireClaim(ClaimTypes.Name);
            policy.RequireRole(PolicyNames.Admin);
        });
    cfg.AddPolicy(PolicyNames.Agent, policy => 
        { 
            policy.RequireClaim(ClaimTypes.Email);
            policy.RequireClaim(ClaimTypes.Name);
            policy.RequireRole(PolicyNames.Agent);
        });
    cfg.AddPolicy(PolicyNames.OwnerOrAdminAccess, policy =>
        {
            policy.RequireClaim(ClaimTypes.Email);
            policy.RequireClaim(ClaimTypes.Name);
            policy.RequireRole(UserRoles.Admin, UserRoles.User, UserRoles.Agent);
        });
}).AddAuthorizationBuilder();

builder.Services.AddScoped<IAdminSeeder, AdminSeeder>();

#endregion

builder.Services.AddScoped<IUserRepository, UserRepository>();

builder.Services.AddExceptionHandler<ExceptionHandler>();
builder.Services.AddOpenApi();
builder.Services.AddCors(options =>
{
    options.AddPolicy("WebCors", policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

builder.Services.AddScoped<ApplicationDatabaseInitializer>();

var app = builder.Build();

if(app.Environment.IsDevelopment())
    await app.InitializeTestDatabaseAsync(false);

if(app.Environment.IsProduction())
{
    var isDeletedata = app.Configuration.GetValue<bool>("DeleteDataAtStartup");
    await app.InitializeTestDatabaseAsync(isDeletedata);
}

app.UseCors("WebCors");
app.UseAuthentication();
app.UseAuthorization();

//app.UseExceptionHandler(_ => {});

//app.UseHttpsRedirection();
app.MapOpenApi();
app.MapEndpoints(Assembly.GetExecutingAssembly());
app.MapGet("/", () => "User sevice is work!");

app.Run();


public class JwtSettingsOptions
{
    public static readonly string SectionName = "Authentication:DomainJwtAuth";

    public required string Authority { get; set; }
    public required string Audience { get; set; }
    public required string Key { get; set; }
}

public class GoogleAuthOptions
{
    public static readonly string SectionName = "Authentication:GoogleAuth";

    public required string ClientId { get; set; }
    public required string ClientSecret { get; set; }
    public required string CallbackPath { get; set; } = "/api/users/signin-google";
}
