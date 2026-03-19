namespace GroundTruthCuration.Core.Constants
{
    /// <summary>
    /// Specifies the type of comment in the ground truth curation process.
    /// </summary>
    public enum GroundTruthCommentType
    {
        /// <summary>
        /// Represents a review comment.
        /// </summary>
        Review,
        /// <summary>
        /// Represents a general note or comment.
        /// </summary>
        Note
    }

    /// <summary>
    /// Provides extension methods for the GroundTruthCommentType enum.
    /// </summary>
    public static class GroundTruthCommentTypeExtensions
    {
        /// <summary>
        /// Converts the comment type to a display-friendly string.
        /// </summary>
        /// <param name="type">The comment type.</param>
        /// <returns>A string representation of the comment type.</returns>
        public static string ToDisplayString(this GroundTruthCommentType type)
        {
            return type switch
            {
                GroundTruthCommentType.Review => "Review",
                GroundTruthCommentType.Note => "Curator Note",
                _ => "Unknown"
            };
        }

        /// <summary>
        /// Parses a display string to its corresponding GroundTruthCommentType enum value.
        /// </summary>
        /// <param name="displayString">The display string.</param>
        /// <returns>The corresponding GroundTruthCommentType enum value.</returns>
        public static GroundTruthCommentType FromDisplayString(string displayString)
        {
            return displayString switch
            {
                "Review" => GroundTruthCommentType.Review,
                "Curator Note" => GroundTruthCommentType.Note,
                _ => throw new ArgumentException($"Unknown display string for GroundTruthCommentType: {displayString}")
            };
        }
    }
}