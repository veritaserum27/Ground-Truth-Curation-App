using GroundTruthCuration.Jobs.Processing;
using GroundTruthCuration.Jobs.Queues;
using GroundTruthCuration.Jobs.Repositories;
using GroundTruthCuration.Jobs.Services;
using Microsoft.Extensions.DependencyInjection;

namespace GroundTruthCuration.Jobs;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Add the default set of services required to enable async jobs processing for Ground Truth Curation.
    /// Queues and jobs are all processed in memory and do not require any additional architecture components.
    /// </summary
    public static IServiceCollection AddDefaultGroundTruthCurationJobsServices(this IServiceCollection services)
    {
        services.AddSingleton<IBackgroundJobRepository, InMemoryBackgroundJobRepository>();
        services.AddSingleton<IBackgroundJobQueue, ChannelBackgroundJobQueue>();
        // Register per-type executors
        services.AddSingleton<IBackgroundJobExecutor, GroundTruthCuration.Jobs.Processing.Executors.ExportJobExecutor>();
        services.AddSingleton<IBackgroundJobExecutor, GroundTruthCuration.Jobs.Processing.Executors.DataQueryExecutionJobExecutor>();
        services.AddSingleton<IBackgroundJobExecutor, GroundTruthCuration.Jobs.Processing.Executors.ResponseGenerationJobExecutor>();
        // Validator aggregates registered executors
        services.AddSingleton<IBackgroundJobTypeValidator, BackgroundJobTypeValidator>();
        services.AddScoped<IBackgroundJobService, BackgroundJobService>();
        services.AddHostedService<BackgroundJobProcessor>();
        return services;
    }
}

