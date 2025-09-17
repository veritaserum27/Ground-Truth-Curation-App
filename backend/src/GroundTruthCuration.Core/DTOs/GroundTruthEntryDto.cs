namespace GroundTruthCuration.Core.DTOs;

public class GroundTruthEntryDto
{
    public Guid GroundTruthEntryId { get; set; }
    public Guid GroundTruthId { get; set; }
    public GroundTruthContextDto? groundTruthContextDto { get; set; } = null;
    public string Response { get; set; } = string.Empty;
    public ICollection<string> RequiredValues { get; set; } = new List<string>();
    public ICollection<RawDataDto> RawData { get; set; } = new List<RawDataDto>();
    public DateTime CreationDateTime { get; set; }
}
