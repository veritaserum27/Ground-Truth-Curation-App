namespace GroundTruthCuration.Core.Entities;

public class GroundTruthEntry
{
    public Guid GroundTruthEntryId { get; set; }
    public Guid GroundTruthId { get; set; }
    public string Response { get; set; } = string.Empty;
    public string RequiredValuesJson { get; set; } = string.Empty;
    public string RawDataJson { get; set; } = string.Empty;
    public DateTime CreationDateTime { get; set; }
    public DateTime StartDateTime { get; set; }
    public DateTime EndDateTime { get; set; }
}
