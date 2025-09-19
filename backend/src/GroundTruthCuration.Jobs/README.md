# GroundTruthCuration.Jobs

Background job processing subsystem that provides an extensible framework for submitting, executing, tracking, and retrieving results of asynchronous operations (e.g., data exports, long-running queries, response generation).

## 1. Quick Start

### Install & Register

Create IBackgroundJobExecutor implementations. Here is one example:

```csharp
using GroundTruthCuration.Jobs.Entities;

namespace YourNamespace;

public class YourJobExecutor : IBackgroundJobExecutor
{
    public string BackgroundJobType => "YourJobType";

    public async Task<string?> ExecuteAsync(BackgroundJob job, Action<int, string?>? progressCallback, CancellationToken cancellationToken)
    {
        var totalSteps = 10;
        for (int i = 1; i <= totalSteps; i++)
        {
            // exit early if cancelled
            cancellationToken.ThrowIfCancellationRequested();

            // do the work
            await Task.Delay(1_000, cancellationToken).ConfigureAwait(false);

            // report progress via progressCallback
            var pct = (int)Math.Round(i * 100.0 / totalSteps);
            progressCallback?.Invoke(pct, $"Running {i}/{totalSteps}...");
        }
        return JsonSerializer.Serialize(new { message = "Job complete", jobId = job.Id });
    }
}
```

In your ASP.NET Core host (`Program.cs`):

```csharp
var builder = WebApplication.CreateBuilder(args);
// ... other service registrations

// Add IBackgroundJobExecutor implementations
builder.Services.AddBackgroundJobExecutor<YourJobExecutor>();

// Add other background job services
builder.Services.AddDefaultGroundTruthCurationJobsServices();

var app = builder.Build();
// ... middleware, endpoints
app.Run();
```

### Submit a Job

```csharp
var job = await _jobs.SubmitJobAsync("YourJob");
// returns job metadata (Id, Type, Status = Queued, etc.)
```

### Poll Status

```csharp
var jobMeta = await _jobs.GetJobAsync(job.Id);
```

### Get Result (after Succeeded)

```csharp
var result = await _jobs.GetJobResultAsync(job.Id);
```

### Cancel

```csharp
var canceled = await _jobs.CancelJobAsync(job.Id);
```

## 2. Architecture & Components

High-level flow (single in-memory worker):

```
Client -> IBackgroundJobService -> Repository (persist job)
                               -> Queue (enqueue id)
BackgroundJobProcessor -> Queue (dequeue id)
                       -> Repository (atomic status update)
                       -> Executor (ExecuteAsync w/ progress callbacks)
                       -> Repository (progress + final status + result)
```

Core abstractions (all DI registered):

- `IBackgroundJobService` – Public façade for submit, get, result,
  cancel.
- `IBackgroundJobQueue` – FIFO queue decoupling submit and execution.
- `IBackgroundJobRepository` – Persistence of job metadata + result.
- `IBackgroundJobExecutor` – Per job-type execution strategy.
- `IBackgroundJobTypeCatalog` – Guards unsupported types early.
- `BackgroundJobProcessor` – Hosted service: pulls queued jobs and
  drives execution.

You can replace queue or repository by swapping implementations
registered for their interface (keep semantics: FIFO for queue, atomic
compare-and-set style status transitions for repo).

---

## 3. Configuration & Extension Points

### Default Registration

```csharp
builder.Services.AddDefaultGroundTruthCurationJobsServices();
```

Registers (current defaults):

- `IBackgroundJobRepository` → `InMemoryBackgroundJobRepository`
- `IBackgroundJobQueue` → `ChannelBackgroundJobQueue`
- `IBackgroundJobTypeCatalog` → `BackgroundJobTypeCatalog`
- `IBackgroundJobService` → `BackgroundJobService`
- All discovered `IBackgroundJobExecutor` implementations
- `BackgroundJobProcessor` (hosted service)

### Adding a New Job Type

1. Pick a unique string identifier.
2. Implement `IBackgroundJobExecutor` with `SupportedType` returning
   that string.
3. Implement `ExecuteAsync` performing work and invoking provided
   progress callback.
4. Register executor (singleton) if not picked up automatically.
5. Expose endpoint calling `SubmitJobAsync("YourType")`.
6. Optionally enrich result payload and document schema.

### Swapping the Repository

Implement `IBackgroundJobRepository` backed by your store (e.g., SQL):

- Ensure atomic status transitions (e.g., `UPDATE ... WHERE Id = @id AND
  Status = @expected`).
- Persist job result (structured JSON recommended) separately from
  metadata columns.
- Support querying by Id efficiently (primary key / index).

### Swapping the Queue

Implement `IBackgroundJobQueue` using Redis, Azure Queue, or a channel
with bounded capacity. Maintain ordering semantics where possible.

### Concurrency / Parallelism

Current processor executes sequentially. To introduce parallelism:

- Run multiple processor hosted services (if repository supports safe
  atomic transitions).
- Partition by job type (one queue per type) if isolation needed.

---

## 4. Job Status & Transitions

State machine (current implementation):

```
Queued -> Running -> (Succeeded | Failed | Canceled)
```

| Status    | Meaning / Notes                                        |
| --------- | ------------------------------------------------------ |
| Queued    | Persisted; awaiting processor dequeue                  |
| Running   | Executor started; progress events may update metadata  |
| Succeeded | Completed without error; result available              |
| Failed    | Terminal error; failure reason stored (if implemented) |
| Canceled  | Job canceled, no execution performed                   |

Progress Updates:

- Executors call provided progress callback (e.g., percentage or phase).
- Repository attempts best-effort mutation without changing status.

Failure Handling:

- Current design: no retries. Caller may resubmit a new job.
- Add retry logic by wrapping executor invocation with policy (e.g.,
  Polly) and storing attempt count.

Result Retrieval:

- Only allowed once status is `Succeeded` (returns null otherwise).
- Results should be considered immutable; if large, plan for external
  blob storage and store a reference.

---

## 5. Execution Flow (Happy Path)

1. Client calls `SubmitJobAsync(type)`.
2. Service creates job, persists, enqueues id.
3. Processor dequeues id, atomically sets `Running`.
4. Executor runs, issuing progress callbacks.
5. Executor finishes → repository updated with final status + result.
6. Client polls metadata / retrieves result.

