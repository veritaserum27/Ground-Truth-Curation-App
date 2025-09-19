namespace GroundTruthCuration.Jobs.Services;

public interface IBackgroundJobTypeValidator
{
    bool IsSupported(string type);
    IReadOnlyCollection<string> SupportedTypes { get; }
    void EnsureSupported(string type);
}
