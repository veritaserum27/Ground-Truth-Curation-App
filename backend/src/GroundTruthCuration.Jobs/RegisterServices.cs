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
        services.AddSingleton<IBackgroundJobTypeCatalog, BackgroundJobTypeCatalog>();
        services.AddScoped<IBackgroundJobService, BackgroundJobService>();
        services.AddHostedService<BackgroundJobProcessor>();
        return services;
    }

    /// <summary>
    /// Registers a background job executor as a singleton. Can be called multiple times
    /// to add additional executor implementations.
    /// </summary>
    public static IServiceCollection AddBackgroundJobExecutor<TExecutor>(this IServiceCollection services)
        where TExecutor : class, IBackgroundJobExecutor
    {
        services.AddSingleton<IBackgroundJobExecutor, TExecutor>();
        return services;
    }
}

