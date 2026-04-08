using System.IdentityModel.Tokens.Jwt;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using BasicBuildingBlocks.Behaviors;
using BasicBuildingBlocks.Endpoints.Extensions;
using BasicBuildingBlocks.Handlers;
using BasicBuildingBlocks.Services;
using BasicBuildingBlocks.Services.Abstractions;
using ChatService.Clients.Listings;
using ChatService.Clients.Listings.Abstractions;
using ChatService.Clients.Users;
using ChatService.Clients.Users.Abstractions;
using ChatService;
using ChatService.Data;
using ChatService.Hubs;
using ChatService.Services;
using ChatService.Services.Abstractions;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAutoMapper(cfg => cfg.AddMaps(Assembly.GetExecutingAssembly()));
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
    cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
});
builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("PostgreSqlConnection"))
        .UseSnakeCaseNamingConvention());

builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<ICurrentUserProvider, CurrentUserProvider>();

builder.Services.AddOptions<JwtSettingsOptions>()
    .BindConfiguration(JwtSettingsOptions.SectionName)
    .ValidateOnStart();

builder.Services.AddOptions<ListingServiceOptions>()
    .BindConfiguration(ListingServiceOptions.SectionName)
    .Validate(x => !string.IsNullOrWhiteSpace(x.BaseUrl), "ListingService base url is required.")
    .Validate(x => x.TimeoutSeconds > 0, "ListingService timeout must be positive.")
    .ValidateOnStart();

builder.Services.AddOptions<AgentServiceOptions>()
    .BindConfiguration(AgentServiceOptions.SectionName)
    .Validate(x => !string.IsNullOrWhiteSpace(x.BaseUrl), "AgentService base url is required.")
    .Validate(x => x.TimeoutSeconds > 0, "AgentService timeout must be positive.")
    .ValidateOnStart();

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer();

builder.Services
    .AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
    .Configure<IOptions<JwtSettingsOptions>>((options, settings) =>
    {
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
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrWhiteSpace(accessToken) && path.StartsWithSegments("/hubs/chat"))
                {
                    context.Token = accessToken;
                }

                return Task.CompletedTask;
            },
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
            }
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddSignalR();

builder.Services.AddHttpClient<IListingCatalogClient, ListingCatalogClient>((serviceProvider, client) =>
{
    var options = serviceProvider.GetRequiredService<IOptions<ListingServiceOptions>>().Value;
    client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/");
    client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
});

builder.Services.AddHttpClient<IUserProfileClient, UserProfileClient>((serviceProvider, client) =>
{
    var options = serviceProvider.GetRequiredService<IOptions<AgentServiceOptions>>().Value;
    client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/");
    client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
});

builder.Services.AddScoped<IChatRealtimeNotifier, ChatRealtimeNotifier>();

builder.Services.AddExceptionHandler<ExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();
builder.Services.AddCors(options =>
{
    options.AddPolicy("DefaultCors", policy => policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});

var app = builder.Build();

app.UseCors("DefaultCors");
app.UseExceptionHandler();
app.UseAuthentication();
app.UseAuthorization();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await dbContext.Database.EnsureCreatedAsync();
}

app.MapOpenApi();
app.MapHub<ChatHub>("/hubs/chat");
app.MapEndpoints(Assembly.GetExecutingAssembly());
app.MapGet("/", () => "Chat Service is working.");

app.Run();
