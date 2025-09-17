namespace GroundTruthCuration.Core.DTOs;

public class GroundTruthContextDto
{
    // <summary>
    /// Gets or sets the unique identifier for the context.
    /// </summary>
    public Guid ContextId { get; set; }

    /// <summary>
    /// Gets or sets the name of the context.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the description of the context.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the collection of parameters associated with this context.
    /// </summary>
    public ICollection<ContextParameterDto> ContextParameters { get; set; } = new List<ContextParameterDto>();
}