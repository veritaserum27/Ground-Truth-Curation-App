using GroundTruthCuration.Core.Entities;
using GroundTruthCuration.Core.Interfaces;
using GroundTruthCuration.Core.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GroundTruthCuration.Infrastructure.Processing;

/// <summary>
/// Hosted background service that dequeues and executes jobs.
/// </summary>
public class BackgroundJobProcessor : BackgroundService
{
    private readonly IBackgroundJobQueue _queue;
    private readonly IBackgroundJobRepository _repository;
    private readonly IBackgroundJobExecutor _executor;
    private readonly ILogger<BackgroundJobProcessor> _logger;

    /// <summary>
    /// Creates a new <see cref="BackgroundJobProcessor"/>.
    /// </summary>
    public BackgroundJobProcessor(IBackgroundJobQueue queue, IBackgroundJobRepository repository, IBackgroundJobExecutor executor, ILogger<BackgroundJobProcessor> logger)
    {
        _queue = queue;
        _repository = repository;
        _executor = executor;
        _logger = logger;
    }

    /// <summary>
    /// Main processing loop which continuously dequeues and executes jobs until cancellation.
    /// </summary>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("BackgroundJobProcessor started");
        while (!stoppingToken.IsCancellationRequested)
        {
            BackgroundJob? job = null;
            try
            {
                job = await _queue.DequeueAsync(stoppingToken).ConfigureAwait(false);
                if (job == null)
                {
                    continue;
                }

                var transitioned = await _repository.TryUpdateStatusAsync(job.Id, BackgroundJobStatus.Queued, BackgroundJobStatus.Running, j =>
                {
                    j.StartedAt = DateTime.UtcNow;
                    j.StatusMessage = "Running";
                }, stoppingToken).ConfigureAwait(false);

                if (!transitioned)
                {
                    // It may have been canceled or started already.
                    continue;
                }

                _logger.LogInformation("Executing job {JobId} of type {Type}", job.Id, job.Type);

                string? result = null;
                try
                {
                    result = await _executor.ExecuteAsync(job, (pct, msg) =>
                    {
                        job.Progress = pct;
                        if (!string.IsNullOrWhiteSpace(msg))
                        {
                            job.StatusMessage = msg;
                        }
                        job.UpdatedAt = DateTime.UtcNow;
                        // Save progress updates (best-effort, ignore result) - no status change.
                        _repository.UpdateAsync(job, CancellationToken.None).ConfigureAwait(false);
                    }, stoppingToken).ConfigureAwait(false);

                    job.ResultData = result;
                    job.CompletedAt = DateTime.UtcNow;
                    job.Progress = 100;
                    job.StatusMessage = "Completed";
                    var success = await _repository.TryUpdateStatusAsync(job.Id, BackgroundJobStatus.Running, BackgroundJobStatus.Succeeded, j =>
                    {
                        j.ResultData = result;
                        j.CompletedAt = job.CompletedAt;
                        j.Progress = job.Progress;
                        j.StatusMessage = job.StatusMessage;
                    }, stoppingToken).ConfigureAwait(false);
                    if (!success)
                    {
                        _logger.LogWarning("Failed to transition job {JobId} to Succeeded (race condition).", job.Id);
                    }
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    // Host shutdown.
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error executing job {JobId}", job.Id);
                    var failed = await _repository.TryUpdateStatusAsync(job.Id, BackgroundJobStatus.Running, BackgroundJobStatus.Failed, j =>
                    {
                        j.Error = ex.Message;
                        j.CompletedAt = DateTime.UtcNow;
                        j.StatusMessage = "Failed";
                    }, stoppingToken).ConfigureAwait(false);
                    if (!failed)
                    {
                        _logger.LogWarning("Failed to transition job {JobId} to Failed.", job.Id);
                    }
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled processor exception");
                // Backoff a little to avoid hot loop on persistent error.
                await Task.Delay(500, stoppingToken).ConfigureAwait(false);
            }
        }
        _logger.LogInformation("BackgroundJobProcessor stopping");
    }
}
