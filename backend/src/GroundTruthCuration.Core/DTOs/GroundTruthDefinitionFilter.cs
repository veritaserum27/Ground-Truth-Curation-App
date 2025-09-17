using GroundTruthCuration.Core.Entities;

namespace GroundTruthCuration.Core.DTOs
{
    /// <summary>
    /// Filter criteria for querying ground truth definitions.
    /// </summary>
    public class GroundTruthDefinitionFilter
    {
        /// <summary>
        /// Gets or sets the validation status to filter ground truth definitions.
        /// </summary>
        public string? ValidationStatus { get; set; }
        /// <summary>
        /// Gets or sets the user query to filter ground truth definitions. Supports partial matches.
        /// </summary>
        public string? UserQuery { get; set; }
    }
}