# GroundTruthCuration.Jobs

Background job processing subsystem for the Ground Truth Curation platform. It provides an extensible framework for submitting, executing, tracking, and retrieving results of asynchronous operations (e.g., data exports, long-running queries, response generation).

> Current implementation is intentionally lightweight (in-memory queue + repository) to accelerate iteration. It is designed so you can later swap persistence and queuing layers (e.g., SQL / Redis / Azure Queue + durable storage) with minimal disruption.

## Key Capabilities

- Submit background jobs by functional type.
- Queue decouples submission from processing.
- Hosted background service executes jobs sequentially.
- Progress + status transitions persisted via repository abstraction.
- Atomic status transitions to avoid race conditions.
- Job cancellation is supported.

## Dependency Injection

Add all default services in an ASP.NET Core host via:

```csharp
builder.Services.AddDefaultGroundTruthCurationJobsServices();
```

This registers:
- `IBackgroundJobRepository` → `InMemoryBackgroundJobRepository`
- `IBackgroundJobQueue` → `ChannelBackgroundJobQueue`
- `IBackgroundJobExecutor` → `BackgroundJobExecutor`
- `IBackgroundJobService` → `BackgroundJobService`
- `BackgroundJobProcessor` (hosted service)

## Lifecycle & State Transitions

```
Submitted -> (Queued) -> [Processor dequeues]
Queued -> Running
Queued -> Canceled (if canceled before Running)
Running -> (Succeeded | Failed | Succeeded) (cancellation of running jobs not yet implemented)
```

## Execution Flow (Happy Path)

1. Client calls `IBackgroundJobService.SubmitJobAsync(type)`.
2. Service creates job, stores it, enqueues it.
3. `BackgroundJobProcessor` dequeues job.
4. Status transitions Queued → Running (atomic check).
5. `IBackgroundJobExecutor.ExecuteAsync` simulates / performs work, issuing progress callbacks.
6. Progress updates mutate job + repository best-effort (without changing status).
7. On success, job transitions Running → Succeeded; result persisted.
8. Client retrieves job metadata or result later.

## Adding a New Job Type

1. Add enum member to `BackgroundJobType` (choose next value, avoid collisions).
2. Extend `BackgroundJobExecutor.ExecuteAsync` switch (or create a specialized executor strategy and refactor if complexity grows).
3. (Optional) Add domain services / repository dependencies required for the new job logic.
4. Register any additional services in DI if needed.
5. Add API/controller endpoint (in API project) that calls `IBackgroundJobService.SubmitJobAsync(newType)`.
6. Update documentation / UI to surface the new job type.

If job execution becomes complex, refactor executor to delegate to per-type handler classes (Strategy pattern) keyed by `BackgroundJobType`.

## Sample Usage

Submit a job (e.g., from an API controller):

```csharp
[HttpPost("/jobs/export")]
public async Task<IActionResult> StartExport([FromServices] IBackgroundJobService jobs)
{
    var job = await jobs.SubmitJobAsync(BackgroundJobType.Export);
    return Accepted(new { jobId = job.Id, status = job.Status });
}
```

Retrieve job status:

```csharp
[HttpGet("/jobs/{id}")]
public async Task<IActionResult> GetJob(Guid id, [FromServices] IBackgroundJobService jobs)
{
    var job = await jobs.GetJobAsync(id);
    return job == null ? NotFound() : Ok(job);
}
```

Retrieve result (only if succeeded):

```csharp
[HttpGet("/jobs/{id}/result")]
public async Task<IActionResult> GetResult(Guid id, [FromServices] IBackgroundJobService jobs)
{
    var result = await jobs.GetJobResultAsync(id);
    return result == null ? NotFound() : Ok(result);
}
```

Cancel:

```csharp
[HttpPost("/jobs/{id}/cancel")]
public async Task<IActionResult> Cancel(Guid id, [FromServices] IBackgroundJobService jobs)
{
    var ok = await jobs.CancelJobAsync(id);
    return ok ? Ok(new { canceled = true }) : Conflict(new { message = "Unable to cancel" });
}
```
