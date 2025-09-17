using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace GroundTruthCuration.Core.Entities;

/// <summary>
/// Represents a single entry of ground truth data, including response, required values, raw data, and timestamps.
/// </summary>
public class GroundTruthEntry
{
    /// <summary>
    /// Gets or sets the unique identifier for the ground truth entry.
    /// </summary>
    public Guid GroundTruthEntryId { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the associated ground truth definition.
    /// </summary>
    public required Guid GroundTruthId { get; set; }

    /// <summary>
    /// Gets or sets the response value for this ground truth entry.
    /// </summary>
    public string Response { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the required values in JSON format for this entry.
    /// </summary>
    public string RequiredValuesJson { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the required values as a collection of strings for this entry.
    /// This property is not mapped to the database and is derived from <see cref="RequiredValuesJson"/>.
    /// </summary>
    [NotMapped]
    public ICollection<string> RequiredValues
    {
        get => string.IsNullOrWhiteSpace(RequiredValuesJson)
            ? new List<string>()
            : JsonSerializer.Deserialize<List<string>>(RequiredValuesJson) ?? new List<string>();
        set => RequiredValuesJson = JsonSerializer.Serialize(value ?? new List<string>());
    }

    /// <summary>
    /// Gets or sets the raw data in JSON format for this entry.
    /// </summary>
    public string RawDataJson { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the raw data as a dictionary for this entry.
    /// This property is not mapped to the database and is derived from <see cref="RawDataJson"/>.
    /// </summary>
    [NotMapped]
    public ICollection<Dictionary<string, object>> RawData
    {
        get => string.IsNullOrWhiteSpace(RawDataJson)
            ? new List<Dictionary<string, object>>()
            : JsonSerializer.Deserialize<List<Dictionary<string, object>>>(RawDataJson) ?? new List<Dictionary<string, object>>();
        set => RawDataJson = JsonSerializer.Serialize(value ?? new List<Dictionary<string, object>>());
    }

    /// <summary>
    /// Gets or sets the date and time when this entry was created (in UTC, ISO 8601 format when serialized).
    /// </summary>
    public DateTime CreationDateTime { get; set; }

    /// <summary>
    /// Gets or sets the start date and time for this entry (in UTC, ISO 8601 format when serialized).
    /// </summary>
    public DateTime StartDateTime { get; set; }

    /// <summary>
    /// Gets or sets the end date and time for this entry (in UTC, ISO 8601 format when serialized). May be null if not ended.
    /// </summary>
    public DateTime? EndDateTime { get; set; } = null;

    /// <summary>
    /// Initializes a new instance of the <see cref="GroundTruthEntry"/> class, setting <see cref="CreationDateTime"/> and <see cref="StartDateTime"/> to the same UTC value.
    /// </summary>
    public GroundTruthEntry()
    {
        var now = DateTime.UtcNow;
        CreationDateTime = now;
        StartDateTime = now;
    }
}
