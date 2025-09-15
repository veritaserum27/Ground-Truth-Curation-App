using GroundTruthCuration.Core;
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
builder.Services.AddSingleton<IGroundTruthDefinitionRepository, InMemoryGroundTruthDefinitionRepository>();
builder.Services.AddSingleton<IGroundTruthEntryRepository, InMemoryGroundTruthEntryRepository>();

// Core layer (domain services) - depends on abstractions (interfaces)
builder.Services.AddScoped<IGroundTruthCurationService, GroundTruthCurationService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// HTTPS redirection: enable only if explicitly requested via config (CertificateSettings:GenerateAspNetCertificate=true)
var enableHttpsRedirection = builder.Configuration.GetValue<bool>("CertificateSettings:GenerateAspNetCertificate");
if (enableHttpsRedirection)
{
     app.UseHttpsRedirection();
}

app.MapControllers();

// Minimal endpoints for quick container / readiness checks
app.MapGet("/", () => Results.Ok(new { message = "Ground Truth Curation API running", time = DateTime.UtcNow }))
    .WithName("Root")
    .WithDescription("Root liveness endpoint.");

app.MapGet("/healthz", () => Results.Ok("healthy"))
    .WithName("Healthz")
    .WithDescription("Basic health probe.");

// Optional namespaced health for API grouping
app.MapGet("/api/healthz", () => Results.Ok(new { status = "ok" }))
    .WithName("ApiHealthz")
    .WithDescription("API health endpoint.");

app.Run();
