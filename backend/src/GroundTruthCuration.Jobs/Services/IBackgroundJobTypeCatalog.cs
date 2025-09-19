namespace GroundTruthCuration.Jobs.Services;

public interface IBackgroundJobTypeCatalog
{
    bool IsSupported(string type);
    IReadOnlyCollection<string> SupportedTypes { get; }
    void EnsureSupported(string type);
}
