using System.Reflection;
using BasicBuildingBlocks.Behaviors;
using BasicBuildingBlocks.Endpoints.Extensions;
using BasicBuildingBlocks.Handlers;
using FluentValidation;
using InfrastructureService.Data;
using InfrastructureService.Data.Repositories;
using InfrastructureService.Data.Repositories.Abstractions;
using InfrastructureService.Services.Scoring;
using InfrastructureService.Services.Scoring.Abstractions;
using InfrastructureService.Services.Yandex;
using InfrastructureService.Services.Yandex.Abstractions;
using MediatR;
using Microsoft.EntityFrameworkCore;

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

builder.Services.AddOptions<YandexOptions>()
    .BindConfiguration(YandexOptions.SectionName)
    .Validate(x => !string.IsNullOrWhiteSpace(x.GeocoderBaseUrl), "Yandex geocoder base url is required.")
    .Validate(x => !string.IsNullOrWhiteSpace(x.OrganizationSearchBaseUrl), "Yandex organization search base url is required.")
    .Validate(x => !string.IsNullOrWhiteSpace(x.ApiKey), "Yandex api key is required.")
    .ValidateOnStart();

builder.Services.AddHttpClient<IYandexGeocoderClient, YandexGeocoderClient>((serviceProvider, client) =>
{
    var options = serviceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<YandexOptions>>().Value;
    client.BaseAddress = new Uri(options.GeocoderBaseUrl.TrimEnd('/') + "/");
});

builder.Services.AddHttpClient<IYandexOrganizationSearchClient, YandexOrganizationSearchClient>((serviceProvider, client) =>
{
    var options = serviceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<YandexOptions>>().Value;
    client.BaseAddress = new Uri(options.OrganizationSearchBaseUrl.TrimEnd('/') + "/");
});

builder.Services.AddScoped<ILocationRepository, LocationRepository>();
builder.Services.AddScoped<IPoiRepository, PoiRepository>();
builder.Services.AddScoped<IInfrastructureRepository, InfrastructureRepository>();
builder.Services.AddScoped<IInfrastructureScoringService, InfrastructureScoringService>();

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

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await dbContext.Database.EnsureCreatedAsync();
}

app.MapOpenApi();
app.MapEndpoints(Assembly.GetExecutingAssembly());
app.MapGet("/", () => "Infrastructure Service is working.");

app.Run();
