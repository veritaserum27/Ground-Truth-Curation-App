namespace GroundTruthCuration.Core.Entities;

public class Tag
{
    public Guid TagId { get; set; }
    public string Name { get; set; } = string.Empty;
    
    public ICollection<GroundTruthDefinition> GroundTruthDefinitions { get; set; } = new List<GroundTruthDefinition>();
}
