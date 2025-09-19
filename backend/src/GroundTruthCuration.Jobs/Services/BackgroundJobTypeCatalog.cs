using System.Collections.ObjectModel;
using GroundTruthCuration.Jobs.Processing;

namespace GroundTruthCuration.Jobs.Services;

public class BackgroundJobTypeCatalog : IBackgroundJobTypeCatalog
{
    private readonly HashSet<string> _types;
    public BackgroundJobTypeCatalog(IEnumerable<IBackgroundJobExecutor> executors)
    {
        ValidateExecutors(executors);
        _types = executors.Select(e => e.BackgroundJobType).ToHashSet(StringComparer.Ordinal);
        SupportedTypes = new ReadOnlyCollection<string>(_types.ToList());
    }

    private static void ValidateExecutors(IEnumerable<IBackgroundJobExecutor> executors)
    {
        var list = executors?.ToList() ?? new List<IBackgroundJobExecutor>();
        if (list.Count == 0)
        {
            throw new ArgumentException("At least one background job executor must be registered.", nameof(executors));
        }
    }

    public IReadOnlyCollection<string> SupportedTypes { get; }

    public bool IsSupported(string type) => !string.IsNullOrWhiteSpace(type) && _types.Contains(type);

    public void EnsureSupported(string type)
    {
        if (!IsSupported(type))
        {
            throw new ArgumentException($"Unsupported background job type '{type}'. Supported: {string.Join(", ", _types)}", nameof(type));
        }
    }
}
