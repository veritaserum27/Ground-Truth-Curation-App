using GroundTruthCuration.Core.Entities;

namespace GroundTruthCuration.Core.DTOs
{
    /// <summary>
    /// Filter criteria for querying ground truth definitions.
    /// </summary>
    public class GroundTruthDefinitionFilter
    {
        /// <summary>
        /// Gets or sets the user ID to filter ground truth definitions by creator.
        /// </summary>
        public string? UserId { get; set; }

        /// <summary>
        /// Gets or sets the validation status to filter ground truth definitions.
        /// </summary>
        public string? ValidationStatus { get; set; }

        /// <summary>
        /// Gets or sets the date range to filter ground truth definitions by creation date.
        /// </summary>
        public (DateTime? Start, DateTime? End)? CreatedDateRange { get; set; }
        public string? ContextId { get; set; }
    }
}