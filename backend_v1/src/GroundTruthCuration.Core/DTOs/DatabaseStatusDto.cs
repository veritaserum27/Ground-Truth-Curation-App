namespace GroundTruthCuration.Core.DTOs;

public class DatabaseStatusDto
{
    public string ResourceName { get; set; } = string.Empty;
    public string DatabaseName { get; set; } = string.Empty;
    public string DatabaseType { get; set; } = string.Empty;
    public bool IsConnected { get; set; } = false;
    public DateTime LastChecked { get; set; } = DateTime.UtcNow;
}