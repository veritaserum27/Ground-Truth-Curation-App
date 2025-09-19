using System.Collections.ObjectModel;

namespace GroundTruthCuration.Jobs.Services;

public class BackgroundJobTypeValidator : IBackgroundJobTypeValidator
{
    private readonly HashSet<string> _types;
    public BackgroundJobTypeValidator(IEnumerable<IBackgroundJobExecutor> executors)
    {
        _types = executors.Select(e => e.SupportedType).ToHashSet(StringComparer.Ordinal);
        SupportedTypes = new ReadOnlyCollection<string>(_types.ToList());
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
