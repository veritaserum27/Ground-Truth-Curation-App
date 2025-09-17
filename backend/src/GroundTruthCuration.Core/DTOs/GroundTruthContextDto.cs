namespace GroundTruthCuration.Core.DTOs;

public class GroundTruthContextDto
{
    /// <summary>
    /// Gets or sets the unique identifier for the context.
    /// </summary>
    public Guid ContextId { get; set; }

    public Guid GroundTruthId { get; set; } // FK to GroundTruthDefinition
    public Guid GroundTruthEntryId { get; set; } // FK to GroundTruthEntry
    public string ContextType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the collection of parameters associated with this context.
    /// </summary>
    public ICollection<ContextParameterDto> ContextParameters { get; set; } = new List<ContextParameterDto>();
}