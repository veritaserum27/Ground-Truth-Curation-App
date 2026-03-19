using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.Tracing;
using GroundTruthCuration.Core.Constants;

namespace GroundTruthCuration.Core.Entities;

/// <summary>
/// Represents the definition of ground truth data, including metadata, validation status, and associated entries.
/// </summary>
public class GroundTruthDefinition
{
    /// <summary>
    /// Gets or sets the unique identifier for the ground truth definition.
    /// </summary>
    public Guid GroundTruthId { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the user query associated with this ground truth definition.
    /// </summary>
    public required string UserQuery { get; set; }

    /// <summary>
    /// Gets or sets the validation status of the ground truth definition as a string.
    /// </summary>
    public string ValidationStatus { get; set; } = GroundTruthValidationStatus.New.ToDisplayString();

    /// <summary>
    /// Gets or sets the validation status of the ground truth definition.
    /// </summary>
    [NotMapped]
    public GroundTruthValidationStatus ValidationStatusEnum => GroundTruthValidationStatusExtensions.FromDisplayString(ValidationStatus);

    /// <summary>
    /// Gets or sets the category of the ground truth definition.
    /// </summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the username of the user who created this ground truth definition.
    /// </summary>
    public string UserCreated { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the username of the user who last updated this ground truth definition.
    /// </summary>
    public string UserUpdated { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the date and time when this ground truth definition was created.
    /// </summary>
    public DateTime CreationDateTime { get; set; }

    /// <summary>
    /// Gets or sets the start date and time for the ground truth data.
    /// </summary>
    public DateTime StartDateTime { get; set; }

    /// <summary>
    /// Gets or sets the end date and time for the ground truth data.
    /// </summary>
    public DateTime? EndDateTime { get; set; } = null;

    /// <summary>
    /// Gets or sets the collection of ground truth entries associated with this definition.
    /// </summary>
    public ICollection<GroundTruthEntry> GroundTruthEntries { get; set; } = new List<GroundTruthEntry>();

    /// <summary>
    /// Gets or sets the collection of data query definitions associated with this ground truth definition.
    /// </summary>
    public ICollection<DataQueryDefinition> DataQueryDefinitions { get; set; } = new List<DataQueryDefinition>();

    /// <summary>
    /// Gets or sets the collection of comments associated with this ground truth definition.
    /// </summary>
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();

    /// <summary>
    /// Gets or sets the collection of tags associated with this ground truth definition.
    /// </summary>
    public ICollection<Tag> Tags { get; set; } = new List<Tag>();

    /// <summary>
    /// Initializes a new instance of the <see cref="GroundTruthDefinition"/> class
    /// with default values for creation and start date times.
    /// </summary>
    public GroundTruthDefinition()
    {
        var now = DateTime.UtcNow;
        CreationDateTime = now;
        StartDateTime = now;
    }
}
