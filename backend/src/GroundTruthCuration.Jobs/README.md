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
- Multiple `IBackgroundJobExecutor` implementations (one per job type)
- `IBackgroundJobTypeValidator` → `BackgroundJobTypeValidator`
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

Background job types are now string identifiers (e.g., `"Export"`, `"DataQueryExecution"`, `"ResponseGeneration"`). Each type is handled by its own executor class implementing `IBackgroundJobExecutor` and advertising a `SupportedType` string.

1. Choose a unique job type identifier string (PascalCase recommended).
2. Create a new executor implementing `IBackgroundJobExecutor` with the `SupportedType` property returning that string.
3. Implement `ExecuteAsync` with progress callbacks as needed.
4. Register the executor by adding another `AddSingleton<IBackgroundJobExecutor, YourExecutor>()` (or rely on assembly scanning if introduced later).
5. (Optional) Add supporting services / repositories for domain-specific logic.
6. Expose an API endpoint that calls `IBackgroundJobService.SubmitJobAsync("YourType")`.
7. Update UI / docs to surface the new type.

Unsupported types are rejected at submission time by `IBackgroundJobTypeValidator` and also guarded at execution time (defensive check).

## Sample Usage

Submit a job (e.g., from an API controller):

```csharp
[HttpPost("/jobs/export")]
public async Task<IActionResult> StartExport([FromServices] IBackgroundJobService jobs)
{
    var job = await jobs.SubmitJobAsync("Export");
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
