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

// Use HTTPS redirection only when not explicitly disabled
var disableHttpsRedirection = builder.Configuration.GetValue<bool>("CertificateSettings:GenerateAspNetCertificate") == false;
if (!disableHttpsRedirection)
{
    app.UseHttpsRedirection();
}

app.MapControllers();

app.Run();
