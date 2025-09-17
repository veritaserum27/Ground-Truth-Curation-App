using System.Diagnostics.Tracing;

namespace GroundTruthCuration.Core.Constants
{
    /// <summary>
    /// Represents the validation status of a ground truth entry.
    /// </summary>
    public enum GroundTruthValidationStatus
    {
        /// <summary>
        /// Indicates that the entry is newly created and has not been processed.
        /// </summary>
        New,

        /// <summary>
        /// Indicates that the entry is new and has had data curated.
        /// </summary>
        NewDataCurated,
        /// <summary>
        /// Indicates that the entry has been validated and approved.
        /// </summary>
        Validated,
        /// <summary>
        /// Indicates that the entry is out of scope.
        /// </summary>
        OutOfScope,
        /// <summary>
        /// Indicates that the entry has been revised.
        /// </summary>
        Revised,
        /// <summary>
        /// Indicates that revisions have been requested for the entry.
        /// </summary>
        RevisionsRequested,
        /// <summary>
        /// Indicates that the entry is pending further information before it can be revised further.
        /// </summary>
        Pending
    }

    /// <summary>
    /// Provides extension methods for the GroundTruthValidationStatus enum.
    /// </summary>
    public static class GroundTruthValidationStatusExtensions
    {
        /// <summary>
        /// Converts the validation status to a display-friendly string.
        /// </summary>
        /// <param name="status">The validation status.</param>
        /// <returns>A string representation of the validation status.</returns>
        public static string ToDisplayString(this GroundTruthValidationStatus status)
        {
            switch (status)
            {
                case GroundTruthValidationStatus.New:
                    return "New";
                case GroundTruthValidationStatus.NewDataCurated:
                    return "New, Data Curated";
                case GroundTruthValidationStatus.Validated:
                    return "Validated";
                case GroundTruthValidationStatus.OutOfScope:
                    return "Out of Scope";
                case GroundTruthValidationStatus.Revised:
                    return "Revised";
                case GroundTruthValidationStatus.RevisionsRequested:
                    return "Revisions Requested";
                case GroundTruthValidationStatus.Pending:
                    return "Pending";
                default:
                    return string.Empty;
            }
        }

        /// <summary>
        /// Converts a display-friendly string to the corresponding validation status.
        /// </summary>
        /// <param name="displayString">The display-friendly string representation of the validation status.</param>
        /// <returns>The corresponding <see cref="GroundTruthValidationStatus"/>.</returns>
        public static GroundTruthValidationStatus FromDisplayString(string displayString)
        {
            return displayString switch
            {
                "New" => GroundTruthValidationStatus.New,
                "New, Data Curated" => GroundTruthValidationStatus.NewDataCurated,
                "Validated" => GroundTruthValidationStatus.Validated,
                "Out of Scope" => GroundTruthValidationStatus.OutOfScope,
                "Revised" => GroundTruthValidationStatus.Revised,
                "Revisions Requested" => GroundTruthValidationStatus.RevisionsRequested,
                "Pending" => GroundTruthValidationStatus.Pending,
                _ => GroundTruthValidationStatus.New, // Default case
            };
        }
    }
}