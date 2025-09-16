namespace GroundTruthCuration.Core.Entities;

public class DataQueryDefinition
{
    public Guid DataQueryId { get; set; }
    public Guid GroundTruthId { get; set; }
    public string DatastoreType { get; set; } = string.Empty;
    public string DatastoreName { get; set; } = string.Empty;
    public string QueryTarget { get; set; } = string.Empty;
    public string QueryDefinition { get; set; } = string.Empty;
    public bool IsFullQuery { get; set; }
    public string RequiredPropertiesJson { get; set; } = string.Empty;
    public string UserCreated { get; set; } = string.Empty;
    public string UserUpdated { get; set; } = string.Empty;
    public DateTime CreationDateTime { get; set; }
    public DateTime StartDateTime { get; set; }
    public DateTime EndDateTime { get; set; }
}
