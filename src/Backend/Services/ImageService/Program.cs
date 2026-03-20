using System.Reflection;
using BasicBuildingBlocks.Endpoints.Extensions;
using BasicBuildingBlocks.Handlers;
using ImageService.Services;
using ImageService.Services.Abstractions;
using MediatR;
using Microsoft.AspNetCore.Http.Features;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
builder.Services.AddOpenApi();
builder.Services.AddExceptionHandler<ExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services
    .AddOptions<ImageBbOptions>()
    .BindConfiguration(ImageBbOptions.SectionName)
    .Validate(options => !string.IsNullOrWhiteSpace(options.ApiKey), "ImageBB API key is required.")
    .Validate(options => Uri.TryCreate(options.Endpoint, UriKind.Absolute, out _), "ImageBB endpoint must be a valid absolute URI.")
    .Validate(options => options.MaxFileSizeInBytes > 0, "Max file size must be greater than zero.")
    .Validate(options => options.MaxFilesPerRequest > 0, "Max files per request must be greater than zero.")
    .ValidateOnStart();

builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 110 * 1024 * 1024;
});

builder.Services.AddHttpClient<IImageBbService, ImageBbService>();

var app = builder.Build();

app.UseExceptionHandler();

app.MapOpenApi();
app.MapEndpoints(Assembly.GetExecutingAssembly());
app.MapGet("/", () => "Image service is work!");

app.Run();
