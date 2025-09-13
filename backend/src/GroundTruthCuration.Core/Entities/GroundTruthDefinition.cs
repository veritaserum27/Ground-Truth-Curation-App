namespace GroundTruthCuration.Core.Entities;

public class GroundTruthDefinition
{
    public Guid GroundTruthId { get; set; }
    public string UserQuery { get; set; } = string.Empty;
    public string ValidationStatus { get; set; } = string.Empty;
    public string UserCreated { get; set; } = string.Empty;
    public string UserUpdated { get; set; } = string.Empty;
    public DateTime CreationDateTime { get; set; }
    public DateTime StartDateTime { get; set; }
    public DateTime EndDateTime { get; set; }
    
    public ICollection<GroundTruthEntry> GroundTruthEntries { get; set; } = new List<GroundTruthEntry>();
    public ICollection<DataQueryDefinition> DataQueryDefinitions { get; set; } = new List<DataQueryDefinition>();
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public ICollection<Tag> Tags { get; set; } = new List<Tag>();
}
