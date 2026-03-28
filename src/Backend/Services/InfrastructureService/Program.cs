using System.Reflection;
using BasicBuildingBlocks.Behaviors;
using BasicBuildingBlocks.Endpoints.Extensions;
using BasicBuildingBlocks.Handlers;
using FluentValidation;
using InfrastructureService.Data;
using InfrastructureService.Data.Repositories;
using InfrastructureService.Data.Repositories.Abstractions;
using InfrastructureService.Services.Places;
using InfrastructureService.Services.Places.Abstractions;
using InfrastructureService.Services.Places.Geoapify;
using InfrastructureService.Services.Places.Geoapify.Abstractions;
using InfrastructureService.Services.Places.Overpass;
using InfrastructureService.Services.Places.Overpass.Abstractions;
using InfrastructureService.Services.Classification;
using InfrastructureService.Services.Classification.Abstractions;
using InfrastructureService.Services.Routing;
using InfrastructureService.Services.Routing.Abstractions;
using InfrastructureService.Services.Scoring;
using InfrastructureService.Services.Scoring.Abstractions;
using InfrastructureService.Services.Scoring.Policies;
using InfrastructureService.Services.Sanitization;
using InfrastructureService.Services.Sanitization.Abstractions;
using InfrastructureService.Services.Summaries;
using InfrastructureService.Services.Summaries.Abstractions;
using InfrastructureService.Services.TransportGrouping;
using InfrastructureService.Services.TransportGrouping.Abstractions;
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
    .Validate(x => !string.IsNullOrWhiteSpace(x.ApiKey), "Yandex api key is required.")
    .ValidateOnStart();

builder.Services.AddOptions<PlacesProviderOptions>()
    .BindConfiguration(PlacesProviderOptions.SectionName)
    .Validate(x => !string.IsNullOrWhiteSpace(x.Provider), "PlacesProvider provider is required.")
    .ValidateOnStart();

builder.Services.AddOptions<OverpassOptions>()
    .BindConfiguration(OverpassOptions.SectionName)
    .Validate(x => !string.IsNullOrWhiteSpace(x.BaseUrl), "Overpass base url is required.")
    .Validate(x => x.FallbackBaseUrls.All(static value => !string.IsNullOrWhiteSpace(value)), "Overpass fallback base urls must not contain empty values.")
    .Validate(x => x.TimeoutSeconds > 0, "Overpass timeout must be positive.")
    .Validate(x => x.DefaultRadiusMeters > 0, "Overpass default radius must be positive.")
    .Validate(x => x.MaxRadiusMeters >= x.DefaultRadiusMeters, "Overpass max radius must be greater than or equal to default radius.")
    .Validate(x => x.RetryCount >= 0, "Overpass retry count must be non-negative.")
    .ValidateOnStart();

builder.Services.AddOptions<GeoapifyOptions>()
    .BindConfiguration(GeoapifyOptions.SectionName)
    .Validate(x => true, "Geoapify options are optional when provider is not active.")
    .ValidateOnStart();

builder.Services.AddOptions<InfrastructureScoringOptions>()
    .BindConfiguration(InfrastructureScoringOptions.SectionName)
    .Validate(x => x.DefaultRadiusMeters > 0, "DefaultRadiusMeters must be positive.")
    .Validate(x => x.MaxRadiusMeters >= x.DefaultRadiusMeters, "MaxRadiusMeters must be greater than or equal to DefaultRadiusMeters.")
    .Validate(x => x.MaxPoiCount > 0, "MaxPoiCount must be positive.")
    .ValidateOnStart();

builder.Services.AddOptions<RouteMetricsOptions>()
    .BindConfiguration(RouteMetricsOptions.SectionName)
    .Validate(x => x.WalkSpeedKmPerHour > 0, "WalkSpeedKmPerHour must be positive.")
    .Validate(x => x.DriveSpeedKmPerHour > 0, "DriveSpeedKmPerHour must be positive.")
    .ValidateOnStart();

builder.Services.AddHttpClient<IYandexGeocoderClient, YandexGeocoderClient>((serviceProvider, client) =>
{
    var options = serviceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<YandexOptions>>().Value;
    client.BaseAddress = new Uri(options.GeocoderBaseUrl.TrimEnd('/') + "/");
});

builder.Services.AddHttpClient<IOverpassApiClient, OverpassApiClient>((serviceProvider, client) =>
{
    var options = serviceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<OverpassOptions>>().Value;
    client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
    client.DefaultRequestHeaders.UserAgent.ParseAdd(options.UserAgent);
});

builder.Services.AddHttpClient<IGeoapifyPlacesClient, GeoapifyPlacesClient>((serviceProvider, client) =>
{
    var options = serviceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<GeoapifyOptions>>().Value;
    client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/");
    client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
});

builder.Services.AddScoped<ILocationRepository, LocationRepository>();
builder.Services.AddScoped<IPoiRepository, PoiRepository>();
builder.Services.AddScoped<IInfrastructureRepository, InfrastructureRepository>();
builder.Services.AddMemoryCache();
builder.Services.AddScoped<IPlaceCategoryNormalizer, PlaceCategoryNormalizer>();
builder.Services.AddSingleton<PoiClassificationDictionaryProvider>();
builder.Services.AddScoped<IPoiClassificationService, PoiClassificationService>();
builder.Services.AddScoped<IOverpassPoiMapper, OverpassPoiMapper>();
builder.Services.AddScoped<OverpassPlacesProvider>();
builder.Services.AddScoped<GeoapifyPlacesProvider>();
builder.Services.AddScoped<IPlacesProvider>(serviceProvider =>
{
    var options = serviceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<PlacesProviderOptions>>().Value;
    return options.Provider.Trim() switch
    {
        "Geoapify" => serviceProvider.GetRequiredService<GeoapifyPlacesProvider>(),
        "Overpass" => serviceProvider.GetRequiredService<OverpassPlacesProvider>(),
        _ => throw new InvalidOperationException($"Unsupported places provider '{options.Provider}'.")
    };
});
builder.Services.AddScoped<IInfrastructureScoringService, InfrastructureScoringService>();
builder.Services.AddScoped<IInfrastructureScoringInputBuilder, InfrastructureScoringInputBuilder>();
builder.Services.AddScoped<IMetroApplicabilityService, MetroApplicabilityService>();
builder.Services.AddScoped<IInfrastructureCategoryScoringPolicy, TransportCategoryScoringPolicy>();
builder.Services.AddScoped<IInfrastructureCategoryScoringPolicy, HealthcareCategoryScoringPolicy>();
builder.Services.AddScoped<IInfrastructureCategoryScoringPolicy, CommerceCategoryScoringPolicy>();
builder.Services.AddScoped<IInfrastructureCategoryScoringPolicy, EducationCategoryScoringPolicy>();
builder.Services.AddScoped<IInfrastructureCategoryScoringPolicy, LeisureCategoryScoringPolicy>();
builder.Services.AddScoped<IPoiSanitizationService, PoiSanitizationService>();
builder.Services.AddScoped<IInfrastructureSummaryBuilder, InfrastructureSummaryBuilder>();
builder.Services.AddScoped<IRouteMetricsService, RouteMetricsService>();
builder.Services.AddScoped<ITransportStopGroupingService, TransportStopGroupingService>();

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
