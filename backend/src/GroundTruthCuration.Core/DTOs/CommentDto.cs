namespace GroundTruthCuration.Core.DTOs;

public class CommentDto
{
    public Guid CommentId { get; set; }
    public string CommentText { get; set; } = string.Empty;
    public string CommentType { get; set; } = string.Empty;
    public string UserCreated { get; set; } = string.Empty;
    public DateTime CreationDateTime { get; set; }
}
