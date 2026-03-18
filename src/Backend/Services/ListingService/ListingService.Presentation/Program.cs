using Microsoft.AspNetCore.Authentication.JwtBearer;
using ListingService.Presentation.Converters.Json;
using BasicBuildingBlocks.Authorization;
using BasicBuildingBlocks.Services.Abstractions;
using BasicBuildingBlocks.Endpoints.Extensions;
using ListingService.Infrastructure.Data;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using ListingService.Infrastructure;
using BasicBuildingBlocks.Handlers;
using BasicBuildingBlocks.Services;
using Microsoft.Extensions.Options;
using ListingService.Domain.Enums;
using ListingService.Application;
using ListingService.Presentation.Authorization;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Reflection;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.AddApplication();
builder.AddInfrastructure();

builder.Services.AddExceptionHandler<ExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();

builder.Services.ConfigureHttpJsonOptions(o =>
{
    o.SerializerOptions.Converters.Add(new CreateListingDtoJsonConverter());
    o.SerializerOptions.Converters.Add(new ListingDetailsDtoWriteConverter());
});


if(builder.Environment.IsProduction())
{
    builder.Services.AddHttpsRedirection(options =>
    {
        options.RedirectStatusCode = StatusCodes.Status308PermanentRedirect;
    });   
}

builder.Services.AddHttpContextAccessor();

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer();

var jwtSettings = builder.Services
    .AddOptions<JwtSettingsOptions>()
    .BindConfiguration(JwtSettingsOptions.SectionName)
    // .Validate(o => o.Authority is not null)
    // .Validate(o => o.Audience is not null)
    // .Validate(o => o.Key is not null)
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

builder.Services.AddScoped<IOwnerResourceAccessor<Guid>, ListingOwnerResourceAccessor>();
builder.Services.AddScoped<IAuthorizationHandler, OwnerOrAdminAccessAuthorizationHandler<Guid>>();
builder.Services.AddScoped<ICurrentUserProvider, CurrentUserProvider>();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.UseExceptionHandler();

if(builder.Environment.IsDevelopment())
{
    app.UseCors("DevCors");
    await app.InitializeTestDatabaseAsync(false);
}

if(app.Environment.IsDevelopment())
    await app.InitializeTestDatabaseAsync(false);

if(app.Environment.IsProduction())
{
    var isDeletedata = app.Configuration.GetValue<bool>("DeleteDataAtStartup");
    await app.InitializeTestDatabaseAsync(isDeletedata);
}

app.MapOpenApi();
app.MapEndpoints(Assembly.GetExecutingAssembly());
app.MapGet("/", () => "Listning Service is work.");

app.Run();

public class JwtSettingsOptions
{
    public static readonly string SectionName = "Authentication:DomainJwtAuth";

    public required string Authority { get; set; }
    public required string Audience { get; set; }
    public required string Key { get; set; }
}
