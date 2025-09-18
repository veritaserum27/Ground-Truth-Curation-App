using System.ComponentModel.DataAnnotations.Schema;
using GroundTruthCuration.Core.Constants;

namespace GroundTruthCuration.Core.Entities;

/// <summary>
/// Represents a comment associated with a ground truth entry, including its type, author, and timestamp.
/// </summary>
public class Comment
{
    /// <summary>
    /// Gets or sets the unique identifier for the comment.
    /// </summary>
    public required Guid CommentId { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the identifier of the associated ground truth entry.
    /// </summary>
    public required Guid GroundTruthId { get; set; }

    /// <summary>
    /// Gets or sets the text content of the comment.
    /// </summary>
    public required string CommentText { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the comment was created (in UTC).
    /// </summary>
    public DateTime CommentDateTime { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the identifier of the user who created the comment.
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the type of the comment (e.g., review, note).
    /// </summary>
    public string CommentType { get; set; } = GroundTruthCommentType.Note.ToDisplayString();

    /// <summary>
    /// Gets the type of the comment as an enumeration.
    /// </summary>
    [NotMapped]
    public GroundTruthCommentType CommentTypeEnum
        => GroundTruthCommentTypeExtensions.FromDisplayString(CommentType);
}
