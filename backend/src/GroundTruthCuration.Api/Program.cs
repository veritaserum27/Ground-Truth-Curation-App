using GroundTruthCuration.Core;
using GroundTruthCuration.Core.DTOs;
using GroundTruthCuration.Core.Entities;
using GroundTruthCuration.Core.Interfaces;
using GroundTruthCuration.Core.Utilities;
using GroundTruthCuration.Core.Services;
using GroundTruthCuration.Infrastructure.Repositories;
using GroundTruthCuration.Core.Delegates;
using GroundTruthCuration.Infrastructure.BackgroundJobExecutors;
using GroundTruthCuration.Jobs;
using GroundTruthCuration.Jobs.Processing.Executors;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var AllowSpecificOrigins = "Frontend";
// CORS configuration (allow configured frontend dev origins)
builder.Services.AddCors(options =>
{
  options.AddPolicy(AllowSpecificOrigins, policy =>
    policy.WithOrigins("http://localhost:3000", "http://localhost:5173", "http://localhost:5105", "https://localhost:7278")
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials());
});

// Register application services
builder.Services.AddScoped<IHello, Hello>();

// Register Clean Architecture dependencies

// Infrastructure layer (repositories) - implements interfaces from Core
builder.Services.AddSingleton<IGroundTruthRepository, GroundTruthRepository>();
builder.Services.AddSingleton<ITagRepository, TagRepository>();
builder.Services.AddSingleton<ManufacturingDataDocDbRepository>();
builder.Services.AddSingleton<ManufacturingDataRelDbRepository>();
builder.Services.AddSingleton<DatastoreRepositoryResolver>(serviceProvider => key =>
{
  return key switch
  {
    "ManufacturingDataDocDb" => serviceProvider.GetRequiredService<ManufacturingDataDocDbRepository>(),
    "ManufacturingDataRelDb" => serviceProvider.GetRequiredService<ManufacturingDataRelDbRepository>(),
    _ => throw new KeyNotFoundException($"No repository found for key: {key}")
  };
});

// Core layer (domain services) - depends on abstractions (interfaces)
builder.Services.AddScoped<IStatusService, StatusService>();
builder.Services.AddScoped<IGroundTruthCurationService, GroundTruthCurationService>();
builder.Services.AddScoped<IGroundTruthMapper<GroundTruthDefinition, GroundTruthDefinitionDto>, GroundTruthDefinitionToDtoMapper>();
builder.Services.AddScoped<ITagService, TagService>();
builder.Services.AddScoped<IGroundTruthComparer<ContextParameterDto, ContextParameter>, ContextParameterComparer>();
builder.Services.AddScoped<IGroundTruthComparer<GroundTruthContextDto, GroundTruthContext>, GroundTruthContextComparer>();

// Background job processing registrations,
// be sure to register IBackgroundJobExecutors before the rest of the job services
builder.Services.AddBackgroundJobExecutor<DataQueryExecution>();
builder.Services.AddBackgroundJobExecutor<ResponseGeneration>();
builder.Services.AddDefaultGroundTruthCurationJobsServices();

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
app.UseCors(AllowSpecificOrigins);

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
