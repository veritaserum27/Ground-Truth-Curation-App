using GroundTruthCuration.Core;
using GroundTruthCuration.Core.DTOs;
using GroundTruthCuration.Core.Entities;
using GroundTruthCuration.Core.Interfaces;
using GroundTruthCuration.Core.Services;
using GroundTruthCuration.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register application services
builder.Services.AddScoped<IHello, Hello>();

// Register Clean Architecture dependencies
// Infrastructure layer (repositories) - implements interfaces from Core
builder.Services.AddSingleton<IGroundTruthRepository, GroundTruthRepository>();
builder.Services.AddSingleton<IManufacturingDataDocDbRepository, ManufacturingDataDocDbRepository>();
builder.Services.AddSingleton<IManufacturingDataRelDbRepository, ManufacturingDataRelDbRepository>();

// Core layer (domain services) - depends on abstractions (interfaces)
builder.Services.AddScoped<IStatusService, StatusService>();
builder.Services.AddScoped<IGroundTruthCurationService, GroundTruthCurationService>();
builder.Services.AddScoped<IGroundTruthMapper<GroundTruthDefinition, GroundTruthDefinitionDto>, GroundTruthDefinitionToDtoMapper>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.MapGet("/", () => Results.Redirect("/swagger"));
}

// HTTPS redirection: enable only if explicitly requested via config (CertificateSettings:GenerateAspNetCertificate=true)
var enableHttpsRedirection = builder.Configuration.GetValue<bool>("CertificateSettings:GenerateAspNetCertificate");
if (enableHttpsRedirection)
{
    app.UseHttpsRedirection();
}

app.MapGet("/healthz", () => Results.Ok("healthy"))
    .WithName("Healthz")
    .WithDescription("Basic health probe.");

// Optional namespaced health for API grouping
app.MapGet("/api/healthz", () => Results.Ok(new { status = "ok" }))
    .WithName("ApiHealthz")
    .WithDescription("API health endpoint.");

app.MapControllers();

// Log user-friendly URLs using proper logging
app.Lifetime.ApplicationStarted.Register(() =>
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    var port = Environment.GetEnvironmentVariable("PORT") ?? "5105";

    // Add a small delay to ensure this appears after built-in messages
    Task.Run(async () =>
    {
        await Task.Delay(100);
        logger.LogInformation("");
        logger.LogInformation("🚀 Ground Truth Curation API is running!");
        logger.LogInformation("📖 Swagger UI: http://localhost:{Port}/swagger/index.html", port);
        logger.LogInformation("🏥 Health Check: http://localhost:{Port}/healthz", port);
        logger.LogInformation("");
    });
});

app.Run();
