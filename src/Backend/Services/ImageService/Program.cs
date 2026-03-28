using System.Reflection;
using BasicBuildingBlocks.Endpoints.Extensions;
using BasicBuildingBlocks.Handlers;
using ImageService.Clients;
using ImageService.Configuration;
using ImageService.Providers;
using ImageService.Services;
using ImageService.Services.Abstractions;
using MediatR;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
builder.Services.AddOpenApi();
builder.Services.AddExceptionHandler<ExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services
    .AddOptions<ImageKitOptions>()
    .BindConfiguration(ImageKitOptions.SectionName)
    .Validate(options => !string.IsNullOrWhiteSpace(options.PrivateKey), "ImageKit private API key is required.")
    .Validate(options => Uri.TryCreate(options.UploadEndpoint, UriKind.Absolute, out _), "ImageKit upload endpoint must be a valid absolute URI.")
    .Validate(options => options.MaxFileSizeInBytes > 0, "Max file size must be greater than zero.")
    .Validate(options => options.MaxFilesPerRequest > 0, "Max files per request must be greater than zero.")
    .Validate(options => options.TimeoutSeconds > 0, "TimeoutSeconds must be greater than zero.")
    .ValidateOnStart();

builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 110 * 1024 * 1024;
});

builder.Services.AddHttpClient<ImageKitApiClient>((serviceProvider, client) =>
{
    var options = serviceProvider.GetRequiredService<IOptions<ImageKitOptions>>().Value;
    client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
});
builder.Services.AddScoped<IImageStorageProvider, ImageKitStorageProvider>();
builder.Services.AddScoped<IImageUploadService, ImageUploadService>();

var app = builder.Build();

app.UseExceptionHandler();

app.MapOpenApi();
app.MapEndpoints(Assembly.GetExecutingAssembly());
app.MapGet("/", () => "Image service is work!");

app.Run();
