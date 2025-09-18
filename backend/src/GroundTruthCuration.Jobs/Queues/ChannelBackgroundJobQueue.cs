using System.Threading.Channels;
using GroundTruthCuration.Jobs.Entities;

namespace GroundTruthCuration.Jobs.Queues;

/// <summary>
/// Channel-based implementation of <see cref="IBackgroundJobQueue"/> using an unbounded channel.
/// </summary>
public class ChannelBackgroundJobQueue : IBackgroundJobQueue
{
    private readonly Channel<BackgroundJob> _channel;

    /// <summary>
    /// Creates a new queue optionally bounded by <paramref name="capacity"/>.
    /// </summary>
    public ChannelBackgroundJobQueue(int? capacity = null)
    {
        _channel = capacity.HasValue
            ? Channel.CreateBounded<BackgroundJob>(new BoundedChannelOptions(capacity.Value)
            {
                FullMode = BoundedChannelFullMode.Wait
            })
            : Channel.CreateUnbounded<BackgroundJob>(new UnboundedChannelOptions
            {
                SingleReader = true,
                SingleWriter = false
            });
    }

    /// <inheritdoc />
    public async Task EnqueueAsync(BackgroundJob job, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(job);
        await _channel.Writer.WriteAsync(job, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<BackgroundJob> DequeueAsync(CancellationToken cancellationToken)
    {
        var job = await _channel.Reader.ReadAsync(cancellationToken).ConfigureAwait(false);
        return job;
    }
}
