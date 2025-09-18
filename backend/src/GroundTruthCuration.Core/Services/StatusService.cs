using GroundTruthCuration.Core.DTOs;
using GroundTruthCuration.Core.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;
namespace GroundTruthCuration.Core.Services;

public class StatusService : IStatusService
{
    private readonly IManufacturingDataDocDbRepository _docDbRepository;
    private readonly IManufacturingDataRelDbRepository _relDbRepository;
    private readonly IGroundTruthRepository _groundTruthRepository;

    public StatusService(IManufacturingDataDocDbRepository docDbRepository, IManufacturingDataRelDbRepository relDbRepository, IGroundTruthRepository groundTruthRepository)
    {
        _docDbRepository = docDbRepository ?? throw new ArgumentNullException(nameof(docDbRepository));
        _relDbRepository = relDbRepository ?? throw new ArgumentNullException(nameof(relDbRepository));
        _groundTruthRepository = groundTruthRepository ?? throw new ArgumentNullException(nameof(groundTruthRepository));
    }
    public async Task<ICollection<DatabaseStatusDto>> GetDatabaseStatusesAsync()
    {
        var statuses = new List<DatabaseStatusDto>();

        // Implementation to retrieve database status
        var docDbStatus = await _docDbRepository.GetStatusAsync();
        var relDbStatus = await _relDbRepository.GetStatusAsync();
        var groundTruthStatus = await _groundTruthRepository.GetStatusAsync();

        statuses.Add(docDbStatus);
        statuses.Add(relDbStatus);
        statuses.Add(groundTruthStatus);

        return statuses;
    }
}