namespace GroundTruthCuration.Core.Entities;

public class Comment
{
    public Guid CommentId { get; set; }
    public Guid GroundTruthId { get; set; }
    public string CommentText { get; set; } = string.Empty;
    public DateTime CommentDateTime { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string CommentType { get; set; } = string.Empty;
    
    public GroundTruthDefinition GroundTruthDefinition { get; set; } = null!;
}
