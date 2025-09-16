using System.Diagnostics.Tracing;

namespace GroundTruthCuration.Core.Constants
{
    /// <summary>
    /// Represents the validation status of a ground truth entry.
    /// </summary>
    public enum ValidationStatus
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
    /// Provides extension methods for the ValidationStatus enum.
    /// </summary>
    public static class ValidationStatusExtensions
    {
        /// <summary>
        /// Converts the validation status to a display-friendly string.
        /// </summary>
        /// <param name="status">The validation status.</param>
        /// <returns>A string representation of the validation status.</returns>
        public static string ToDisplayString(this ValidationStatus status)
        {
            switch (status)
            {
                case ValidationStatus.New:
                    return "New";
                case ValidationStatus.NewDataCurated:
                    return "New, Data Curated";
                case ValidationStatus.Validated:
                    return "Validated";
                case ValidationStatus.OutOfScope:
                    return "Out of Scope";
                case ValidationStatus.Revised:
                    return "Revised";
                case ValidationStatus.RevisionsRequested:
                    return "Revisions Requested";
                case ValidationStatus.Pending:
                    return "Pending";
                default:
                    return string.Empty;
            }
        }
    }
}