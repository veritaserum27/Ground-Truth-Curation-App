namespace GroundTruthCuration.Core.DTOs;

public class ContextParameterDto
{
    /// <summary>
    /// Gets or sets the unique identifier for the parameter.
    /// </summary>
    public required Guid ParameterId { get; set; }

    /// <summary>
    /// Gets or sets the name of the parameter.
    /// </summary>
    public required string ParameterName { get; set; }

    /// <summary>
    /// Gets or sets the value of the parameter.
    /// </summary>
    public required string ParameterValue { get; set; }

    /// <summary>
    /// Gets or sets the data type of the parameter (e.g., string, int, bool).
    /// </summary>
    public required string DataType { get; set; }
}
