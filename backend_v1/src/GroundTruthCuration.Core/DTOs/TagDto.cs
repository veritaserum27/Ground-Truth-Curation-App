namespace GroundTruthCuration.Core.DTOs;

public class TagDto
{
    public Guid TagId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
