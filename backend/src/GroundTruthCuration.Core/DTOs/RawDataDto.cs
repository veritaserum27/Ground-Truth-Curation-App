namespace GroundTruthCuration.Core.DTOs;

public class RawDataDto
{
    public Guid DataQueryId { get; set; }
    public ICollection<Dictionary<string, object>> RawData { get; set; } = new List<Dictionary<string, object>>();
}