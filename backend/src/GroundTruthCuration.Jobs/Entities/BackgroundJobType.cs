using System.Text.Json.Serialization;
namespace GroundTruthCuration.Jobs.Entities;

/// <summary>
/// Enumerates the supported types of background jobs.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum BackgroundJobType
{
    /// <summary>
    /// A job that exports curated data.
    /// </summary>
    Export = 0,
    /// <summary>
    /// A job that executes a potentially long-running data query.
    /// </summary>
    DataQueryExecution = 1,
    /// <summary>
    /// A job that generates a response or synthesized content.
    /// </summary>
    ResponseGeneration = 2,
    /// <summary>
    /// Placeholder / future extension type used for scaffolding.
    /// </summary>
    Placeholder = 99
}
